using Diagnosis.Common;
using Diagnosis.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Diagnosis.ViewModels.Autocomplete
{
    public partial class AutocompleteViewModel : ViewModelBase, Diagnosis.ViewModels.Autocomplete.IAutocompleteViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AutocompleteViewModel));
        private readonly Recognizer recognizer;
        private readonly bool allowSendToSearch;
        private readonly bool allowTagConvertion;
        private readonly bool allowConfidenceToggle;
        private readonly bool singleTag;

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

        public AutocompleteViewModel(Recognizer recognizer, OptionsMode mode, IEnumerable<ConfindenceHrItemObject> initItems)
            : this(recognizer,
                allowTagConvertion: mode == OptionsMode.HrEditor,
                allowSendToSearch: mode == OptionsMode.HrEditor,
                allowConfidenceToggle: mode == OptionsMode.HrEditor,
                singleTag: mode == OptionsMode.MeasureEditor,
                initItems: initItems)
        {
        }

        public AutocompleteViewModel(Recognizer recognizer, bool allowTagConvertion, bool allowSendToSearch, bool allowConfidenceToggle, bool singleTag, IEnumerable<ConfindenceHrItemObject> initItems)
        {
            Contract.Requires(recognizer != null);

            this.recognizer = recognizer;
            this.allowTagConvertion = allowTagConvertion;
            this.allowSendToSearch = allowSendToSearch;
            this.allowConfidenceToggle = allowConfidenceToggle;
            this.singleTag = singleTag;

            Tags = new ObservableCollection<TagViewModel>();
            Tags.CollectionChanged += (s, e) =>
            {
                if (inDispose) return;

                // logger.DebugFormat("{0} '{1}' '{2}'", e.Action, e.OldStartingIndex, e.NewStartingIndex);

                // кроме добавления пустого тега
                if (!(e.Action == NotifyCollectionChangedAction.Add && ((TagViewModel)e.NewItems[0]).State == State.Init))
                    OnEntitiesChanged();
            };
            hanlder = this.Subscribe(Event.WordPersisted, (e) =>
            {// TODO двжды здесь?
                // созданные слова можно искать из поиска
                var word = e.GetValue<Word>(MessageKeys.Word);
                Tags.Where(t => (t.Blank as Word) == word)
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
            Suggestions = new ObservableCollection<SuggestionViewModel>();

            DropHandler = new AutocompleteViewModel.DropTargetHandler(this);
            DragHandler = new AutocompleteViewModel.DragSourceHandler(this);
            IsDropTargetEnabled = true;
            IsDragSourceEnabled = true;
        }

        /// <summary>
        /// Возникает, когда работа с автокомплитом окончена. (Enter второй раз.)
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

        public event EventHandler ConfidencesChanged;

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
                    (tag) => tag != null && !recognizer.OnlyWords);
            }
        }

        public ObservableCollection<SuggestionViewModel> Suggestions
        {
            get;
            private set;
        }

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

        public ObservableCollection<TagViewModel> Tags
        {
            get;
            private set;
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
                            case BlankType.Measure:
                                var vm = new MeasureEditorViewModel(SelectedTag.Blank as Measure);
                                vm.OnDialogResult(() =>
                                {
                                    CompleteCommon(SelectedTag, vm.Measure, false);
                                });
                                this.Send(Event.OpenDialog, vm.AsParams(MessageKeys.Dialog));
                                break;

                            case BlankType.Icd:
                                var vm0 = new IcdSelectorViewModel(SelectedTag.Blank as IcdDisease);
                                vm0.OnDialogResult(() =>
                                {
                                    CompleteCommon(SelectedTag, vm0.SelectedIcd, false);
                                });
                                this.Send(Event.OpenDialog, vm0.AsParams(MessageKeys.Dialog));
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
                    recognizer.AddQueryToSuggestions = !recognizer.AddQueryToSuggestions;
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
                    IEnumerable<IHrItemObject> entities;
                    if (t != null)
                        entities = recognizer.EntityOf(t).ToEnumerable();
                    else
                        entities = GetCHIOsOfSelectedCompleted().Select(x => x.HIO);
                    this.Send(Event.SendToSearch, entities.AsParams(MessageKeys.HrItemObjects));
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

        public bool WithConvert
        {
            get { return allowTagConvertion; }
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
        /// <param name="сontent">Строка запроса, ConfindenceHrItemObject или null для пустого тега.</param>
        public TagViewModel CreateTag(object content = null)
        {
            TagViewModel tag;
            var itemObject = content as IHrItemObject;
            var chio = content as ConfindenceHrItemObject;
            var str = content as string;

            if (itemObject != null)
                tag = new TagViewModel(this, itemObject);
            else if (chio != null)
            {
                tag = new TagViewModel(this, chio.HIO);
                tag.Confidence = chio.Confindence;
            }
            else if (str != null)
                tag = new TagViewModel(this, str);
            else
                tag = new TagViewModel(this);

            tag.Deleted += (s, e) =>
            {
                Contract.Requires(!tag.IsLast);
                Tags.Remove(tag);
                StartEdit(LastTag);
            };
            tag.Converting += (s, e) =>
            {
                CompleteOnConvert(tag, e.type);
                {
                }
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
                var vm = new MeasureEditorViewModel();
                vm.OnDialogResult(() =>
                {
                    AddTag(vm.Measure, index);
                });
                this.Send(Event.OpenDialog, vm.AsParams(MessageKeys.Dialog));
            }
            else if (type == BlankType.Icd)
            {
                var vm = new IcdSelectorViewModel();
                vm.OnDialogResult(() =>
                {
                    AddTag(vm.SelectedIcd, index);
                });
                this.Send(Event.OpenDialog, vm.AsParams(MessageKeys.Dialog));
            }
        }

        /// <summary>
        /// Добавляет тег в коллекцию.
        /// <param name="tagOrContent">Созданный тег, строка запроса, ConfindenceHrItemObject или null для пустого тега.</param>
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

            // complete editing tags before add new
            Tags.Where(t => t.State == State.Typing)
                .ForEach(t => CompleteOnLostFocus(t));

            if (SingleTag && Tags.Count > 0)
                Tags[0] = tag;
            else
                Tags.Insert(index, tag);
            return tag;
        }

        /// <summary>
        /// Добавляет пустой тег рядом с другим.
        /// </summary>
        public void AddAndEditTag(TagViewModel from, bool left)
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

        public void ReplaceTagsWith(IEnumerable<IHrItemObject> items)
        {
            Contract.Requires(items != null);

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
        /// Не должен вызываться, если есть редактируемый тег.
        /// </summary>
        public IEnumerable<ConfindenceHrItemObject> GetCHIOs()
        {
            Contract.Requires(Tags.All(t => t.State != State.Typing));

            var result = new List<ConfindenceHrItemObject>();
            foreach (var tag in Tags)
            {
                if (tag.BlankType != BlankType.None)
                    result.Add(new ConfindenceHrItemObject(recognizer.EntityOf(tag), tag.Confidence));
                else if (tag.State != State.Init)
                    logger.WarnFormat("{0} without entity blank, skip", tag);
            }

            return result;
        }

        /// <summary>
        /// Возвращает сущности из завершенных тегов по порядку.
        /// </summary>
        public IEnumerable<ConfindenceHrItemObject> GetCHIOsOfCompleted()
        {
            var completed = Tags.Where(t => t.State == State.Completed);

            var hios = completed
                 .Select(t => new ConfindenceHrItemObject(recognizer.EntityOf(t), t.Confidence))
                 .ToList();
            return hios;
        }

        /// <summary>
        /// Возвращает сущности из выделенных завершенных тегов по порядку.
        /// </summary>
        /// <returns></returns>
        private List<ConfindenceHrItemObject> GetCHIOsOfSelectedCompleted()
        {
            var completed = SelectedTags.Where(t => t.State == State.Completed);

            var hios = completed
                 .Select(t => new ConfindenceHrItemObject(recognizer.EntityOf(t), t.Confidence))
                 .ToList();
            return hios;
        }

        private Signalizations Validator(TagViewModel tag)
        {
            return recognizer.OnlyWords && tag.BlankType != BlankType.Word ? Signalizations.Forbidden : Signalizations.None;
        }

        /// <summary>
        /// Завершает тег.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="suggestion">Слово или запрос</param>
        /// <param name="exactMatchRequired">Требуется совпадение запроса и текста выбранного предположения.</param>
        private void CompleteCommon(TagViewModel tag, object suggestion, bool exactMatchRequired, bool inverse = false)
        {
            Contract.Requires(suggestion is SuggestionViewModel || suggestion is Word || suggestion == null);

            var hio = suggestion as IHrItemObject;
            var vm = suggestion as SuggestionViewModel;
            if (vm != null)
                hio = vm.Hio;

            recognizer.SetBlank(tag, hio, exactMatchRequired, inverse);

            CompleteEnding(tag);

            if (tag.Query.IsNullOrEmpty())
            {
                tag.DeleteCommand.Execute(null);
            }
        }

        private void CompleteOnConvert(TagViewModel tag, BlankType toType)
        {
            var measure = (tag.Blank as Measure);
            var wasLast = tag.IsLast;
            var x = recognizer.ConvertBlank(tag, toType) //.ConfigureAwait<bool>(false);
                .ContinueWith(t =>
                 {
                     if (t.Result)
                     {
                         Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                         {
                             if (measure != null && toType != BlankType.Comment)
                             {
                                 // отдельный комментарий из числа измерения
                                 var comment = new Comment(string.Format("{0} {1}", measure.Value, measure.Uom).Trim());
                                 AddTag(comment, Tags.IndexOf(tag) + 1);
                             }

                             CompleteEnding(tag);

                             OnEntitiesChanged(); // повторно, тк при конверте сначала меняется query, поэтому меняется state на completed

                             if (wasLast)
                                 // convert from Last - continue typing
                                 StartEdit();
                         }));
                     }
                 }, TaskContinuationOptions.ExecuteSynchronously);
        }

        public void CompleteOnEnter(TagViewModel tag, bool inverse = false, bool withControl = false)
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
            if (tag.State == State.Typing)
            {
                logger.Debug("CompleteOnLostFocus");
                CompleteCommon(tag, prevSelectedSuggestion, true);
            }
        }

        public void CompleteTypings()
        {
            Tags.Where(t => t.State == State.Typing)
                .ForEach(tag => CompleteOnLostFocus(tag));
        }

        private void CompleteEnding(TagViewModel tag)
        {
            Suggestions.Clear();
            RefreshPopup();
            tag.Validate();

            recognizer.AfterCompleteTag(tag);

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

            var results = recognizer.SearchForSuggesstions(
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
        }

        protected virtual void OnConfidencesChanged()
        {
            var h = ConfidencesChanged;
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
            Contract.Invariant(!(WithConvert && recognizer.OnlyWords)); // конвертировать, когда только слова, бессмысленно
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

        System.Windows.Input.ICommand IAutocompleteViewModel.EditCommand
        {
            get { return EditCommand; }
        }

        System.Windows.Input.ICommand IAutocompleteViewModel.SendToSearchCommand
        {
            get { return SendToSearchCommand; }
        }

        System.Windows.Input.ICommand IAutocompleteViewModel.ToggleConfidenceCommand
        {
            get { return ToggleConfidenceCommand; }
        }
    }
}