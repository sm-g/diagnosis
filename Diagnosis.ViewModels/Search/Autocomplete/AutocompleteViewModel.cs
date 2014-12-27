using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Screens;
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
    public class AutocompleteViewModel : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AutocompleteViewModel));
        private readonly Recognizer recognizer;
        private readonly bool allowSendToSearch;
        private readonly bool allowTagConvertion;
        private readonly bool singleTag;

        private TagViewModel _selTag;
        private bool _popupOpened;
        private object prevSelectedSuggestion;
        private TagViewModel _editingTag;
        private bool _supressCompletion;
        private object _selectedSuggestion;
        private bool _showALt;
        private EventAggregator.EventMessageHandler hanlder;
        private bool inDispose;

        public AutocompleteViewModel(Recognizer recognizer, bool allowTagConvertion, bool allowSendToSearch, bool singleTag, IEnumerable<IHrItemObject> initItems)
        {
            this.recognizer = recognizer;
            this.allowTagConvertion = allowTagConvertion;
            this.allowSendToSearch = allowSendToSearch;
            this.singleTag = singleTag;

            DropHandler = new AutocompleteViewModel.DropTargetHandler(this);
            DragHandler = new AutocompleteViewModel.DragSourceHandler();
            Tags = new ObservableCollection<TagViewModel>();
            Tags.CollectionChanged += (s, e) =>
            {
                if (inDispose) return;

                logger.DebugFormat("{0} '{1}' '{2}'", e.Action, e.OldStartingIndex, e.NewStartingIndex);

                // кроме добавления пустого тега
                if (!(e.Action == NotifyCollectionChangedAction.Add && ((TagViewModel)e.NewItems[0]).State == TagViewModel.States.Init))
                    OnEntitiesChanged();
            };
            hanlder = this.Subscribe(Events.WordPersisted, (e) =>
            {
                // созданные слова можно искать из поиска
                var word = e.GetValue<Word>(MessageKeys.Word);
                Tags.Where(t => t.Entities != null && t.Entities.Contains(word))
                    .ForAll(t => t.Validate());
            });

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

        public RelayCommand<TagViewModel> EnterCommand
        {
            get
            {
                return new RelayCommand<TagViewModel>(
                    (tag) => CompleteOnEnter(tag),
                    (tag) => tag != null);
            }
        }

        public RelayCommand<TagViewModel> InverseEnterCommand
        {
            get
            {
                return new RelayCommand<TagViewModel>(
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

        public ObservableCollection<TagViewModel> Tags
        {
            get;
            set;
        }

        public TagViewModel SelectedTag
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
                        _selTag.IsSelected = true;
                    }

                    OnPropertyChanged(() => SelectedTag);
                }
            }
        }

        private List<TagViewModel> SelectedTags
        {
            get { return Tags.Where(t => t.IsSelected).ToList(); }
        }
        public RelayCommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (SelectedTag != null)
                    {
                        switch (SelectedTag.BlankType)
                        {
                            case TagViewModel.BlankTypes.Measure:
                                var vm = new MeasureEditorViewModel(SelectedTag.Entities.First() as Measure);
                                this.Send(Events.OpenDialog, vm.AsParams(MessageKeys.Dialog));
                                if (vm.DialogResult == true)
                                {
                                    CompleteCommon(SelectedTag, vm.Measure, false);
                                }
                                break;

                            case TagViewModel.BlankTypes.Icd:
                                var vm0 = new IcdSelectorViewModel(SelectedTag.Entities.First() as IcdDisease);
                                this.Send(Events.OpenDialog, vm0.AsParams(MessageKeys.Dialog));
                                if (vm0.DialogResult == true)
                                {
                                    CompleteCommon(SelectedTag, vm0.SelectedIcd, false);
                                }
                                break;

                            default:
                                SelectedTag.SwitchEdit();
                                break;
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Показывает предположения для редактируемого тега.
        /// </summary>
        public RelayCommand ShowSuggestionsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    MakeSuggestions(SelectedTag);
                    RefreshPopup();
                }, () => SelectedTag != null && SelectedTag.IsTextBoxFocused);
            }
        }

        public RelayCommand<TagViewModel> SendToSearchCommand
        {
            get
            {
                return new RelayCommand<TagViewModel>((t) =>
                {
                    IEnumerable<IHrItemObject> entities;
                    if (t != null)
                        entities = recognizer.EntitiesOf(t);
                    else
                        entities = GetEntitiesOfSelected();
                    this.Send(Events.SendToSearch, entities.AsParams(MessageKeys.HrItemObjects));
                }, (t) => WithSendToSearch);
            }
        }

        public RelayCommand DeleteCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SelectedTags.ForEach(t => t.DeleteCommand.Execute(null));
                });
            }
        }

        /// <summary>
        /// При потере фокуса списком тегов SelectedTag будет null.
        /// </summary>
        public TagViewModel EditingTag
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

        public TagViewModel LastTag
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

        /// <summary>
        /// Автокомплит с единственным тегом (не IsLast). Добавление новых заменяет его.
        /// </summary>
        public bool SingleTag
        {
            get { return singleTag; }
        }

        public bool WithSendToSearch
        {
            get
            {
                return allowSendToSearch;
            }
        }

        public bool WithConvert
        {
            get
            {
                return allowTagConvertion;
            }
        }

        public DropTargetHandler DropHandler { get; private set; }

        public DragSourceHandler DragHandler { get; private set; }

        /// <summary>
        /// Создает тег.
        /// </summary>
        /// <param name="сontent">Строка запроса, IHrsItemObject или null для пустого тега.</param>
        public TagViewModel CreateTag(object content = null)
        {
            TagViewModel tag;
            var itemObject = content as IHrItemObject;
            var str = content as string;

            if (itemObject != null)
                // для давления — искать связный тег и добавлять туда сущность
                tag = new TagViewModel(this, itemObject);
            else if (str != null)
                tag = new TagViewModel(this, str);
            else
                tag = new TagViewModel(this);

            tag.Deleted += (s, e) =>
            {
                Contract.Requires(!tag.IsLast);
                Tags.Remove(tag);
                LastTag.IsTextBoxFocused = true;
            };
            tag.Converting += (s, e) =>
            {
                CompleteOnConvert(tag, e.type);
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
                        if (tag.Signalization == null || tag.Signalization == Signalizations.None)
                        {
                            Suggestions.Clear();
                        }
                        else
                        {
                            MakeSuggestions(SelectedTag); // предположения для недописанных, новых
                        }

                        CanCompleteOnLostFocus = true;

                        SelectedTags.Except(tag.ToEnumerable()).ForAll(t => t.IsSelected = false);
                        tag.IsSelected = true;
                    }
                    else
                    {
                        // выход из редактирования
                        Suggestions.Clear();

                        // потерялся фокус после перехода не в предположения → завершение введенного текста
                        if (CanCompleteOnLostFocus)
                        {
                            CompleteOnLostFocus(tag);
                        }
                    }

                    RefreshPopup();
                }
                else if (e.PropertyName == "State")
                {
                    if (tag.State == TagViewModel.States.Completed)
                    {
                        OnTagCompleted(tag);
                        OnEntitiesChanged();
                    }
                }
                else if (e.PropertyName == "IsSelected")
                {
                    tag.IsDraggable = tag.IsSelected;
                }
            };
            return tag;
        }

        /// <summary>
        /// Добавляет тег в коллекцию.
        /// <param name="tagOrContent">Созданный тег, строка запроса, IHrsItemObject или null для пустого тега.</param>
        /// </summary>
        public TagViewModel AddTag(object tagOrContent = null, int index = -1, bool isLast = false)
        {
            var tag = tagOrContent as TagViewModel;
            if (tag == null)
            {
                tag = CreateTag(tagOrContent);
            }

            if (isLast)
            {
                if (LastTag != null)
                    LastTag.IsLast = false;
                tag.IsLast = true && !SingleTag;
            }

            if (index < 0 || index > Tags.Count - 1)
                index = Tags.Count - 1; // перед последним
            if (isLast)
                index = Tags.Count;

            if (SingleTag && Tags.Count > 0)
                Tags[0] = tag;
            else
                Tags.Insert(index, tag);
            return tag;
        }

        public void Add(TagViewModel from, bool left)
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
                AddTag(item).Validate(Validator);
            }
        }

        /// <summary>
        /// Возвращает сущности из тегов по порядку.
        /// </summary>
        public IEnumerable<IHrItemObject> GetEntities()
        {
            Contract.Requires(Tags.All(t => t.State != TagViewModel.States.Typing));

            List<IHrItemObject> result = new List<IHrItemObject>();
            foreach (var tag in Tags)
            {
                if (tag.BlankType != TagViewModel.BlankTypes.None)
                    foreach (var item in recognizer.EntitiesOf(tag)) // у давлния мб две сущности, возвращаем их отдельно
                    {
                        result.Add(item);
                    }
                else if (tag.State != TagViewModel.States.Init)
                    logger.WarnFormat("{0} without entity blank, skip", tag);
            }

            return result;
        }

        public void CutSelected()
        {
            var hios = CopySelected();

            var completed = SelectedTags.Where(t => t.State == TagViewModel.States.Completed); // do not remove init tags
            completed.ForAll(t => t.DeleteCommand.Execute(null));

            LogHrItemObjects("cut", hios);
        }

        public List<IHrItemObject> CopySelected()
        {
            var hios = GetEntitiesOfSelected();
            var data = new TagData() { ItemObjects = hios };
            var strings = string.Join(", ", hios);

            IDataObject dataObj = new DataObject(TagData.DataFormat.Name, data);
            dataObj.SetData(System.Windows.DataFormats.UnicodeText, strings);
            Clipboard.SetDataObject(dataObj, false);

            LogHrItemObjects("copy", hios);
            return hios;
        }

        private List<IHrItemObject> GetEntitiesOfSelected()
        {
            var completed = SelectedTags.Where(t => t.State == TagViewModel.States.Completed);
            var hios = completed
                 .SelectMany(t => recognizer.EntitiesOf(t))
                 .ToList();
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
                var index = Tags.IndexOf(SelectedTag); // paste before first SelectedTag
                SelectedTags.ForEach(t => t.IsSelected = false);

                data = recognizer.SyncWithSession(data);

                foreach (var item in data.ItemObjects)
                {
                    if (item == null) continue;

                    var tag = AddTag(item, index++);
                    tag.IsSelected = true;

                    tag.Validate(Validator);
                }
                LogHrItemObjects("paste", data.ItemObjects);
            }
        }

        private Signalizations Validator(TagViewModel tag)
        {
            return recognizer.OnlyWords && tag.BlankType != TagViewModel.BlankTypes.Word ? Signalizations.Forbidden : Signalizations.None;
        }

        /// <summary>
        /// Формат {[id] ToString()[,] ...}
        /// </summary>
        private void LogHrItemObjects(string action, IEnumerable<IHrItemObject> hios)
        {
            logger.DebugFormat("{0} hios: {1}", action, hios.FlattenString());
        }

        /// <summary>
        /// Завершает тег.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="suggestion">Слово или запрос</param>
        /// <param name="exactMatchRequired">Требуется совпадение запроса и текста выбранного предположения.</param>
        private void CompleteCommon(TagViewModel tag, object suggestion, bool exactMatchRequired, bool inverse = false)
        {
            recognizer.SetBlank(tag, suggestion, exactMatchRequired, inverse);
            if (tag.Query == "")
            {
                tag.DeleteCommand.Execute(null);
            }
            else
            {
            }

            CompleteEnding(tag);
        }

        private void CompleteOnConvert(TagViewModel tag, TagViewModel.BlankTypes toType)
        {
            Contract.Requires(!tag.Query.IsNullOrEmpty());

            var measure = (tag.Blank as Measure);
            var converted = recognizer.ConvertBlank(tag, toType);

            if (converted && measure != null && toType != TagViewModel.BlankTypes.Comment)
            {
                // отдельный комментарий из числа измерения
                var comment = new Comment(string.Format("{0} {1}", measure.Value, measure.Uom).Trim());
                AddTag(comment, Tags.IndexOf(tag) + 1);
            }

            CompleteEnding(tag);
        }

        public void CompleteOnEnter(TagViewModel tag, bool inverse = false)
        {
            switch (tag.State)
            {
                case TagViewModel.States.Init:
                    // Enter в пустом поле
                    OnInputEnded();
                    return;

                case TagViewModel.States.Typing:
                    CompleteCommon(tag, SelectedSuggestion, false, inverse);
                    break;

                case TagViewModel.States.Completed:
                    // тег не изменен
                    break;
            }

            // переходим к вводу нового слова
            SelectedTag = LastTag;
            LastTag.IsTextBoxFocused = true;
        }

        public void CompleteOnLostFocus(TagViewModel tag)
        {
            if (tag.State == TagViewModel.States.Typing)
            {
                logger.Debug("CompleteOnLostFocus");
                CompleteCommon(tag, prevSelectedSuggestion, true);
            }
        }

        private void CompleteEnding(TagViewModel tag)
        {
            Suggestions.Clear();
            RefreshPopup();
            tag.Validate();

            // добавляем пустое поле
            if (LastTag.State == TagViewModel.States.Completed
                && !SingleTag)
            {
                AddTag(isLast: true);
            }
        }

        private object MakeSuggestions(TagViewModel tag)
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
            IsPopupOpen = Suggestions.Count > 0; // not on suggestion.collectionchanged - мигание при очистке
        }

        protected virtual void OnInputEnded()
        {
            var h = InputEnded;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }

        protected virtual void OnTagCompleted(TagViewModel tag)
        {
            var h = TagCompleted;
            if (h != null)
            {
                h(this, new TagEventArgs(tag));
            }
        }

        protected virtual void OnEntitiesChanged()
        {
            logger.DebugFormat("entities changed in {0}", this);
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
            Contract.Invariant(inDispose || LastTag.IsLast || SingleTag); // поле ввода по умолчанию
            Contract.Invariant(inDispose || LastTag.IsLast != SingleTag); // единственный тег не IsLast
            Contract.Invariant(Tags.Count(t => t.State == TagViewModel.States.Typing) <= 1); // только один тег редактируется
            Contract.Invariant(Tags.Count == 1 || !SingleTag); // единственный тег
        }

        protected override void Dispose(bool disposing)
        {
            inDispose = true;
            if (disposing)
            {
                Tags.Clear();
                hanlder.Dispose();
            }
            base.Dispose(disposing);
        }

        public class DropTargetHandler : DefaultDropHandler
        {
            private readonly AutocompleteViewModel master;

            public DropTargetHandler(AutocompleteViewModel master)
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
                return sourceList is IEnumerable<TagViewModel>;
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
                if (data.First() is TagViewModel)
                {
                    // drop tags from autocomplete
                    var tags = data.Cast<TagViewModel>();

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

                            n = Math.Max(n, 0); // when single n == -1

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
                        // copy tags' HrItemObjects or query

                        foreach (var tag in tags)
                        {
                            if (tag.BlankType == TagViewModel.BlankTypes.None)
                            {
                                master.AddTag(tag.Query).Validate(master.Validator);
                            }
                            else
                            {
                                var items = master.recognizer.EntitiesOf(tag).ToList();
                                foreach (var item in items)
                                {
                                    master.AddTag(item).Validate(master.Validator);
                                }
                            }
                        }
                    }

                    master.LastTag.IsSelected = false;
                }
            }
        }

        public class DragSourceHandler : IDragSource
        {
            /// <summary>
            /// Data is tags without Last tag.
            /// </summary>
            /// <param name="dragInfo"></param>
            public void StartDrag(IDragInfo dragInfo)
            {
                var tags = dragInfo.SourceItems.Cast<TagViewModel>().Where(t => !t.IsLast);
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
                var tags = dragInfo.SourceItems.Cast<TagViewModel>().Where(t => !t.IsLast);
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
                    var sugg = recognizer.SearchForSuggesstions(str, null).FirstOrDefault();
                    CompleteCommon(tag, sugg, true);
                    AddTag(tag);
                }
            }
        }
    }
}