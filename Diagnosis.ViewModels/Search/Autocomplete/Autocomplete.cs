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
        private Tag _selItem;
        private bool _popupOpened;
        private object prevSelectedSuggestion;
        private Tag _editingTag;
        private bool _supressCompletion;
        private object _selectedSuggestion;
        private Recognizer recognizer;

        public Autocomplete(Recognizer recognizer, bool allowTagEditing, IEnumerable<IHrItemObject> initItems)
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

        public RelayCommand<Tag> EnterCommand
        {
            get
            {
                return new RelayCommand<Tag>(
                    (tag) => CompleteOnEnter(tag),
                    (tag) => tag != null);
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
                    //  logger.DebugFormat("selected sugg = {0}", value);
                    OnPropertyChanged(() => SelectedSuggestion);
                }
            }
        }

        public ObservableCollection<Tag> Tags
        {
            get;
            set;
        }

        public Tag SelectedTag
        {
            get
            {
                return _selItem;
            }
            set
            {
                if (_selItem != value)
                {
                    _selItem = value;
                    if (value != null)
                    {
                        _selItem.IsFocused = true;
                        EditingTag = value;
                    }

                    OnPropertyChanged(() => SelectedTag);
                }
            }
        }

        /// <summary>
        /// При потере фокуса списком тегов SelectedTag будет null.
        /// </summary>
        public Tag EditingTag
        {
            get
            {
                return _editingTag;
            }
            set
            {
                if (_editingTag != value)
                {
                    _editingTag = value;
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
                    logger.DebugFormat("opened {0}", value);

                    OnPropertyChanged(() => IsPopupOpen);
                }
            }
        }

        /// <summary>
        /// Не завершать тег при переходе фокуса на попап.
        /// </summary>
        public bool CanCompleteOnLostFocus
        {
            get { return _supressCompletion; }
            set
            {
                _supressCompletion = value;
                logger.DebugFormat("supress {0}", value);
            }
        }

        /// <summary>
        /// Показывает, что можно редактировать теги после завершения.
        /// </summary>
        public bool IsEditable { get { return _isEditable; } }

        /// <summary>
        /// Добавляет тег в конец списка.
        /// </summary>
        public void AddTag(IHrItemObject item = null)
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
                    MakeSuggestions(EditingTag);
                    RefreshPopup();
                }
                else if (e.PropertyName == "IsFocused")
                {
                    if (tag.IsFocused)
                    {
                        prevSelectedSuggestion = SelectedSuggestion; // сначала фокус получает выбранный тег

                        if (tag.Signalization != Signalizations.None) // предположения для недописанных
                        {
                            MakeSuggestions(SelectedTag);
                        }
                        else
                        {
                            Suggestions.Clear();
                        }

                        CanCompleteOnLostFocus = true;
                    }

                    // потерялся фокус после перехода на другой тег → завершение введенного текста
                    if (!tag.IsFocused && CanCompleteOnLostFocus)
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

        public void ReplaceTagsWith(IEnumerable<IHrItemObject> items)
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
        public IEnumerable<IHrItemObject> GetEntities()
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
            RefreshPopup();
            tag.Validate();

            // добавляем пустое поле
            if (!IsLastTagEmpty)
                AddTag();
        }

        public void CompleteOnEnter(Tag tag)
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

        public void CompleteOnLostFocus(Tag tag)
        {
            if (tag.State == Tag.States.Typing)
            {
                logger.Debug("Complete from VM");
                CompleteCommon(tag, prevSelectedSuggestion, true);
            }
        }

        private void MakeSuggestions(Tag tag)
        {
            var tagIndex = Tags.IndexOf(tag);
            var results = recognizer.SearchForSuggesstions(
                query: tag.Query,
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