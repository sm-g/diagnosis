using Diagnosis.Common;
using Diagnosis.Models;
using GongSolutions.Wpf.DragDrop;
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
        private bool _reorder;
        private bool _showALt;
        private Recognizer recognizer;
        private bool inDispose;

        public Autocomplete(Recognizer recognizer, bool allowTagConvertion, IEnumerable<IHrItemObject> initItems)
        {
            this.recognizer = recognizer;
            this.allowTagConvertion = allowTagConvertion;

            DropHandler = new Autocomplete.DropTargetHandler(this);
            DragHandler = new Autocomplete.DragSourceHandler();
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
                    if (value == null)
                        prevSelectedSuggestion = _selectedSuggestion;

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
        /// Switch focus between textbox and listitem for SelectedTag
        /// </summary>
        public RelayCommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (SelectedTag != null)
                        if (SelectedTag.IsTextBoxFocused)
                        {
                            SelectedTag.IsListItemFocused = true;
                        }
                        else
                        {
                            SelectedTag.IsTextBoxFocused = true;
                        }
                });
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

        public bool WithReorder
        {
            get
            {
                return _reorder;
            }
            set
            {
                if (_reorder != value)
                {
                    _reorder = value;
                    OnPropertyChanged(() => WithReorder);
                }
            }
        }

        public DropTargetHandler DropHandler { get; private set; }
        public DragSourceHandler DragHandler { get; private set; }

        /// <summary>
        /// Создает тег.
        /// </summary>
        /// <param name="сontent">Строка запроса, IHrsItemObject или null для пустого тега.</param>
        public Tag CreateTag(object content = null)
        {
            Tag tag;
            var itemObject = content as IHrItemObject;
            var str = content as string;

            if (itemObject != null)
                // для давления — искать связный тег и добавлять туда сущность
                tag = new Tag(this, itemObject, allowTagConvertion);
            else if (str != null)
                tag = new Tag(this, str, allowTagConvertion);
            else
                tag = new Tag(this, allowTagConvertion);

            tag.Deleted += (s, e) =>
            {
                Contract.Requires(!tag.IsLast);
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
                else if (e.PropertyName == "IsTextBoxFocused")
                {
                    if (tag.IsTextBoxFocused)
                    {
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
                    if (!tag.IsTextBoxFocused && CanCompleteOnLostFocus)
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
            return tag;
        }

        /// <summary>
        /// Добавляет тег в коллекцию.
        /// <param name="tagOrContent">Созданный тег, строка запроса, IHrsItemObject или null для пустого тега.</param>
        /// </summary>
        public Tag AddTag(object tagOrContent = null, int index = -1, bool isLast = false)
        {
            var tag = tagOrContent as Tag;
            if (tag == null)
            {
                tag = CreateTag(tagOrContent);
            }

            if (isLast)
            {
                if (LastTag != null)
                    LastTag.IsLast = false;
                tag.IsLast = true;
            }

            if (index < 0 || index > Tags.Count - 1)
                index = Tags.Count - 1; // перед последним
            if (isLast)
                index = Tags.Count;

            Tags.Insert(index, tag);
            return tag;
        }

        public void Add(Tag from, bool left)
        {
            var tag = AddTag(index: Tags.IndexOf(from) + (left ? 0 : 1));
            tag.IsTextBoxFocused = true;
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

            var completed = SelectedTags.Where(t => t.State == Tag.States.Completed); // do not remove init tags
            completed.ForAll(i => Tags.Remove(i));

            LogHrItemObjects("cut", hios);
        }

        public List<IHrItemObject> Copy()
        {
            var completed = SelectedTags.Where(t => t.State == Tag.States.Completed);
            var hios = completed
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
                var index = Tags.IndexOf(SelectedTag); // paste before first SelectedTag

                SelectedTags.ForEach(t => t.IsSelected = false);

                foreach (var item in data.ItemObjects)
                {
                    LogHrItemObjects("paste", data.ItemObjects);

                    var tag = AddTag(item, index++);
                    tag.IsSelected = true;
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
            if (tag.Query == "")
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
            LastTag.IsTextBoxFocused = true;
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
            if (LastTag.State == Tag.States.Completed)
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

        public class DropTargetHandler : DefaultDropHandler
        {
            private readonly Autocomplete master;

            public DropTargetHandler(Autocomplete master)
            {
                this.master = master;
            }

            public bool FromSameAutocomplete(IDropInfo dropInfo)
            {
                if (dropInfo.DragInfo == null || dropInfo.DragInfo.SourceCollection == null)
                    return false;
                var sourceList = GetList(dropInfo.DragInfo.SourceCollection);
                return sourceList == master.Tags;
            }

            public bool FromOtherAutocomplete(IDropInfo dropInfo)
            {
                if (dropInfo.DragInfo == null || dropInfo.DragInfo.SourceCollection == null)
                    return false;
                var sourceList = GetList(dropInfo.DragInfo.SourceCollection);
                return sourceList is IEnumerable<Tag>;
            }

            public override void DragOver(IDropInfo dropInfo)
            {
                var destinationList = GetList(dropInfo.TargetCollection);
                if (FromSameAutocomplete(dropInfo))
                {
                    dropInfo.Effects = DragDropEffects.Move;
                }
                else if (FromOtherAutocomplete(dropInfo))
                {
                    dropInfo.Effects = DragDropEffects.Copy;
                }
                else
                {
                    dropInfo.Effects = DragDropEffects.Scroll;
                }
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            }

            public override void Drop(IDropInfo dropInfo)
            {
                var data = ExtractData(dropInfo.Data).Cast<object>();
                if (data.Count() == 0)
                    return;

                logger.DebugFormat("ddrop {0} {1}", data.Count(), data.First().GetType());

                var insertIndex = dropInfo.InsertIndex;
                if (data.First() is Tag)
                {
                    // drop tags from autocomplete
                    var tags = data.Cast<Tag>();

                    if (FromSameAutocomplete(dropInfo))
                    {
                        // reorder tags

                        foreach (var tag in tags)
                        {
                            var old = master.Tags.IndexOf(tag);
                            //master.Tags.RemoveAt(old);
                            //if (old < insertIndex)
                            //{
                            //    --insertIndex;
                            //}
                            var n = old < insertIndex ? insertIndex - 1 : insertIndex;

                            // not after last
                            if (n == master.Tags.IndexOf(master.LastTag))
                                n--;

                            if (old != n) // prevent deselecting
                                master.Tags.Move(old, n);
                        }
                        //foreach (var tag in tags)
                        //{
                        //    master.Tags.Insert(insertIndex, tag);
                        //}
                    }
                    else if (FromOtherAutocomplete(dropInfo))
                    {
                        // copy tags' HrItemObjects

                        foreach (var tag in tags)
                        {
                            var items = master.recognizer.EntitiesOf(tag).ToList();
                            foreach (var item in items)
                            {
                                master.AddTag(item);
                            }
                        }
                    }

                    master.LastTag.IsSelected = false;
                }
            }
        }

        public class DragSourceHandler : IDragSource
        {
            public void StartDrag(IDragInfo dragInfo)
            {
                var tags = dragInfo.SourceItems.Cast<Tag>().Where(t => !t.IsLast);
                var itemCount = tags.Count();

                if (itemCount == 1)
                {
                    dragInfo.Data = tags.First();
                }
                else if (itemCount > 1)
                {
                    dragInfo.Data = GongSolutions.Wpf.DragDrop.Utilities.TypeUtilities.CreateDynamicallyTypedList(tags);
                }

                dragInfo.Effects = (dragInfo.Data != null) ?
                                     DragDropEffects.Copy | DragDropEffects.Move :
                                     DragDropEffects.None;
            }

            public bool CanStartDrag(IDragInfo dragInfo)
            {
                var tags = dragInfo.SourceItems.Cast<Tag>().Where(t => !t.IsLast);
                return tags.Count() > 0;
            }

            public void DragCancelled()
            {
            }

            public void Dropped(IDropInfo dropInfo)
            {
            }


        }

        public void OnDrop(DragEventArgs e)
        {
            logger.DebugFormat("drop {0}", e.Data.ToString());

            string text = null;
            string unicodeText = null;

            // prefer unicode format
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                text = (string)e.Data.GetData(DataFormats.Text);
            }
            if (e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                unicodeText = (string)e.Data.GetData(DataFormats.UnicodeText);
            }
            if (unicodeText != null)
                text = unicodeText;

            if (text != null)
            {
                // drop strings - make tag with query and complete it

                var strings = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var str in strings)
                {
                    var tag = CreateTag(str);
                    CompleteCommon(tag, null, true);
                    AddTag(tag);
                }
            }
        }
    }
}