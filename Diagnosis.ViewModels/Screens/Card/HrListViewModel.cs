using Diagnosis.Common;
using Diagnosis.Common.Util;
using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public enum HrViewSortingColumn
    {
#if DEBUG

        [LocalizableDescription(@"Sorting_None")]
        None,

#endif

        [LocalizableDescription(@"Sorting_Ord")]
        Ord,

        [LocalizableDescription(@"Sorting_Category")]
        Category,

        [LocalizableDescription(@"Sorting_Date")]
        SortingDate,

        [LocalizableDescription(@"Sorting_CreatedAt")]
        CreatedAt
    }

    public enum HrViewGroupingColumn
    {
        [LocalizableDescription(@"Sorting_None")]
        None,

        [LocalizableDescription(@"Sorting_Category")]
        Category,

        //[LocalizableDescription(@"Sorting_Date")]
        //GroupingDate,
        [LocalizableDescription(@"Sorting_CreatedAt")]
        GroupingCreatedAt
    }

    public class HrListViewModel : ViewModelBase, IClipboardTarget
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HrListViewModel));
        private static HrViewer hrViewer = new HrViewer();
        internal readonly IHrsHolder holder;
        internal readonly HealthRecordManager hrManager;
        private readonly ListCollectionView view;
        private ShortHealthRecordViewModel _selectedHealthRecord;
        private Action<HealthRecord, HrData.HrInfo> fillHr;
        private Action<List<IHrItemObject>> syncHios;
        private ReentrantFlag noUnselectPrev = new ReentrantFlag();
        private bool _rectSelect;
        private HrViewSortingColumn _sort = HrViewSortingColumn.SortingDate; // to change in ctor
        private HrViewGroupingColumn _group = HrViewGroupingColumn.GroupingCreatedAt;
        private bool _dragSource;
        private bool _dropTarget;
        private bool _focused;
        internal readonly FlagActionWrapper<IEnumerable<ShortHealthRecordViewModel>> preserveSelected;
        bool inSetSelected;
        public event EventHandler<ListEventArgs<HealthRecord>> SaveNeeded;

        public HrListViewModel(IHrsHolder holder, Action<HealthRecord, HrData.HrInfo> filler, Action<List<IHrItemObject>> syncer)
        {
            Contract.Requires(holder != null);
            Contract.Requires(filler != null);
            Contract.Requires(syncer != null);
            this.holder = holder;
            this.fillHr = filler;
            this.syncHios = syncer;

            HolderVm = new HolderViewModel(holder);

            preserveSelected = new FlagActionWrapper<IEnumerable<ShortHealthRecordViewModel>>((hrs) => { hrs.ForEach(vm => vm.IsSelected = true); });
            hrManager = new HealthRecordManager(holder, onHrVmPropChanged: (s, e) =>
            {
                var hrvm = s as ShortHealthRecordViewModel;
                if (hrvm != null)
                {
                    if (e.PropertyName == "IsSelected")
                    {

                        // simulate IsSynchronizedWithCurrentItem
                        // SelectedHealthRecord points to new IsSelected without unselect prev
                        if (hrvm.IsSelected)
                        {
                            // logger.DebugFormat("{0} {1}", hrvm.IsSelected, hrvm);
                            using (noUnselectPrev.Join())
                            {
                                SelectedHealthRecord = hrvm;
                            }
                        }
                    }
                    if (Enum.GetNames(typeof(HrViewGroupingColumn)).Contains(e.PropertyName) ||
                       Enum.GetNames(typeof(HrViewSortingColumn)).Contains(e.PropertyName))
                    {
                        // simulate liveshaping
                        using (preserveSelected.Enter(SelectedHealthRecords)) // fix selection after CommitEdit when view grouping
                        {
                            logger.DebugFormat("edit {0} in {1}", e.PropertyName, hrvm);
                            SetHrExtra(hrvm.ToEnumerable().ToList());
                            view.EditItem(hrvm);
                            view.CommitEdit();
                            //  logger.DebugFormat("commit {0}", hrvm);
                        }
                    }

                    if (e.PropertyName == "IsChecked")
                    {
                        OnPropertyChanged(() => CheckedHrCount);
                    }
                }

            });
            hrManager.DeletedHealthRecords.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    // удалены

                    // выделяем первую после удаленной записи, чтобы она была выделена для фокуса на ней
                    var del = e.NewItems.Cast<ShortHealthRecordViewModel>().ToList();
                    logger.DebugFormat("deleted {0}", del);
                    SelectedHealthRecord = HealthRecords.FirstAfterAndNotIn(del);

                    OnSaveNeeded(del.Select(x => x.healthRecord).ToList());
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    // восстановлены или убраны из держателя

                    // сохраняем восстановленные
                    var restored = e.OldItems.Cast<ShortHealthRecordViewModel>()
                        .Where(x => holder.HealthRecords.Contains(x.healthRecord))
                        .Select(x => x.healthRecord)
                        .ToList();
                    OnSaveNeeded(restored);
                }
            };

            HealthRecords.CollectionChanged += (s, e) =>
            {
                // если запись IsDeleted
                HolderVm.UpdateIsEmpty();

                // новые в списке
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    SetHrExtra(e.NewItems.Cast<ShortHealthRecordViewModel>().ToList());
                }
            };

            view = (ListCollectionView)CollectionViewSource.GetDefaultView(HealthRecords);
            Grouping = HrViewGroupingColumn.Category;
            Sorting = HrViewSortingColumn.Ord;
            SetHrExtra(HealthRecords);

            DropHandler = new DropTargetHandler(this);
            DragHandler = new DragSourceHandler();

            IsRectSelectEnabled = true;
            IsDragSourceEnabled = true;
            IsDropTargetEnabled = true;

            SelectHealthRecord(hrViewer.GetLastSelectedFor(holder));
        }

        public HolderViewModel HolderVm { get; private set; }

        public ObservableCollection<ShortHealthRecordViewModel> HealthRecords { get { return hrManager.HealthRecords; } }

        //public IList<ShortHealthRecordViewModel> HealthRecordsView
        //{
        //    get
        //    {
        //        var view = new ListCollectionView(HealthRecords);

        //        // основная сортировка, если уже нет
        //        if (Sorting != HrViewSortingColumn.Ord)
        //        {
        //            var sort = new SortDescription(Sorting.ToString(), ListSortDirection.Ascending);
        //            view.SortDescriptions.Add(sort);
        //        }

        //        // сортировка по порядку всегда есть
        //        //var ord = new SortDescription(HrViewSortingColumn.Ord.ToString(), ListSortDirection.Ascending);
        //        // view.SortDescriptions.Add(ord);

        //        return view.Cast<ShortHealthRecordViewModel>().ToList();
        //    }
        //}

        public ShortHealthRecordViewModel SelectedHealthRecord
        {
            get { return _selectedHealthRecord; }
            set
            {
                if (_selectedHealthRecord == value)
                    return;

                if (value == null && inSetSelected)
                    // list box sets value to null, skip if we are in setting new value
                    return;

                inSetSelected = true;
                if (_selectedHealthRecord != null && noUnselectPrev.CanEnter) // снимаем выделение с прошлой выделенной
                {
                    using (noUnselectPrev.Join())
                    {
                        _selectedHealthRecord.IsSelected = false;

                    }
                }
                if (value != null)
                {
                    hrViewer.Select(value.healthRecord, holder);
                    value.IsSelected = true;
                }
                logger.DebugFormat("hrList selected {0} -> {1}", _selectedHealthRecord, value);
                _selectedHealthRecord = value;
                OnPropertyChanged(() => SelectedHealthRecord);
                inSetSelected = false;

            }
        }

        public IEnumerable<ShortHealthRecordViewModel> SelectedHealthRecords
        {
            get { return HealthRecords.Where(vm => vm.IsSelected).ToList(); }
        }

        public bool AddingHrByCommnd { get; private set; }

        #region Commands

        public ICommand AddHealthRecordCommand
        {
            get
            {
                return new RelayCommand(() =>
                    {
                        var lastHrVM = SelectedHealthRecord ?? HealthRecords.LastOrDefault();
                        var newHr = AddHr(true);

                        if (lastHrVM != null)
                        {
                            // копируем категории из последней записи
                            newHr.Category = lastHrVM.healthRecord.Category;
                        }
                    },
                    // нельзя добавлять новую запись, пока выбранная пуста
                    () => SelectedHealthRecord == null || !SelectedHealthRecord.healthRecord.IsEmpty()
                    );
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return new RelayCommand(() =>
                    {
                        hrManager.DeleteCheckedHealthRecords();
                    }, () => CheckedHrCount > 0);
            }
        }

        public ICommand SendToSearchCommand
        {
            get
            {
                return new RelayCommand(() =>
                        {
                            this.Send(Event.SendToSearch, hrManager.GetSelectedHrs()
                                .AsParams(MessageKeys.HealthRecords));
                        }, () => CheckedHrCount > 0);
            }
        }

        public RelayCommand<bool> MoveHrSelectionCommand
        {
            get
            {
                return new RelayCommand<bool>((up) =>
                        {
                            Contract.Ensures(SelectedHealthRecords.Count() <= 1);
                            Contract.Ensures(SelectedHealthRecord != Contract.OldValue(SelectedHealthRecord) || HealthRecords.Count <= 1);

                            HealthRecords.Except(SelectedHealthRecord.ToEnumerable()).ToList().ForEach(vm => vm.IsSelected = false);
                            
                            var sortedView = view.Cast<ShortHealthRecordViewModel>().ToList();
                            var current = sortedView.IndexOf(SelectedHealthRecord);
                            if (up)
                            {
                                if (current > 0)
                                    SelectedHealthRecord = sortedView[current - 1];
                                else
                                    SelectedHealthRecord = sortedView.Last(); // select last when Selected == null
                            }
                            else
                            {
                                if (current != sortedView.Count - 1)
                                    SelectedHealthRecord = sortedView[current + 1]; // select fisrt when Selected == null
                                else
                                    SelectedHealthRecord = sortedView.First();
                            }
                        });
            }
        }

        #endregion Commands

        public int CheckedHrCount
        {
            get
            {
                return HealthRecords.Where(hr => hr.IsChecked).Count();
            }
        }

        public bool ShowCanAlso
        {
            get
            {
                return !(holder is Appointment && !holder.IsEmpty()); // непустой осмотр - ничего дополнительного
            }
        }

        public DropTargetHandler DropHandler { get; private set; }

        public DragSourceHandler DragHandler { get; private set; }

        public bool IsRectSelectEnabled
        {
            get
            {
                return _rectSelect;
            }
            set
            {
                if (_rectSelect != value)
                {
                    _rectSelect = value;
                    OnPropertyChanged(() => IsRectSelectEnabled);
                }
            }
        }

        public bool IsDragSourceEnabled
        {
            get
            {
                return _dragSource;
            }
            set
            {
                if (_dragSource != value)
                {
                    _dragSource = value;
                    OnPropertyChanged(() => IsDragSourceEnabled);
                }
            }
        }

        public bool IsDropTargetEnabled
        {
            get
            {
                return _dropTarget;
            }
            set
            {
                if (_dropTarget != value)
                {
                    _dropTarget = value;
                    OnPropertyChanged(() => IsDropTargetEnabled);
                }
            }
        }

        public bool CanReorder
        {
            get
            {
                return (Sorting == HrViewSortingColumn.Ord
#if DEBUG
 || Sorting == HrViewSortingColumn.None
#endif
);
            }
        }

        public bool IsFocused
        {
            get
            {
                return _focused;
            }
            set
            {
                if (_focused != value)
                {
                    _focused = value;
                    //logger.DebugFormat("HrList focused {0} ", value);

                    OnPropertyChanged(() => IsFocused);
                }
            }
        }

        public HrViewSortingColumn Sorting
        {
            get
            {
                return _sort;
            }
            set
            {
                if (_sort != value)
                {
#if DEBUG
                    if (value == HrViewSortingColumn.None)
                        view.SortDescriptions.Clear();
                    else
#endif
                    {
                        using (view.DeferRefresh())
                        {
                            view.SortDescriptions.Clear();

                            // сначала сортируем по группам
                            if (Grouping != HrViewGroupingColumn.None)
                            {
                                var groupingSd = new SortDescription(Group2SortString(Grouping), ListSortDirection.Ascending);
                                view.SortDescriptions.Add(groupingSd);
                            }

                            // основная сортировка, если уже нет
                            if (value != HrViewSortingColumn.Ord && Group2Sort(Grouping) != value)
                            {
                                var sort = new SortDescription(value.ToString(), ListSortDirection.Ascending);
                                view.SortDescriptions.Add(sort);
                            }

                            // сортировка по порядку всегда есть
                            var ord = new SortDescription(HrViewSortingColumn.Ord.ToString(), ListSortDirection.Ascending);
                            view.SortDescriptions.Add(ord);
                        }
                    }

                    _sort = value;

                    SetHrExtra(HealthRecords);
                    OnPropertyChanged(() => Sorting);
                    OnPropertyChanged(() => CanReorder);
                }
            }
        }

        public HrViewGroupingColumn Grouping
        {
            get
            {
                return _group;
            }
            set
            {
                if (_group != value)
                {
                    using (view.DeferRefresh())
                    {
                        // убрать сортировку по старой группировке, если Sorting не по ней
                        var oldGroupingSd = new SortDescription(Group2SortString(_group), ListSortDirection.Ascending);
                        var oldind = view.SortDescriptions.IndexOf(oldGroupingSd);
                        if (Sorting != Group2Sort(_group) && oldind >= 0)
                        {
                            view.SortDescriptions.RemoveAt(oldind);
                        }

                        view.GroupDescriptions.Clear();
                        if (value != HrViewGroupingColumn.None)
                        {
                            // сортировка по новой группировке первая
                            var groupingSd = new SortDescription(Group2SortString(value), ListSortDirection.Ascending);
                            var ind = view.SortDescriptions.IndexOf(groupingSd);
                            if (ind > 0)
                            {
                                view.SortDescriptions.Remove(groupingSd);
                            }
                            view.SortDescriptions.Insert(0, groupingSd);

                            var groupingGd = new PropertyGroupDescription(value.ToString());
                            view.GroupDescriptions.Add(groupingGd);
                        }
                    }
                    _group = value;

                    SetHrExtra(HealthRecords);
                    OnPropertyChanged(() => Grouping);
                }
            }
        }

        private HrViewGroupingColumn Sort2Group(HrViewSortingColumn col)
        {
            switch (col)
            {
                case HrViewSortingColumn.Category:
                    return HrViewGroupingColumn.Category;

                case HrViewSortingColumn.CreatedAt:
                    return HrViewGroupingColumn.GroupingCreatedAt;

                case HrViewSortingColumn.SortingDate:
                default:
                    return HrViewGroupingColumn.None;
            }
        }

        private HrViewSortingColumn? Group2Sort(HrViewGroupingColumn col)
        {
            switch (col)
            {
                case HrViewGroupingColumn.Category:
                    return HrViewSortingColumn.Category;

                case HrViewGroupingColumn.GroupingCreatedAt:
                    return HrViewSortingColumn.CreatedAt;

                case HrViewGroupingColumn.None:
                default:
                    return null;
            }
        }

        private object GetGroupObject(ShortHealthRecordViewModel vm)
        {
            switch (Grouping)
            {
                case HrViewGroupingColumn.Category:
                    return vm.Category;

                case HrViewGroupingColumn.GroupingCreatedAt:
                    return vm.GroupingCreatedAt;

                default:
                    break;
            }
            return null;
        }

        private void SetGroupObject(ShortHealthRecordViewModel vm, CollectionViewGroup group)
        {
            if (group == null)
                return;

            switch (Grouping)
            {
                case HrViewGroupingColumn.Category:
                    // перенос в другую группу меняет категорию
                    Contract.Assume(group.Name is HrCategory);
                    vm.healthRecord.Category = group.Name as HrCategory;
                    break;

                case HrViewGroupingColumn.GroupingCreatedAt:
                default:
                    break;
            }
        }

        public bool CanMove(IEnumerable<ShortHealthRecordViewModel> vms, CollectionViewGroup group)
        {
            // группы не важны
            if (group == null)
            {
                Contract.Assume(Grouping == HrViewGroupingColumn.None);
                return CanReorder;
            }
            return false;

            // изменяем порядок внутри группы
            switch (Grouping)
            {
                case HrViewGroupingColumn.Category:
                    return true;

                case HrViewGroupingColumn.GroupingCreatedAt:
                    return vms.All(vm => vm.GroupingCreatedAt == (group.Name as string)); // если все в одной группе
                default:
                    break;
            }
            return false;
        }

        private string Group2SortString(HrViewGroupingColumn col)
        {
            return col.ToString();
        }

        private void SetHrExtra(IList<ShortHealthRecordViewModel> vms)
        {
            // Показываем то, по чему сортируем, если нет группировки по этому же полю.

            Action<ShortHealthRecordViewModel> setter = (vm) => vm.SortingExtraInfo = "";

            switch (Sorting)
            {
                case HrViewSortingColumn.Category:
                    if (Grouping != HrViewGroupingColumn.Category)
                        setter = (vm) => vm.SortingExtraInfo = vm.Category != null ? vm.Category.ToString() : "";
                    break;

                case HrViewSortingColumn.CreatedAt:
                    if (Grouping != HrViewGroupingColumn.GroupingCreatedAt)
                        setter = (vm) => vm.SortingExtraInfo = string.Format("{0} {1:H:mm}", DateFormatter.GetDateString(vm.CreatedAt), vm.CreatedAt);
                    break;

                case HrViewSortingColumn.SortingDate:
                    // дата записи всегда видна
                    break;

                default: // Ord
                    break;
            }

            vms.ForAll(setter);
        }

        public void Cut()
        {
            logger.Debug("cut");
            Copy();
            hrManager.DeleteCheckedHealthRecords(withCancel: false);
        }

        public void Copy()
        {
            var hrs = hrManager.GetSelectedHrs();
            var hrInfos = hrs.Select(hr => new HrData.HrInfo()
            {
                HolderId = (Guid)hr.Holder.Id,
                DoctorId = hr.Doctor.Id,
                CategoryId = hr.Category != null ? (int?)hr.Category.Id : null,
                FromDay = hr.FromDay,
                FromMonth = hr.FromMonth,
                FromYear = hr.FromYear,
                Unit = hr.Unit,
                Hios = new List<IHrItemObject>(hr.HrItems.Select(x => x.Entity))
            }).ToList();

            var data = new HrData() { Hrs = hrInfos };

            var strings = string.Join(".\n", hrs.Select(hr => string.Join(", ", hr.GetOrderedEntities()))) + ".";

            IDataObject dataObj = new DataObject(HrData.DataFormat.Name, data);
            dataObj.SetData(System.Windows.DataFormats.UnicodeText, strings);
            Clipboard.SetDataObject(dataObj, false);

            LogHrs("copy", hrInfos);
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
                syncHios(data.ItemObjects);

                var hrs = hrManager.GetSelectedHrs();
                if (hrs.Count > 0)
                {
                    // add hios to end of Selected Hrs
                    hrs.ForAll(hr => hr.AddItems(data.ItemObjects));
                    OnSaveNeeded(hrManager.GetSelectedHrs());
                }
                else
                {
                    // new hr with pasted hios
                    var newHR = AddHr();
                    newHR.AddItems(data.ItemObjects);
                    OnSaveNeeded(); // save all
                }
                LogHrItemObjects("paste", data.ItemObjects);
            }

            HrData hrDat = null;

            if (ido.GetDataPresent(HrData.DataFormat.Name))
            {
                hrDat = (HrData)ido.GetData(HrData.DataFormat.Name);
            }
            if (hrDat != null)
            {
                // paste hrs TODO before first Selected
                var index = HealthRecords.IndexOf(SelectedHealthRecord);
                var pasted = new List<HealthRecord>();
                foreach (var hr2 in hrDat.Hrs)
                {
                    if (hr2 == null) continue;

                    var newHr = AddHr();
                    // vm уже добавлена
                    var newVm = HealthRecords.FirstOrDefault(vm => vm.healthRecord == newHr);
                    Debug.Assert(newVm != null);
                    fillHr(newHr, hr2);
                    // теперь запись заполнена

                    pasted.Add(newHr);
                }
                OnSaveNeeded(); // save all

                SelectHealthRecords(pasted);
                LogHrs("paste", hrDat.Hrs);
            }
        }

        public override string ToString()
        {
            return "HrList for " + holder.ToString();
        }

        /// <summary>
        /// Делает запись текущей выделенной.
        /// </summary>
        /// <param name="healthRecord"></param>
        /// <param name="addToSelected">Не снимать выделение с других выделенных.</param>
        internal void SelectHealthRecord(HealthRecord healthRecord, bool addToSelected = false)
        {
            var toSelect = HealthRecords.FirstOrDefault(vm => vm.healthRecord == healthRecord);
            if (!addToSelected)
            {
                HealthRecords.Except(toSelect.ToEnumerable()).ForAll(vm => vm.IsSelected = false);
                SelectedHealthRecord = toSelect;
            }
            else if (toSelect != null)
            {
                using (noUnselectPrev.Enter())
                {
                    SelectedHealthRecord = toSelect;
                }
            }
            else
            {
                // записи нет в списке, не меняем SelectedHealthRecord
            }
        }

        /// <summary>
        /// Устанавливает выделение на записях, последняя — текущая выделенная.
        /// </summary>
        /// <param name="hrs"></param>
        internal void SelectHealthRecords(IEnumerable<HealthRecord> hrs)
        {
            var toSelect = HealthRecords.Where(vm => hrs.Contains(vm.healthRecord)).ToList();
            HealthRecords.Except(toSelect).ForAll(vm => vm.IsSelected = false);
            toSelect.ForAll(vm => vm.IsSelected = true);

            SelectHealthRecord(hrs.LastOrDefault(), true);
        }

        private HealthRecord AddHr(bool fromCommand = false)
        {
            AddingHrByCommnd = fromCommand;
            var newHr = holder.AddHealthRecord(AuthorityController.CurrentDoctor);
            AddingHrByCommnd = false;
            return newHr;
        }

        private void LogHrs(string action, IEnumerable<HrData.HrInfo> hrs)
        {
            logger.DebugFormat("{0} hrs with hios: {1}", action, string.Join("\n", hrs.Select((hr, i) => string.Format("{0} {1}", i, hr.Hios.FlattenString()))));
        }

        private void LogHrItemObjects(string action, IEnumerable<IHrItemObject> hios)
        {
            logger.DebugFormat("{0} hios: {1}", action, hios.FlattenString());
        }

        protected virtual void OnSaveNeeded(List<HealthRecord> hrsToSave = null)
        {
            var h = SaveNeeded;
            if (h != null)
            {
                h(this, new ListEventArgs<HealthRecord>(hrsToSave));
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    hrManager.Dispose();
                    HolderVm.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public class DropTargetHandler : DefaultDropHandler
        {
            private readonly HrListViewModel master;

            public DropTargetHandler(HrListViewModel master)
            {
                this.master = master;
            }

            public bool FromSameCollection(IDropInfo dropInfo)
            {
                var sourceList = dropInfo.DragInfo.SourceCollection.ToList();
                var targetList = dropInfo.TargetCollection.ToList();
                return Equals(sourceList, targetList);
            }

            public bool FromAutocomplete(IDropInfo dropInfo)
            {
                var sourceList = dropInfo.DragInfo.SourceCollection.ToList();
                return sourceList is IEnumerable<TagViewModel>;
            }

            public override void DragOver(IDropInfo dropInfo)
            {
                var data = ExtractData(dropInfo.Data).Cast<object>();
                if (dropInfo.DragInfo == null || dropInfo.DragInfo.SourceCollection == null || data.Count() == 0)
                {
                    dropInfo.Effects = DragDropEffects.None;
                }
                else if (FromSameCollection(dropInfo))
                {
                    var vms = ExtractData(dropInfo.Data).Cast<ShortHealthRecordViewModel>();
                    if (master.CanMove(vms, dropInfo.TargetGroup))
                    {
                        dropInfo.Effects = DragDropEffects.Move;
                        dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                    }
                }
                else if (FromAutocomplete(dropInfo))
                {
                    dropInfo.Effects = DragDropEffects.Copy;
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                }
                else
                {
                    dropInfo.Effects = DragDropEffects.Scroll;
                }
            }

            public override void Drop(IDropInfo dropInfo)
            {
                var data = ExtractData(dropInfo.Data).Cast<object>();

                //  logger.DebugFormat("ddrop {0} {1}", data.Count(), data.First().GetType());

                var insertView = dropInfo.InsertIndex;
                ShortHealthRecordViewModel hrView;
                if (FromSameCollection(dropInfo))
                {
                    // drop hrs from hrslist
                    var hrs = data.Cast<ShortHealthRecordViewModel>().ToList();
                    //var targetList = dropInfo.TargetCollection.ToList();

                    //  logger.DebugFormat("selected bef {0} ", master.SelectedHealthRecords.Count());

                    // reorder hrs

                    // view even with sorting can update after moving
                    var view = master.view.Cast<ShortHealthRecordViewModel>().ToList();

                    foreach (var hr in hrs)
                    {
                        var old = master.HealthRecords.IndexOf(hr);
                        var oldView = view.IndexOf(hr);

                        // insertView [0..view.Count]

                        // insert above group border or at the end
                        bool aboveBorderOrAtEnd = insertView == view.Count;
                        if (0 < insertView && insertView < view.Count && dropInfo.TargetGroup != null)
                        {
                            // gong can show adoner in both groups, above and below border

                            hrView = (ShortHealthRecordViewModel)view[insertView];
                            var hrPrevView = (ShortHealthRecordViewModel)view[insertView - 1];

                            // var groups = master.view.Groups;
                            // разные группы и у верхнего элемента — целевая
                            aboveBorderOrAtEnd = master.GetGroupObject(hrPrevView) != master.GetGroupObject(hrView)
                                && dropInfo.TargetGroup.Name == master.GetGroupObject(hrPrevView);
                        }

                        // сравниваем с элементом ниже указателя или с тем, что над границей/над концом списка, если указатель прямо под ним
                        var toCompareView = aboveBorderOrAtEnd ? insertView - 1 : insertView;
                        var toCompareHr = (ShortHealthRecordViewModel)view[toCompareView];
                        var toCompare = master.HealthRecords.IndexOf(toCompareHr);

                        // in view: always move before next element, but after last in group

                        // перемещаем на место элемента, с которым сравниваем,
                        // если над границей - на место следующего
                        int moveBeforeItemAt = aboveBorderOrAtEnd ? toCompare + 1 : toCompare;
                        var down = old < moveBeforeItemAt; // move = remove + insert
                        var dest = down ? moveBeforeItemAt - 1 : moveBeforeItemAt;

                        //if (aboveBorderOrAtEnd)
                        //    dest++;
                        //if (old < dest)
                        //    dest--;

                        logger.DebugFormat("move {0}({1}) - {2}({3}) \n{4} compareTo {5}\n aboveBorderOrAtEnd = {6}\ndropInfo.TargetGroup {7}",
                            old, oldView, dest, insertView,
                            hr, toCompareHr,
                            aboveBorderOrAtEnd,
                            dropInfo.TargetGroup);
                        if (old != dest)
                        {
                            master.HealthRecords.Move(old, dest);
                        }
                    }

                    // view refreshed after this
                    foreach (var hr in hrs)
                    {
                        if (dropInfo.TargetGroup != null)
                            master.SetGroupObject(hr, dropInfo.TargetGroup);
                    }
                    //  logger.DebugFormat("selected after dd {0} ", master.SelectedHealthRecords.Count());
                }
                else if (FromAutocomplete(dropInfo))
                {
                    // drop tags from autocomplete

                    var tags = data.Cast<TagViewModel>();

                    // new hr from tags
                    var newHR = master.AddHr();
                    var items = tags.Select(t => t.Entity).ToList();
                    newHR.SetItems(items);
                }
                master.OnSaveNeeded();
                logger.DebugFormat("selected after save {0} ", master.SelectedHealthRecords.Count());
            }
        }

        public class DragSourceHandler : IDragSource
        {
            /// <summary>
            /// Data is hrs vm.
            /// </summary>
            /// <param name="dragInfo"></param>
            public void StartDrag(IDragInfo dragInfo)
            {
                var hrs = dragInfo.SourceItems.Cast<ShortHealthRecordViewModel>();
                var itemCount = hrs.Count();

                if (itemCount == 1)
                {
                    dragInfo.Data = hrs.First();
                }
                else if (itemCount > 1)
                {
                    dragInfo.Data = GongSolutions.Wpf.DragDrop.Utilities.TypeUtilities.CreateDynamicallyTypedList(hrs);
                }
                dragInfo.Effects = (dragInfo.Data != null) ?
                                     DragDropEffects.Copy | DragDropEffects.Move :
                                     DragDropEffects.None;
            }

            public bool CanStartDrag(IDragInfo dragInfo)
            {
                return dragInfo.SourceItems.Cast<ShortHealthRecordViewModel>().Count() > 0;
            }

            public void DragCancelled()
            {
            }

            public void Dropped(IDropInfo dropInfo)
            {
            }
        }
    }

    [Serializable]
    public class HrData
    {
        public static readonly DataFormat DataFormat = DataFormats.GetDataFormat("hr");

        public List<HrInfo> Hrs { get; set; }

        [Serializable]
        public class HrInfo
        {
            public Guid HolderId { get; set; }

            public Guid DoctorId { get; set; }

            public int? CategoryId { get; set; }

            public int? FromDay { get; set; }

            public int? FromMonth { get; set; }

            public int? FromYear { get; set; }

            public HealthRecordUnit Unit { get; set; }

            public List<IHrItemObject> Hios { get; set; }
        }
    }
}