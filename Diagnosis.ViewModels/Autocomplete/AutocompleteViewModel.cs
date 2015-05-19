using Diagnosis.Common;
using Diagnosis.Common.Types;
using Diagnosis.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Autocomplete
{
    public partial class AutocompleteViewModel : ViewModelBase, ITagParentAutocomplete, IHrEditorAutocomplete, IQbAutocompleteViewModel, IViewAutocompleteViewModel, ITagsTrackableAutocomplete
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AutocompleteViewModel));
        private readonly SuggestionsMaker sugMaker;
        private readonly bool allowSendToSearch;
        private readonly bool allowConfidenceToggle;
        private readonly bool singleTag;
        private readonly bool measureEditorWithCompare;
        private readonly IEnumerable<BlankType> convertTo;

        private ObservableCollection<TagViewModel> tagsWritable;
        private TagViewModel _selTag;
        private bool _popupOpened;
        private SuggestionViewModel prevSelectedSuggestion;
        private TagViewModel _editingTag;
        private bool _supressCompletion;
        private SuggestionViewModel _selectedSuggestion;
        private bool _showALt;
        private EventAggregator.EventMessageHandler hanlder;
        private bool inDispose;
        private VisibleRelayCommand<TagViewModel> sendToSearch;
        private VisibleRelayCommand toggleConfidence;
        private OptionsMode mode;
        private BlankSetter blankSetter;

        public AutocompleteViewModel(SuggestionsMaker sugMaker, OptionsMode mode, IEnumerable<object> initItems)
            : this(sugMaker,
                allowSendToSearch: mode == OptionsMode.HrEditor,
                allowConfidenceToggle: mode != OptionsMode.MeasureEditor,
                singleTag: mode == OptionsMode.MeasureEditor,
                measureEditorWithCompare: mode == OptionsMode.Search,
                initItems: initItems ?? Enumerable.Empty<object>(),
                convertTo: mode == OptionsMode.HrEditor ? new[] { BlankType.Word, BlankType.Comment, BlankType.Icd, BlankType.Measure } :
                            mode == OptionsMode.Search ? new[] { BlankType.Word, BlankType.Measure } :
                            Enumerable.Empty<BlankType>()
            )
        {
            this.mode = mode;
        }

        private AutocompleteViewModel(SuggestionsMaker sugMaker,
            bool allowSendToSearch,
            bool allowConfidenceToggle,
            bool singleTag,
            bool measureEditorWithCompare,
            IEnumerable<object> initItems,
            IEnumerable<BlankType> convertTo)
        {
            Contract.Requires(sugMaker != null);
            Contract.Requires(initItems != null);
            Contract.Requires(convertTo != null);

            this.sugMaker = sugMaker;
            this.convertTo = convertTo;
            this.allowSendToSearch = allowSendToSearch;
            this.allowConfidenceToggle = allowConfidenceToggle;
            this.singleTag = singleTag;
            this.measureEditorWithCompare = measureEditorWithCompare;
            this.blankSetter = new BlankSetter(sugMaker.FirstMatchingOrNewWord, OpenMeasureEditor, OpenIcdSelector);

            tagsWritable = new ObservableCollection<TagViewModel>();
            Tags = new INCCReadOnlyObservableCollection<TagViewModel>(tagsWritable);
            Tags.CollectionChangedWrapper += (s, e) =>
            {
                if (inDispose) return;

                // logger.DebugFormat("{0} '{1}' '{2}'", e.Action, e.OldStartingIndex, e.NewStartingIndex);

                // кроме добавления пустого тега
                if (!(e.Action == NotifyCollectionChangedAction.Add && ((TagViewModel)e.NewItems[0]).State == State.Init))
                {
                    OnEntitiesChanged();
                    OnPropertyChanged(() => IsEmpty);
                }
            };
            hanlder = this.Subscribe(Event.WordPersisted, (e) =>
            {// TODO двжды здесь?
                // созданные слова можно искать из поиска, убираем сигнал "новое" после сохранения слова
                var word = e.GetValue<Word>(MessageKeys.Word);
                Tags.Where(t => (t.Blank as Word) == word)
                    .ForAll(t => t.Validate());
            });

            AddTag(isLast: true);

            foreach (var item in initItems)
            {
                AddTag(item);
            }

            Suggestions = new ObservableCollection<SuggestionViewModel>();

            DropHandler = new AutocompleteViewModel.DropTargetHandler(this);
            DragHandler = new AutocompleteViewModel.DragSourceHandler(this);
            IsDropTargetEnabled = true;
            IsDragSourceEnabled = true;
        }

        /// <summary>
        /// Возникает, когда работа с автокомплитом окончена. (Enter второй раз.)
        /// True если Control+Enter
        /// </summary>
        public event EventHandler<BoolEventArgs> InputEnded;

        /// <summary>
        /// Возникает, когда завершается редактирование тега.
        /// </summary>
        public event EventHandler<TagEventArgs> TagCompleted;

        /// <summary>
        /// Возникает, когда меняется набор сущностей в тегах. (Завершение редактирования, конвертация, удаление, cut, paste.)
        /// </summary>
        public event EventHandler EntitiesChanged;

        /// <summary>
        /// Возникает, когда меняется уверенность у завершенных тегов.
        /// </summary>
        public event EventHandler ConfidencesChanged;

        /// <summary>
        /// Возникает, когда меняется набор сущностей или уверенность.
        /// </summary>
        public event EventHandler CHiosChanged;

        public enum OptionsMode
        {
            HrEditor,
            MeasureEditor,
            Search
        }

        public RelayCommand<TagViewModel> EnterCommand
        {
            get
            {
                return new RelayCommand<TagViewModel>(
                    (tag) => CompleteOnEnter(tag),
                    (tag) => tag != null);
            }
        }

        public RelayCommand<TagViewModel> ControlEnterCommand
        {
            get
            {
                return new RelayCommand<TagViewModel>(
                    (tag) => CompleteOnEnter(tag, withControl: true),
                    (tag) => tag != null);
            }
        }

        public RelayCommand<TagViewModel> InverseEnterCommand
        {
            get
            {
                return new RelayCommand<TagViewModel>(
                    (tag) => CompleteOnEnter(tag, inverse: true),
                    (tag) => tag != null);
            }
        }

        public ObservableCollection<SuggestionViewModel> Suggestions { get; private set; }

        public SuggestionViewModel SelectedSuggestion
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

        public INCCReadOnlyObservableCollection<TagViewModel> Tags { get; private set; }

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

        public RelayCommand AddIcdCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    AddFromEditor(BlankType.Icd);
                });
            }
        }

        public RelayCommand AddMeasureCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    AddFromEditor(BlankType.Measure);
                });
            }
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
                            case BlankType.Measure:
                                OpenMeasureEditor(SelectedTag.Blank as Measure, null, (m) =>
                                {
                                    CompleteCommon(SelectedTag, m, false);
                                });
                                break;

                            case BlankType.Icd:
                                OpenIcdSelector(SelectedTag.Blank as IcdDisease, null, (i) =>
                                {
                                    CompleteCommon(SelectedTag, i, false);
                                });
                                break;

                            default:
                                SelectedTag.ToggleEditState();
                                break;
                        }
                    }
                });
            }
        }

        public VisibleRelayCommand ToggleConfidenceCommand
        {
            get
            {
                return toggleConfidence ?? (toggleConfidence = new VisibleRelayCommand(() =>
                {
                    if (SelectedTag != null)
                    {
                        var next = SelectedTag.Confidence == Confidence.Present ? Confidence.Absent : Confidence.Present;
                        SelectedTags.ForEach(t => t.Confidence = next);
                        OnConfidencesChanged();
                    }
                }, () => WithConfidence)
                {
                    IsVisible = WithConfidence
                });
            }
        }

        /// <summary>
        /// Показывает предположения для редактируемого тега.
        /// Затем дополняет ввод выбранным предположением без завершения.
        /// </summary>
        public RelayCommand ShowSuggestionsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsPopupOpen)
                    {
                        SelectedTag.Query = SelectedSuggestion.Hio.ToString();
                    }
                    else
                    {
                        MakeSuggestions(SelectedTag);
                        RefreshPopup();
                    }
                }, () => SelectedTag != null && SelectedTag.IsTextBoxFocused);
            }
        }

        /// <summary>
        /// Переключает режим добавления запроса в предположения.
        /// </summary>
        public RelayCommand ToggleSuggestionModeCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    sugMaker.AddQueryToSuggestions = !sugMaker.AddQueryToSuggestions;
                    MakeSuggestions(SelectedTag);
                    RefreshPopup();
                }, () => SelectedTag != null && SelectedTag.IsTextBoxFocused);
            }
        }

        public VisibleRelayCommand<TagViewModel> SendToSearchCommand
        {
            get
            {
                return sendToSearch ?? (sendToSearch = new VisibleRelayCommand<TagViewModel>((t) =>
                {
                    IEnumerable<ConfWithHio> entities;
                    if (t != null)
                        entities = new ConfWithHio(t.Blank, t.Confidence).ToEnumerable();
                    else
                        entities = GetCHIOsOfSelectedCompleted();
                    this.Send(Event.SendToSearch, entities.ToList().AsParams(MessageKeys.ToSearchPackage));
                }, (t) => WithSendToSearch)
                {
                    IsVisible = WithSendToSearch
                });
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
        /// Последний выбранный тег.
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

        /// <summary>
        /// Ни в одном теге нет текста.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return !Tags.Any(x => !x.Query.IsNullOrEmpty());
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
                    Suggestions.ForEach(x => x.IsAlter = value);
                    OnPropertyChanged(() => ShowAltSuggestion);
                }
            }
        }

        /// <summary>
        /// Добавлять запрос как новое слово в список предположений, если нет соответствующего слова.
        /// </summary>
        public bool AddQueryToSuggestions
        {
            get { return sugMaker.AddQueryToSuggestions; }
            set
            {
                sugMaker.AddQueryToSuggestions = value;
                OnPropertyChanged(() => AddQueryToSuggestions);
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
            get { return allowSendToSearch; }
        }

        public bool WithConvertTo(BlankType type)
        {
            return convertTo.Contains(type);
        }

        public bool WithConvert
        {
            get { return convertTo.Any(); }
        }

        public bool WithConfidence
        {
            get { return allowConfidenceToggle; }
        }

        public bool InDispose
        {
            get { return inDispose; }
        }

        /// <summary>
        /// Создает тег.
        /// </summary>
        private TagViewModel CreateTag(object content = null)
        {
            Contract.Requires(content == null || content is string || content is ConfWithHio || content is IHrItemObject);

            TagViewModel tag;
            var itemObject = content as IHrItemObject;
            var chio = content as ConfWithHio;
            var str = content as string;

            if (itemObject != null)
                tag = new TagViewModel(this, itemObject);
            else if (chio != null)
            {
                tag = new TagViewModel(this, chio.HIO);
                tag.Confidence = chio.Confidence;
            }
            else if (str != null)
                tag = new TagViewModel(this, str);
            else
                tag = new TagViewModel(this);

            tag.Deleted += (s, e) =>
            {
                Contract.Requires(!tag.IsLast);
                tagsWritable.Remove(tag);
                StartEdit(LastTag);
            };
            tag.Converting += CompleteOnConvert;
            tag.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Query")
                {
                    if (EditingTag == null) // non user change
                        return;
                    MakeSuggestions(EditingTag);
                    RefreshPopup();
                    OnPropertyChanged(() => IsEmpty);
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
                            MakeSuggestions(SelectedTag); // предположения для тегов с сигнализацией
                        }

                        CanCompleteOnLostFocus = true;

                        SelectedTags.Except(tag.ToEnumerable()).ForAll(t => t.IsSelected = false);
                        SelectedTag = tag;
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
                    if (tag.State == State.Completed)
                    {
                        OnTagCompleted(tag);
                        OnEntitiesChanged();
                    }
                }
            };
            return tag;
        }

        public void AddFromEditor(BlankType type, int index = -1)
        {
            if (type == BlankType.Measure)
            {
                OpenMeasureEditor(null, null, (m) =>
                {
                    AddTag(m, index);
                });
            }
            else if (type == BlankType.Icd)
            {
                OpenIcdSelector(null, null, (i) =>
                {
                    AddTag(i, index);
                });
            }
        }

        /// <summary>
        /// Добавляет тег в коллекцию.
        /// <param name="tagOrContent">Созданный тег, строка запроса, ConfindenceHrItemObject или null для пустого тега.</param>
        /// </summary>
        public TagViewModel AddTag(object tagOrContent = null, int index = -1, bool isLast = false)
        {
            Contract.Requires(tagOrContent == null || tagOrContent is TagViewModel || tagOrContent is string || tagOrContent is ConfWithHio || tagOrContent is IHrItemObject);

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

            // complete editing tags before add new
            Tags.Where(t => t.State == State.Typing)
                .ForEach(t => CompleteOnLostFocus(t));

            if (SingleTag && Tags.Count > 0)
                tagsWritable[0] = tag;
            else
                tagsWritable.Insert(index, tag);
            return tag;
        }

        /// <summary>
        /// Добавляет пустой тег рядом с другим.
        /// </summary>
        public void AddTagNearAndEdit(TagViewModel from, bool left)
        {
            var tag = AddTag(index: Tags.IndexOf(from) + (left ? 0 : 1));
            StartEdit(tag);
        }

        public void StartEdit(TagViewModel tag)
        {
            SelectedTag = tag;
            tag.IsTextBoxFocused = true;
        }

        public void StartEdit()
        {
            SelectedTag = LastTag;
            LastTag.IsTextBoxFocused = true;
        }

        public void ReplaceTagsWith(IEnumerable<object> items)
        {
            // оставляем последний тег
            while (Tags.Count != 1)
            {
                tagsWritable.RemoveAt(Tags.Count - 2);
            }

            foreach (var item in items)
            {
                AddTag(item).Validate(Validator);
            }
        }

        /// <summary>
        /// Возвращает сущности из тегов по порядку.
        /// Не должен вызываться, если есть редактируемый тег.
        /// </summary>
        public IEnumerable<ConfWithHio> GetCHIOs()
        {
            Contract.Assume(Tags.All(t => t.State != State.Typing));

            Tags.Where(x => x.BlankType == BlankType.None && x.State != State.Init)
                 .ForAll((x) => logger.WarnFormat("{0} without entity blank, skip", x));

            return Tags
                .Where(x => x.BlankType != BlankType.None)
                .Select(t => new ConfWithHio(t.Blank, t.Confidence));
        }
        /// <summary>
        /// Возвращает сущности из завершенных тегов по порядку.
        /// </summary>
        public IEnumerable<ConfWithHio> GetCHIOsOfCompleted()
        {
            return Tags
                .Where(t => t.State == State.Completed)
                .Select(t => new ConfWithHio(t.Blank, t.Confidence));
        }

        /// <summary>
        /// Возвращает сущности из выделенных завершенных тегов по порядку.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<ConfWithHio> GetCHIOsOfSelectedCompleted()
        {
            return SelectedTags
                .Where(t => t.State == State.Completed)
                .Select(t => new ConfWithHio(t.Blank, t.Confidence));
        }

        private Signalizations Validator(TagViewModel tag)
        {
            return mode == OptionsMode.Search && (tag.BlankType == BlankType.None || tag.BlankType == BlankType.Comment)
                ? Signalizations.Forbidden
                : Signalizations.None;
        }

        /// <summary>
        /// Завершает тег.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="sugOrHio">Предложение или hio</param>
        /// <param name="exactMatchRequired">Требуется совпадение запроса и текста выбранного предположения.</param>
        private void CompleteCommon(TagViewModel tag, object sugOrHio, bool exactMatchRequired, bool inverse = false)
        {
            Contract.Requires(sugOrHio is SuggestionViewModel || sugOrHio is IHrItemObject || sugOrHio == null);
            Contract.Ensures(tag.State == State.Completed);

            var hio = sugOrHio as IHrItemObject;
            var vm = sugOrHio as SuggestionViewModel;
            if (vm != null)
                hio = vm.Hio;

            blankSetter.SetBlank(tag, hio, exactMatchRequired, inverse);

            CompleteEnding(tag);

            if (tag.Query.IsNullOrEmpty())
            {
                tag.DeleteCommand.Execute(null);
            }
        }

        private void CompleteOnConvert(object s, BlankTypeEventArgs e)
        {
            var tag = s as TagViewModel;
            var measure = (tag.Blank as Measure);
            var wasLast = tag.IsLast;
            Action onConverted = () =>
            {
                if (measure != null && e.type != BlankType.Comment)
                {
                    // отдельный комментарий из числа измерения
                    var comment = new Comment(measure.FormattedValueUom);
                    AddTag(comment, Tags.IndexOf(tag) + 1);
                }

                CompleteEnding(tag);

                OnEntitiesChanged(); // TODO повторно, тк при конверте сначала меняется query, поэтому меняется state на completed

                if (wasLast)
                {
                    // convert from Last - continue typing
                    StartEdit();
                }
            };
            blankSetter.ConvertBlank(tag, e.type, onConverted);
        }

        private void OpenMeasureEditor(Measure m, Word w, Action<Measure> onOk)
        {
            var vm = new MeasureEditorViewModel(m, w, measureEditorWithCompare);
            vm.OnDialogResult(() => onOk(vm.Measure));
            uiTaskFactory.StartNew(() =>
            {
                this.Send(Event.OpenDialog, vm.AsParams(MessageKeys.Dialog));
            });
        }

        private void OpenIcdSelector(IcdDisease i, string q, Action<IcdDisease> onOk)
        {
            var vm = new IcdSelectorViewModel(i, q);
            vm.OnDialogResult(() => onOk(vm.SelectedIcd));
            uiTaskFactory.StartNew(() =>
            {
                this.Send(Event.OpenDialog, vm.AsParams(MessageKeys.Dialog));
            });
        }

        internal void CompleteOnEnter(TagViewModel tag, bool inverse = false, bool withControl = false)
        {
            switch (tag.State)
            {
                case State.Init:
                    if (tag.IsLast)
                    {
                        OnInputEnded(withControl);
                        return;
                    }
                    else
                    {
                        CompleteCommon(tag, SelectedSuggestion, false, false); // пустой не последний — удаляем
                        break;
                    }

                case State.Typing:
                    CompleteCommon(tag, SelectedSuggestion, false, inverse);
                    break;

                case State.Completed:
                    // тег не изменен, но выбрано новое
                    if (SelectedSuggestion != null &&
                        SelectedSuggestion != tag.Blank)
                        CompleteCommon(tag, SelectedSuggestion, false, false);
                    break;
            }
            if (SingleTag || withControl)
                OnInputEnded(withControl);
            else
                // переходим к вводу нового слова
                StartEdit(LastTag);
        }

        public void CompleteOnLostFocus(TagViewModel tag)
        {
            // при завершении пустого тега он удаляется, теряется фокус и автокомплит пытается завершить тег еще раз
            if (!Tags.Contains(tag))
                return;

            if (tag.State == State.Typing)
            {
                logger.Debug("CompleteOnLostFocus");
                CompleteCommon(tag, prevSelectedSuggestion, true);
            }
        }

        public void CompleteTypings()
        {
            Contract.Ensures(Tags.All(t => t.State != State.Typing));

            Tags.Where(t => t.State == State.Typing)
                .ForEach(tag => CompleteOnLostFocus(tag));
        }

        private void CompleteEnding(TagViewModel tag)
        {
            Suggestions.Clear();
            RefreshPopup();
            tag.Validate();

            sugMaker.AfterCompleteTag(tag);

            // добавляем пустое поле
            if (LastTag.State == State.Completed
                && !SingleTag)
            {
                AddTag(isLast: true);
            }
        }

        private object MakeSuggestions(TagViewModel tag)
        {
            Contract.Requires(tag != null);

            var tagIndex = Tags.IndexOf(tag);

            // все сущности кроме сущности редактируемого тега
            var tagBlanksExceptEditing = Tags.Select((t, i) => i != tagIndex ? t.Blank : null);

            var results = sugMaker.SearchForSuggesstions(
                query: tag.Query,
                prevEntityBlank: tagIndex > 0 ? Tags[tagIndex - 1].Blank : null,
                exclude: null);

            Suggestions.Clear();
            foreach (var item in results)
            {
                Suggestions.Add(new SuggestionViewModel(item, ShowAltSuggestion, item.IsTransient));
            }

            SelectedSuggestion = Suggestions.FirstOrDefault();
            return Suggestions.FirstOrDefault();
        }

        private void RefreshPopup()
        {
            IsPopupOpen = Suggestions.Count > 0; // not on suggestion.collectionchanged - мигание при очистке
        }

        protected virtual void OnInputEnded(bool addHr)
        {
            var h = InputEnded;
            if (h != null)
            {
                h(this, new BoolEventArgs(addHr));
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
            // logger.DebugFormat("entities changed in {0}", this);
            var h = EntitiesChanged;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
            OnChiosChanged();
        }

        protected virtual void OnConfidencesChanged()
        {
            var h = ConfidencesChanged;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
            OnChiosChanged();
        }
        protected virtual void OnChiosChanged()
        {
            var h = CHiosChanged;
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
            Contract.Invariant(Tags.Count(t => t.State == State.Typing) <= 1); // только один тег редактируется
            Contract.Invariant(Tags.Count == 1 || !SingleTag); // единственный тег
        }

        protected override void Dispose(bool disposing)
        {
            inDispose = true;
            if (disposing)
            {
                tagsWritable.Clear();
                hanlder.Dispose();
            }
            base.Dispose(disposing);
        }

        public void OnDrop(DragEventArgs e)
        {
            logger.DebugFormat("drop {0}", e.Data.ToString());

            string text = GetDroppedText(e);

            if (text != null)
            {
                // drop strings - make tag with query and complete it

                var strings = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var str in strings)
                {
                    var tag = CreateTag(str);
                    var sugg = sugMaker.SearchForSuggesstions(str, null).FirstOrDefault();
                    CompleteCommon(tag, sugg, true);
                    AddTag(tag);
                }
            }
        }

        private static string GetDroppedText(DragEventArgs e)
        {
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
            return text;
        }

        ICommand ITagParentAutocomplete.EditCommand
        {
            get { return EditCommand; }
        }

        ICommand ITagParentAutocomplete.SendToSearchCommand
        {
            get { return SendToSearchCommand; }
        }

        ICommand ITagParentAutocomplete.ToggleConfidenceCommand
        {
            get { return ToggleConfidenceCommand; }
        }

        ICommand IHrEditorAutocomplete.DeleteCommand
        {
            get { return DeleteCommand; }
        }

        ICommand IHrEditorAutocomplete.SendToSearchCommand
        {
            get { return SendToSearchCommand; }
        }

        ICommand IHrEditorAutocomplete.ToggleSuggestionModeCommand
        {
            get { return ToggleSuggestionModeCommand; }
        }

        INotifyCollectionChanged IQbAutocompleteViewModel.Tags
        {
            get { return Tags; }
        }
    }
}