using Diagnosis.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Search.Autocomplete
{
    public class Autocomplete : ViewModelBase
    {
        public static readonly ILog logger = LogManager.GetLogger(typeof(Autocomplete));
        private readonly bool _isEditable;
        private Tag _editingItem;
        private bool _popupOpened;
        private object _selectedSuggestion;
        private Recognizer recognizer;
        private object prevSelectedSuggestion;

        public Autocomplete(Recognizer recognizer, bool allowTagEditing, IEnumerable<IDomainEntity> initItems)
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

        /// <summary>
        /// Возникает, когда завершается редактирование тега.
        /// </summary>
        public event EventHandler<TagEventArgs> TagCompleted;

        /// <summary>
        /// Возникает, когда меняется набор сущностей в тегах. (Завершение редактрования теги или удаление тега.)
        /// </summary>
        public event EventHandler EntitiesChanged;

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
                    logger.DebugFormat("selected sugg = {0}", value);
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

                    OnPropertyChanged(() => EditingTag);
                }
            }
        }

        public bool IsLastTagEmpty
        {
            get
            {
                return Tags.Last().State == Tag.States.Init;
            }
        }

        public bool IsPopupOpen
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
                    OnPropertyChanged(() => IsPopupOpen);
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
        public void AddTag(IDomainEntity item = null)
        {
            Tag tag;
            bool empty = item == null;
            if (empty)
                tag = new Tag(!IsEditable);
            else
                tag = new Tag(item, !IsEditable);

            tag.Deleted += (s, e) =>
            {
                Tags.Remove(tag);
                OnEntitiesChanged();
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
                    if (tag.IsFocused)
                    {
                        prevSelectedSuggestion = SelectedSuggestion; // сначала фокус получает выбранный тег

                        if (tag.Signalization != Signalizations.None) // предположения для недописанных
                            MakeSuggestions();
                    }

                    // потерялся фокус → завершение введенного текста
                    if (!tag.IsFocused && tag.State == Tag.States.Typing)
                    {
                        CompleteOnLostFocus(tag);
                    }

                    RefreshPopup();
                }
                else if (e.PropertyName == "State")
                {
                    if (tag.State == Tag.States.Completed)
                    {
                        OnTagCompleted(tag);
                        OnEntitiesChanged();
                    }
                }
            };

            if (empty)
                Tags.Add(tag);
            else
                Tags.Insert(Tags.Count - 1, tag);
        }

        public void ReplaceTagsWith(IEnumerable<IDomainEntity> items)
        {
            // оставляем последний тег
            while (Tags.Count != 1)
            {
                Tags.RemoveAt(Tags.Count - 2);
            }

            foreach (var item in items)
            {
                AddTag(item);
            }
        }

        /// <summary>
        /// Возвращает сущности из тегов.
        /// </summary>
        public IEnumerable<IDomainEntity> GetEntities()
        {
            Contract.Requires(Tags.All(t => t.State != Tag.States.Typing));

            foreach (var tag in Tags)
            {
                if (tag.BlankType != Tag.BlankTypes.None)
                    foreach (var item in recognizer.MakeEntities(tag))
                    {
                        yield return item;
                    }
                else if (tag.State != Tag.States.Init)
                    logger.WarnFormat("{0} without entity blank, skip", tag);
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

        protected virtual void OnTagCompleted(Tag tag)
        {
            var h = TagCompleted;
            if (h != null)
            {
                h(this, new TagEventArgs(tag));
            }
        }

        protected virtual void OnEntitiesChanged()
        {
            var h = EntitiesChanged;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Завершает тег.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="suggestion">Слово, число с единицей или запрос</param>
        /// <param name="exactMatchRequired">Требуется совпадение запроса и текста выбранного предположения.</param>
        private void CompleteCommon(Tag tag, object suggestion, bool exactMatchRequired)
        {
            if (suggestion == null)
            {
                if (tag.Query == "")
                    tag.DeleteCommand.Execute(null);
                else if (recognizer.CanMakeEntityFrom(tag.Query))
                    tag.Blank = tag.Query; // измерение без правльной единицы
                else
                    tag.Blank = null;
            }
            else
            {
                if (!exactMatchRequired || Recognizer.Matches(suggestion, tag.Query))
                    tag.Blank = suggestion;
                else if (recognizer.CanMakeEntityFrom(tag.Query))
                    tag.Blank = tag.Query; // недописанное слово
                else
                    tag.Blank = null;
            }

            Suggestions.Clear();

            // добавляем пустое поле
            if (!IsLastTagEmpty)
                AddTag();
        }

        private void CompleteOnEnter(Tag tag)
        {
            switch (tag.State)
            {
                case Tag.States.Init:
                    // Enter в пустом поле
                    OnInputEnded();
                    return;

                case Tag.States.Typing:
                    CompleteCommon(tag, SelectedSuggestion, false);
                    break;

                case Tag.States.Completed:
                    // тег не изменен
                    break;
            }

            // переходим к вводу нового слова
            Tags.Last().IsFocused = true;
        }

        private void CompleteOnLostFocus(Tag tag)
        {
            Contract.Requires(tag.State == Tag.States.Typing);

            CompleteCommon(tag, prevSelectedSuggestion, true);
        }

        private void MakeSuggestions()
        {
            var tagIndex = Tags.IndexOf(EditingTag);
            var results = recognizer.SearchForSuggesstions(
                query: EditingTag.Query,
                prevEntityBlank: tagIndex > 0 ? Tags[tagIndex - 1].Blank : null,
                exclude: Tags.Select((t, i) => i != tagIndex ? t.Blank : null)); // все сущности кроме сущности редактируемого тега

            Suggestions.Clear();
            foreach (var item in results)
            {
                Suggestions.Add(item);
            }

            SelectedSuggestion = Suggestions.FirstOrDefault();
        }

        private void RefreshPopup()
        {
            IsPopupOpen = Suggestions.Count > 0; // not on suggestion.collectionchanged - мигание при очистке, лишние запросы к IsPopupOpen
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(Tags.Count > 0); // хотя бы один тег - поле ввода
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Tags.Clear();
            }
            base.Dispose(disposing);
        }
    }
}