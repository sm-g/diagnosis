using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Diagnosis.ViewModels.Search.Autocomplete
{
    public class Autocomplete : ViewModelBase
    {
        private readonly bool _isEditable;
        Tag _editingItem;
        bool _popupOpened;
        object _selectedSuggestion;
        Recognizer recognizer;
        object prevSelectedSuggestion;

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
        /// Возникает, когда работа с автокомплитом окончена. (Enter второй раз.)
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
                return _selectedSuggestion;
            }
            set
            {
                if (_selectedSuggestion != value)
                {
                    _selectedSuggestion = value;
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

                    Debug.Print("editing = {0}", value);
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
                    // предположения для недописанных слов
                    if (tag.IsFocused && (tag.EntityBlank is string || tag.EntityBlank == null))
                    {
                        prevSelectedSuggestion = SelectedSuggestion; // сначала фокус получает выбранный тег
                        MakeSuggestions();
                    }

                    // потерялся фокус → завершение введенного текста
                    if (!tag.IsFocused && tag.State == TagStates.Typing)
                        CompleteOnLostFocus(tag);
                    }

                    RefreshPopup();
                }
            };

            if (empty)
                Tags.Add(tag);
            else
                Tags.Insert(Tags.Count - 1, tag);
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
                    foreach (var item in recognizer.MakeEntities(tag.EntityBlank))
                    {
                        yield return item;
                    }
                else if (tag.State != TagStates.Init)
                    Debug.Print("tag without entity blank, skip");
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
        private void CompleteCommon(Tag tag, object suggestion, bool exactMatchRequired)
        {
            if (suggestion != null &&
               (!exactMatchRequired || suggestion.ToString() == tag.Query))
            {
                // записывам выбранное из попапа в тег
                tag.EntityBlank = suggestion;
            }
            else if (recognizer.CanMakeEntityFrom(tag.Query))
            {
                // записываем введенный текст
                tag.EntityBlank = tag.Query;
            }
            else
            {
                // тег ещё не готов - убираем заготовку, если она была создана
                tag.EntityBlank = null;
            }
            Suggestions.Clear();

            // удаляем тег без текста
            if (tag.Query == "")
                tag.DeleteCommand.Execute(null);

            // добавляем пустое поле
            if (!IsLastTagEmpty)
                AddTag();
        }

        private void CompleteOnEnter(Tag tag)
        {
            switch (tag.State)
            {
                case TagStates.Init:
                    // Enter второй раз (в пустом поле)
                    OnInputEnded();
                    return;
                case TagStates.Typing:
                    CompleteCommon(tag, SelectedSuggestion, false);
                    break;
                case TagStates.Completed:
                    break;
            }

            // переходим к вводу нового слова
            Tags.Last().IsFocused = true;
        }

        private void CompleteOnLostFocus(Tag tag)
        {
            Contract.Requires(tag.State == TagStates.Typing);

            CompleteCommon(tag, prevSelectedSuggestion, true);
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
