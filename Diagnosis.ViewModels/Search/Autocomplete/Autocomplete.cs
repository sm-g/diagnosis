using Diagnosis.Common;
using Diagnosis.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;

namespace Diagnosis.ViewModels.Search.Autocomplete
{
    public class Autocomplete : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Autocomplete));
        private readonly bool allowTagConvertion;
        private Tag _selTag;
        private bool _popupOpened;
        private object prevSelectedSuggestion;
        private Tag _editingTag;
        private bool _supressCompletion;
        private object _selectedSuggestion;
        private Recognizer recognizer;
        private bool inDispose;

        public Autocomplete(Recognizer recognizer, bool allowTagConvertion, IEnumerable<IHrItemObject> initItems)
        {
            this.recognizer = recognizer;
            this.allowTagConvertion = allowTagConvertion;

            Tags = new ObservableCollection<Tag>();
            Tags.CollectionChanged += (s, e) =>
            {
                if (inDispose) return;

                logger.DebugFormat("{0} '{1}' '{2}'", e.Action, e.OldStartingIndex, e.NewStartingIndex);

                // кроме добавления пустого тега
                if (!(e.Action == NotifyCollectionChangedAction.Add && ((Tag)e.NewItems[0]).State == Tag.States.Init))
                    OnEntitiesChanged();
            };

            AddTag(isLast: true);

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
        /// Возникает, когда меняется набор сущностей в тегах. (Завершение редактирования, конвертация, удаление, cut, paste.)
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

        public RelayCommand<Tag> InverseEnterCommand
        {
            get
            {
                return new RelayCommand<Tag>(
                    (tag) => CompleteOnEnter(tag, true),
                    (tag) => tag != null && !recognizer.OnlyWords);
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
                return _selTag;
            }
            set
            {
                if (_selTag != value)
                {
                    _selTag = value;
                    if (value != null)
                    {
                        //    _selTag.IsFocused = true;
                        EditingTag = value;
                    }

                    OnPropertyChanged(() => SelectedTag);
                }
            }
        }

        private List<Tag> SelectedTags
        {
            get { return Tags.Where(t => t.IsSelected).ToList(); }
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

        public Tag LastTag
        {
            get
            {
                return Tags.LastOrDefault();
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

                    if (!value)
                    {
                        SelectedSuggestion = null; // нельзя выбирать предположения, когда попап скрыт
                    }

                    OnPropertyChanged(() => IsPopupOpen);
                }
            }
        }

        /// <summary>
        /// Тег завершается при потере фокуса.
        /// </summary>
        public bool CanCompleteOnLostFocus
        {
            get { return _supressCompletion; }
            set
            {
                _supressCompletion = value;
                logger.DebugFormat("CanCompleteOnLostFocus {0}", value);
            }
        }

        private bool _showALt;

        public bool ShowAltSuggestion
        {
            get
            {
                return _showALt;
            }
            set
            {
                if (_showALt != value)
                {
                    _showALt = value;
                    OnPropertyChanged(() => ShowAltSuggestion);
                }
            }
        }

        /// <summary>
        /// Добавляет тег в конец списка.
        /// </summary>
        public Tag AddTag(IHrItemObject item = null, bool isLast = false)
        {
            Tag tag;
            bool empty = item == null;
            if (empty)
                tag = new Tag(allowTagConvertion);
            else
                // для давления — искать связный тег и добавлять туда сущность
                tag = new Tag(item, allowTagConvertion);

            tag.Deleted += (s, e) =>
            {
                Tags.Remove(tag);
            };
            tag.Converting += (s, e) =>
            {
                CompleteOnConvert(tag);
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

                        if (tag.Signalization != Signalizations.None) // TODO предположения для недописанных
                        {
                            MakeSuggestions(SelectedTag);
                        }
                        else
                        {
                            Suggestions.Clear();
                        }

                        CanCompleteOnLostFocus = true;

                        SelectedTags.Except(tag.ToEnumerable()).ForAll(t => t.IsSelected = false);
                        tag.IsSelected = true;
                    }

                    // потерялся фокус после перехода не в предположения → завершение введенного текста
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

            if (isLast)
            {
                if (LastTag != null)
                    LastTag.IsLast = false;
                tag.IsLast = true;
            }

            if (empty)
                Tags.Add(tag);
            else
                Tags.Insert(Tags.Count - 1, tag);
            return tag;
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
        /// Возвращает сущности из тегов по порядку.
        /// </summary>
        public IEnumerable<IHrItemObject> GetEntities()
        {
            Contract.Requires(Tags.All(t => t.State != Tag.States.Typing));

            List<IHrItemObject> result = new List<IHrItemObject>();
            foreach (var tag in Tags)
            {
                if (tag.BlankType != Tag.BlankTypes.None)
                    foreach (var item in recognizer.EntitiesOf(tag)) // у давлния мб две сущности, возвращаем их отдельно
                    {
                        result.Add(item);
                    }
                else if (tag.State != Tag.States.Init)
                    logger.WarnFormat("{0} without entity blank, skip", tag);
            }

            return result;
        }

        public void Cut()
        {
            var hios = Copy();

            SelectedTags.ForAll(i => Tags.Remove(i));

            LogHrItemObjects("cut", hios);
        }

        public List<IHrItemObject> Copy()
        {
            var completed = SelectedTags.Where(t => t.State == Tag.States.Completed);
            var hios = SelectedTags
                 .SelectMany(t => recognizer.EntitiesOf(t))
                 .ToList();

            var data = new TagData() { ItemObjects = hios };
            var strings = string.Join(", ", hios);

            IDataObject dataObj = new DataObject(TagData.DataFormat.Name, data);
            dataObj.SetData(System.Windows.DataFormats.UnicodeText, strings);
            Clipboard.SetDataObject(dataObj, false);

            LogHrItemObjects("copy", hios);
            return hios;
        }

        public void Paste()
        {
            TagData data = null;

            var ido = Clipboard.GetDataObject();
            if (ido.GetDataPresent(TagData.DataFormat.Name))
            {
                data = (TagData)ido.GetData(TagData.DataFormat.Name);
            }
            if (data != null)
            {
                for (int i = 0; i < data.ItemObjects.Count; i++)
                {
                    Word word = data.ItemObjects[i] as Word;
                    if (word != null && word.IsTransient)
                    {
                        // копируем несохраненное слово - вставляем другое такое же
                        // word1.Equals(word2) == false, but word1.CompareTo(word2) == 0
                        // willSet in SetOrderedHrItems будет с первым совпадающим элементом в entitiesToBe
                        var same = GetEntities().Where(e => e is Word).Where(e => (e as Word).CompareTo(word) == 0).FirstOrDefault();
                        if (same != null)
                        {
                            data.ItemObjects[i] = same;
                        }
                    }
                }
                foreach (var item in data.ItemObjects)
                {
                    LogHrItemObjects("paste", data.ItemObjects);

                    AddTag(item);
                }
            }
        }

        /// <summary>
        /// Формат {[id] ToString()[,] ...}
        /// </summary>
        private void LogHrItemObjects(string action, IEnumerable<IHrItemObject> hios)
        {
            var str = hios.Select(item =>
            {
                dynamic entity = item;
                var pre = "";
                try
                {
                    pre = entity.Id.ToString();
                }
                catch
                {
                }
                return string.Format("{0} {1}", pre, item.ToString());
            });
            logger.DebugFormat("{0} hios: {1}", action, string.Join(", ", str));
        }

        /// <summary>
        /// Завершает тег.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="suggestion">Слово или запрос</param>
        /// <param name="exactMatchRequired">Требуется совпадение запроса и текста выбранного предположения.</param>
        private void CompleteCommon(Tag tag, object suggestion, bool exactMatchRequired, bool inverse = false)
        {
            if (tag.Query == "") // пустой тег — удаляем
            {
                tag.DeleteCommand.Execute(null);
            }
            else
            {
                recognizer.SetBlank(tag, suggestion, exactMatchRequired, inverse);
            }

            CompleteEnding(tag);
        }

        private void CompleteOnConvert(Tag tag)
        {
            Contract.Requires(!tag.Query.IsNullOrEmpty());

            recognizer.ConvertBlank(tag);

            CompleteEnding(tag);
        }

        public void CompleteOnEnter(Tag tag, bool inverse = false)
        {
            switch (tag.State)
            {
                case Tag.States.Init:
                    // Enter в пустом поле
                    OnInputEnded();
                    return;

                case Tag.States.Typing:
                    CompleteCommon(tag, SelectedSuggestion, false, inverse);
                    break;

                case Tag.States.Completed:
                    // тег не изменен
                    break;
            }

            // переходим к вводу нового слова
            SelectedTag = Tags.Last();
        }

        public void CompleteOnLostFocus(Tag tag)
        {
            if (tag.State == Tag.States.Typing)
            {
                logger.Debug("CompleteOnLostFocus");
                CompleteCommon(tag, prevSelectedSuggestion, true);
            }
        }

        private void CompleteEnding(Tag tag)
        {
            Suggestions.Clear();
            RefreshPopup();
            tag.Validate();

            // добавляем пустое поле
            if (LastTag.State != Tag.States.Init)
            {
                AddTag(isLast: true);
            }
        }

        private object MakeSuggestions(Tag tag)
        {
            var tagIndex = Tags.IndexOf(tag);
            var exclude = Tags.Select((t, i) => i != tagIndex ? t.Blank : null); // все сущности кроме сущности редактируемого тега

            var results = recognizer.SearchForSuggesstions(
                query: tag.Query,
                prevEntityBlank: tagIndex > 0 ? Tags[tagIndex - 1].Blank : null,
                exclude: null);

            Suggestions.Clear();
            foreach (var item in results)
            {
                Suggestions.Add(item);
            }

            SelectedSuggestion = Suggestions.FirstOrDefault();
            return Suggestions.FirstOrDefault();
        }

        private void RefreshPopup()
        {
            IsPopupOpen = Suggestions.Count > 0; // not on suggestion.collectionchanged - мигание при очистке, лишние запросы к IsPopupOpen
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

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(inDispose || LastTag.IsLast); // поле ввода по умолчанию
            Contract.Invariant(Tags.Count(t => t.State == Tag.States.Typing) <= 1); // только один тег редактируется
        }

        protected override void Dispose(bool disposing)
        {
            inDispose = true;
            if (disposing)
            {
                Tags.Clear();
            }
            base.Dispose(disposing);
        }
    }
}