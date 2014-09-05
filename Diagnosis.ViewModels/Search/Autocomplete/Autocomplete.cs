using Remotion.Linq.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels.Search.Autocomplete
{
    public class Autocomplete : ViewModelBase
    {
        private readonly bool _isEditable;
        Tag _editingItem;
        bool _popupOpened;
        object _selectedPopupItem;
        Recognizer recognizer;

        public Autocomplete(Recognizer recognizer, bool allowTagEditing, object[] initItems)
        {
            this.recognizer = recognizer;
            this._isEditable = allowTagEditing;

            Tags = new ObservableCollection<Tag>();
            AddTag();

            if (initItems != null)
            {
                foreach (var item in initItems)
                {
                    AddTag(item);
                }
            }
            Suggestions = new ObservableCollection<object>();
        }
        /// <summary>
        /// Возникает, когда работа с автокомплитом окончена.
        /// </summary>
        public event EventHandler InputEnded;
        public RelayCommand EnterCommand
        {
            get
            {
                return new RelayCommand(() => CompleteOnEnter(EditingTag), () => EditingTag != null);
            }
        }
        public ObservableCollection<object> Suggestions
        {
            get;
            set;
        }

        public object SelectedSuggestion
        {
            get
            {
                return _selectedPopupItem;
            }
            set
            {
                if (_selectedPopupItem != value)
                {
                    _selectedPopupItem = value;
                    // Console.WriteLine("selected = {0}", value);
                    OnPropertyChanged(() => SelectedSuggestion);
                }
            }
        }

        public ObservableCollection<Tag> Tags
        {
            get;
            set;
        }

        public Tag EditingTag
        {
            get
            {
                return _editingItem;
            }
            set
            {
                if (_editingItem != value)
                {
                    _editingItem = value;
                    if (value != null)
                        _editingItem.IsFocused = true;

                    Console.WriteLine("editing = {0}", value);
                    OnPropertyChanged(() => EditingTag);
                }
            }
        }

        public bool IsLastTagEmpty
        {
            get
            {
                return Tags.Last().State == TagStates.Init;
            }
        }

        public bool IsPopupOpened
        {
            get
            {
                return _popupOpened;
            }
            set
            {
                if (_popupOpened != value)
                {
                    _popupOpened = value;
                    OnPropertyChanged(() => IsPopupOpened);
                }
            }
        }
        /// <summary>
        /// Показывает, что можно редактировать теги после завершения.
        /// </summary>
        public bool IsEditable { get { return _isEditable; } }

        /// <summary>
        /// Добавляет тег в конец списка.
        /// </summary>
        /// <param name="item">Сущность или null.</param>
        public void AddTag(object item = null)
        {
            Contract.Requires(item == null || !(item is string));

            Tag tag;
            bool empty = item == null;
            if (empty)
                tag = new Tag(!IsEditable);
            else
                tag = new Tag(item, !IsEditable);

            tag.Deleted += (s, e) =>
            {
                Tags.Remove(tag);
            };
            tag.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Query")
                {
                    MakeSuggestions();
                    RefreshPopup();
                }
                else if (e.PropertyName == "IsFocused")
                {
                    if (tag.IsFocused && (tag.Query != null || tag.EntityBlank is string)) // попап для недописанных слов
                        MakeSuggestions();

                    // потерялся фокус → завершение введенного текста
                    if (!tag.IsFocused && tag.State == TagStates.Typing)
                        CompleteOnLostFocus(tag);

                    RefreshPopup();
                }
            };

            if (empty)
                Tags.Add(tag);
            else
                Tags.Insert(Tags.Count - 2, tag);
        }

        /// <summary>
        /// Возвращает сущности из тегов.
        /// </summary>
        public IEnumerable<object> GetItems()
        {
            Contract.Requires(Tags.All(t => t.State != TagStates.Typing));

            foreach (var tag in Tags)
            {
                if (tag.EntityBlank != null)
                    foreach (var item in recognizer.MakeEntities(tag))
                    {
                        yield return item;
                    }

                if (tag.State != TagStates.Init)
                    Console.WriteLine("tag without entity blank, skip");
            }
        }

        protected virtual void OnInputEnded()
        {
            var h = InputEnded;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="exactMatchRequired">Требуется совпадение запроса и текста выбранной сущности.</param>
        private void CompleteCommon(Tag tag, bool exactMatchRequired)
        {
            if (SelectedSuggestion != null &&
               (!exactMatchRequired || SelectedSuggestion.ToString() == tag.Query))
            {
                // записывам выбранное из попапа в тег
                tag.EntityBlank = SelectedSuggestion;
            }
            else if (recognizer.CanMakeEntityFrom(tag.Query))
            {
                // записываем введенный текст
                tag.EntityBlank = tag.Query;
            }
            else
            {
                // убираем бывшую заготовку
                tag.EntityBlank = null;
            }
            Suggestions.Clear();

            // удаляем тег без текста
            if (tag.Query == "")
                tag.DeleteCommand.Execute(null);
        }

        private void CompleteOnEnter(Tag tag)
        {
            if (tag.State == TagStates.Init)
            {
                // Enter второй раз (в пустом поле)
                OnInputEnded();
                return;
            }
            CompleteCommon(tag, false);

            // добавляем пустое поле
            if (!IsLastTagEmpty)
                AddTag();

            // переходим к вводу нового слова
            Tags.Last().IsFocused = true;
        }

        private void CompleteOnLostFocus(Tag tag)
        {
            Contract.Requires(tag.State == TagStates.Typing);

            CompleteCommon(tag, true);
        }

        private void MakeSuggestions()
        {
            var tagIndex = Tags.IndexOf(EditingTag);
            var results = recognizer.Search(
                EditingTag.Query,
                tagIndex > 0 ? Tags[tagIndex - 1].EntityBlank : null,
                Tags.Select((t, i) => i != tagIndex ? t.EntityBlank : null));

            Suggestions.Clear();
            foreach (var item in results)
            {
                Suggestions.Add(item);
            }

            SelectedSuggestion = Suggestions.FirstOrDefault();
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(Tags.Count > 0);
        }

        private void RefreshPopup()
        {
            IsPopupOpened = Suggestions.Count > 0;
        }
    }
}
