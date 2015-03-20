using Diagnosis.Common;
using Diagnosis.Common.Util;
using Diagnosis.Models;
using Diagnosis.Models.Enums;
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
    public partial class HrListViewModel : ViewModelBase
    {
        /// <summary>
        /// When fixing duplicates in List.SelectedItems
        /// </summary>
        public bool inRemoveDup;

        /// <summary>
        /// If set, selection changes not meaningfull.
        /// </summary>
        internal readonly FlagActionWrapper<IEnumerable<ShortHealthRecordViewModel>> preserveSelected;

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HrListViewModel));
        private static HrViewer hrViewer = new HrViewer();

        internal readonly IHrsHolder holder;
        internal readonly HealthRecordManager hrManager;
        private readonly ListCollectionView view;
        private ShortHealthRecordViewModel _selectedHealthRecord;
        private ShortHealthRecordViewModel _selectedCopy;
        private List<ShortHealthRecordViewModel> selectedOrder = new List<ShortHealthRecordViewModel>();
        private ReentrantFlag unselectPrev = new ReentrantFlag();
        private ReentrantFlag doNotNotifySelectedChanged = new ReentrantFlag();
        private FlagActionWrapper doNotNotifyLastSelectedChanged;
        private HrViewSortingColumn _sort = HrViewSortingColumn.SortingDate; // to change in ctor
        private HrViewGroupingColumn _group = HrViewGroupingColumn.GroupingCreatedAt;
        private bool _rectSelect;
        private bool _canReorder;
        private bool _focused;
        private bool inSetSelected;
        private bool disposed;
        private VisibleRelayCommand<bool> _moveHr;

        public HrListViewModel(IHrsHolder holder, Action<HealthRecord, HrData.HrInfo> filler, Action<IList<ConfindenceHrItemObject>> syncer)
        {
            Contract.Requires(holder != null);
            Contract.Requires(filler != null);
            Contract.Requires(syncer != null);
            this.holder = holder;
            this.fillHr = filler;
            this.syncHios = syncer;

            HolderVm = new HolderViewModel(holder);

            doNotNotifyLastSelectedChanged = new FlagActionWrapper(() =>
            {
                OnPropertyChanged(() => LastSelected);
                logger.DebugFormat("(bulk) selected in order\n{0}", string.Join("\n", selectedOrder));
            });

            preserveSelected = new FlagActionWrapper<IEnumerable<ShortHealthRecordViewModel>>((hrs) => { hrs.ForEach(vm => vm.IsSelected = true); });
            hrManager = new HealthRecordManager(holder, onHrVmPropChanged: (s, e) =>
            {
                var hrvm = s as ShortHealthRecordViewModel;
                if (hrvm != null)
                {
                    if (e.PropertyName == "IsSelected")
                    {
                        // simulate IsSynchronizedWithCurrentItem for Extended mode
                        // SelectedHealthRecord points to last IsSelected without unselect prev
                        // select may be by IsSelected (rect), so need to set SelectedHealthRecord
                        if (hrvm.IsSelected)
                        {
                            if (!selectedOrder.Contains(hrvm))
                                selectedOrder.Add(hrvm);
                            else
                                logger.DebugFormat("selectedOrder contains {0}", hrvm);

                            using (unselectPrev.Join())
                            {
                                if (SelectedHealthRecords.Count() > 1)
                                    // dont's notify SelectedChanged to save selection
                                    using (doNotNotifySelectedChanged.Join())
                                    {
                                        SelectedHealthRecord = hrvm;
                                    }
                                else
                                    SelectedHealthRecord = hrvm;
                            }
                            logger.DebugFormat("select {0}", hrvm);
                        }
                        else if (!inRemoveDup)
                        {
                            selectedOrder.Remove(hrvm);
                            logger.DebugFormat("unselect {0}", hrvm);

                            // Сняли выделение, фокус остался — enter будет открывать этот элемент, а выделен другой. Это ок.
                            // Выбранным становится последний.

                            if (SelectedHealthRecord == hrvm)
                                using (doNotNotifySelectedChanged.Join())
                                {
                                    SelectedHealthRecord = LastSelected;
                                }
                        }
                        if (doNotNotifyLastSelectedChanged.CanEnter)
                        {
                            OnPropertyChanged(() => LastSelected);
                            logger.DebugFormat("selected in order\n{0}", string.Join("\n", selectedOrder));
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
            hrManager.DeletedHealthRecords.CollectionChangedWrapper += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    // удалены

                    // выделяем первую после удаленной записи, чтобы она была выделена для фокуса на ней
                    var del = e.NewItems.Cast<ShortHealthRecordViewModel>().ToList();
                    logger.DebugFormat("deleted {0}", del);
                    SelectedHealthRecord = HealthRecordsView.FirstAfterAndNotIn(del);

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

            HealthRecords.CollectionChangedWrapper += (s, e) =>
            {
                // если запись IsDeleted
                HolderVm.UpdateIsEmpty();

                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    // новые в списке
                    SetHrExtra(e.NewItems.Cast<ShortHealthRecordViewModel>().ToList());
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    // убранные не выделены
                    var removed = e.OldItems.Cast<ShortHealthRecordViewModel>().ToList();
                    if (removed.Contains(SelectedHealthRecord))
                    {
                        SelectedHealthRecord = null;
                    }
                    using (doNotNotifyLastSelectedChanged.Join())
                    {
                        removed.ForAll(vm => vm.IsSelected = false);
                    }
                }
            };

            view = (ListCollectionView)CollectionViewSource.GetDefaultView(HealthRecords);
            Grouping = HrViewGroupingColumn.Category;
            Sorting = HrViewSortingColumn.Ord;
            SetHrExtra(HealthRecords);

            DropHandler = new DropTargetHandler(this);
            DragHandler = new DragSourceHandler();

            IsDragSourceEnabled = true;
            IsDropTargetEnabled = true;
            IsRectSelectEnabled = true;

            SelectHealthRecord(hrViewer.GetLastSelectedFor(holder));
        }

        public event EventHandler<ListEventArgs<HealthRecord>> SaveNeeded;
        public HolderViewModel HolderVm { get; private set; }

        public INCCReadOnlyObservableCollection<ShortHealthRecordViewModel> HealthRecords { get { return hrManager.HealthRecords; } }

        /// <summary>
        /// Visible HealthRecords in right order (with sorting, grouping, filtering).
        /// </summary>
        public ReadOnlyCollection<ShortHealthRecordViewModel> HealthRecordsView
        {
            // View even with sorting can update after moving?
            get { return view.Cast<ShortHealthRecordViewModel>().ToList().AsReadOnly(); }
        }

        /// <summary>
        /// To toggle editor for last selected item, not first in listbox selection.
        /// </summary>
        public ShortHealthRecordViewModel LastSelected
        {
            get
            {
                return selectedOrder.LastOrDefault();
            }
        }

        public ShortHealthRecordViewModel SelectedHealthRecord
        {
            get { return _selectedHealthRecord; }
            set
            {
                if (_selectedHealthRecord == value)
                    return;

                if (value == null)
                    if (inSetSelected)
                    {
                        // list box sets value to null, skip if we are in setting new value
                        logger.DebugFormat("set null inSetSelected");
                        return;
                    }
                    else
                    {
                        // nothing selected
                        using (doNotNotifyLastSelectedChanged.Join())
                        {
                            hrManager.UnselectAll();
                        }
                    }

                inSetSelected = true;
                if (_selectedHealthRecord != null && unselectPrev.CanEnter)
                {
                    // снимаем выделение с прошлой выделенной
                    using (unselectPrev.Join())
                    {
                        _selectedHealthRecord.IsSelected = false;
                    }
                }
                logger.DebugFormat("hrList selected {0} -> {1}", _selectedHealthRecord, value);
                _selectedHealthRecord = value;
#if DEBUG
                SelectedCopy = value;
#endif
                if (value != null)
                {
                    hrViewer.Select(value.healthRecord, holder);
                    value.IsSelected = true;
                    Contract.Assume(selectedOrder.Contains(value));
                }

                if (doNotNotifySelectedChanged.CanEnter)
                    OnPropertyChanged(() => SelectedHealthRecord);
                inSetSelected = false;
            }
        }

#if DEBUG

        /// <summary>
        /// When doNotNotifySelectedChanged, use for debug.
        /// </summary>
        public ShortHealthRecordViewModel SelectedCopy
        {
            get
            {
                return _selectedCopy;
            }
            set
            {
                if (_selectedCopy != value)
                {
                    _selectedCopy = value;
                    OnPropertyChanged(() => SelectedCopy);
                }
            }
        }

#endif

        public IEnumerable<ShortHealthRecordViewModel> SelectedHealthRecords
        {
            get { return HealthRecords.Where(vm => vm.IsSelected).ToList(); }
        }


        #region Commands

        public ICommand AddHealthRecordCommand
        {
            get
            {
                return new RelayCommand(() =>
                    {

                        var stratEdit = true;
                        this.Send(Event.AddHr, new object[] { holder, stratEdit }.AsParams(MessageKeys.Holder, MessageKeys.Boolean));

                        // async?                       
                    });
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

                            var current = HealthRecordsView.IndexOf(SelectedHealthRecord);
                            if (up)
                            {
                                if (current > 0)
                                    SelectedHealthRecord = HealthRecordsView[current - 1];
                                else
                                    SelectedHealthRecord = HealthRecordsView.Last(); // select last when Selected == null
                            }
                            else
                            {
                                if (current != HealthRecordsView.Count - 1)
                                    SelectedHealthRecord = HealthRecordsView[current + 1]; // select fisrt when Selected == null
                                else
                                    SelectedHealthRecord = HealthRecordsView.First();
                            }

                            using (doNotNotifyLastSelectedChanged.Enter())
                            {
                                hrManager.UnselectExcept(SelectedHealthRecord);
                            }

                            if (HealthRecordsView.Any(x => x.IsFocused))
                                SelectedHealthRecord.IsFocused = true;
                        });
            }
        }

        public VisibleRelayCommand<bool> MoveHrCommand
        {
            get
            {
                return _moveHr ?? (_moveHr = new VisibleRelayCommand<bool>((up) =>
                {
                    if (SelectedHealthRecord == null || !CanReorder)
                        return;

                    logger.DebugFormat("begin move hrs, up={0}", up);

                    var hrs = SelectedHealthRecords;
                    var selectedInd = hrs.Select(v => HealthRecordsView.IndexOf(v));
                    var allNear = selectedInd.OrderBy(x => x).IsSequential();

                    int current;
                    if (allNear)
                        current = up ? selectedInd.Min() : selectedInd.Max();
                    else
                        current = HealthRecordsView.IndexOf(SelectedHealthRecord);

                    int newIndex = current;
                    if (up)
                    {
                        if (current > 0)
                            newIndex = current - 1;
                    }
                    else
                    {
                        if (current < HealthRecordsView.Count - 1)
                            newIndex = current + 1;
                    }

                    // разные группы
                    var border = GetGroupObject(HealthRecordsView[newIndex]) != GetGroupObject(SelectedHealthRecord);
                    var group = GetGroupObject(HealthRecordsView[newIndex]);

                    if (up && border)
                        newIndex = current;
                    // вниз — через 2, если не над границей группы и не в конце
                    if (!up && !border)
                        newIndex++;

                    Reorder(hrs, newIndex, group);

                    logger.DebugFormat("end move hrs");
                }, (up) =>
                {
                    if (SelectedHealthRecord == null || !CanReorder)
                        return false;

                    var selectedInd = SelectedHealthRecords.Select(v => HealthRecordsView.IndexOf(v));
                    var allNear = selectedInd.OrderBy(x => x).IsSequential();

                    int current;
                    if (allNear)
                        current = up ? selectedInd.Min() : selectedInd.Max();
                    else
                        current = HealthRecordsView.IndexOf(SelectedHealthRecord);

                    int newIndex = current;
                    if (up)
                        return current > 0;
                    else
                        return current < HealthRecordsView.Count - 1;
                }));
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

        public bool CanReorder
        {
            get
            {
                return _canReorder;
            }
            set
            {
                if (_canReorder != value)
                {
                    _canReorder = value;

                    MoveHrCommand.IsVisible = value;
                    HealthRecords.ForAll(x => x.IsDraggable = value);
                    OnPropertyChanged(() => CanReorder);
                }
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
                    CanReorder = (Sorting == HrViewSortingColumn.Ord
#if DEBUG
 || Sorting == HrViewSortingColumn.None
#endif
);
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

        /// <summary>
        ///
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="group">CollectionViewGroup or CollectionViewGroup.Name.</param>
        private void SetGroupObject(ShortHealthRecordViewModel vm, object group)
        {
            if (group == null)
                return;
            if (group is CollectionViewGroup)
                group = ((CollectionViewGroup)group).Name;

            switch (Grouping)
            {
                case HrViewGroupingColumn.Category:
                    // перенос в другую группу меняет категорию
                    Contract.Assume(group is HrCategory);
                    vm.healthRecord.Category = group as HrCategory;
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

        private void Reorder(IEnumerable<object> data, int insertView, object group)
        {
            Contract.Requires(data.All(o => o is ShortHealthRecordViewModel));
            // don't change selection
            Contract.Ensures(Contract.OldValue<IEnumerable<ShortHealthRecordViewModel>>(SelectedHealthRecords).ScrambledEquals(SelectedHealthRecords));

            var hrs = data.Cast<ShortHealthRecordViewModel>().ToList();

            // insertView [0..view.Count]
            bool aboveBorder = false;
            if (0 < insertView && insertView < view.Count && group != null)
            {
                // gong can show adoner in both groups, above and below border

                var hrView = HealthRecordsView[insertView];
                var hrPrevView = HealthRecordsView[insertView - 1];

                // var groups = view.Groups;
                // разные группы и у верхнего элемента — целевая
                aboveBorder = GetGroupObject(hrPrevView) != GetGroupObject(hrView)
                    && group == GetGroupObject(hrPrevView);
            }

            hrManager.Reorder(hrs, HealthRecordsView, insertView, group, aboveBorder, SetGroupObject);
        }

        private void SetHrExtra(IList<ShortHealthRecordViewModel> vms)
        {
            // Показываем то, по чему сортируем, если нет группировки по этому же полю.

            Action<ShortHealthRecordViewModel> setter = (vm) => vm.SortingExtraInfo = "";

            switch (Sorting)
            {
                case HrViewSortingColumn.Category:
                    if (Grouping != HrViewGroupingColumn.Category)
                        setter = (vm) => vm.SortingExtraInfo = vm.Category != null && vm.Category != HrCategory.Null ? vm.Category.ToString() : "";
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
            if (!addToSelected) // смена выделенного
            {
                using (doNotNotifyLastSelectedChanged.Enter())
                {
                    hrManager.UnselectExcept(toSelect);
                }
                SelectedHealthRecord = toSelect;
            }
            else if (toSelect != null) // добавление к выделенным, выделяем последнюю
            {
                using (unselectPrev.Enter())
                {
                    using (doNotNotifySelectedChanged.Enter()) // без этого addToSelected: false - выделяется одна, после выхода из редактора снова можно вернуть веделение с шифтом
                    {
                        SelectedHealthRecord = toSelect;
                    }
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
            hrManager.UnselectExcept(toSelect);
            toSelect.ForAll(vm => vm.IsSelected = true);

            SelectHealthRecord(hrs.LastOrDefault(), true);
        }

        internal static void ResetHistory()
        {
            hrViewer = new HrViewer();
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            // может: LastSelected - есть, но ничего не выбрано
            // не может: что-то выбрано, а LastSelected нет

            // снимаем выделение: сначала менять SelectedHealthRecord, потом все остальные
            // добавляем выделение - наоборот
            Contract.Invariant(disposed || LastSelected != null || SelectedHealthRecord == null);

            // без повторов
            Contract.Invariant(selectedOrder.Distinct().Count() == selectedOrder.Count());

            //Contract.Invariant(disposed || SelectedHealthRecord == null || SelectedHealthRecord.IsSelected);
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
                    selectedOrder.Clear();
                    disposed = true;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }


    }

    [Serializable]
    public class HrData
    {
        public static readonly DataFormat DataFormat = DataFormats.GetDataFormat("hr");
        private IList<HrInfo> hrs;

        public IList<HrInfo> Hrs { get { return hrs; } }

        public HrData(IList<HrInfo> hrs)
        {
            this.hrs = hrs;
        }

        [Serializable]
        public class HrInfo
        {
            public Guid HolderId { get; set; }

            public Guid DoctorId { get; set; }

            public Guid? CategoryId { get; set; }

            public int? FromDay { get; set; }

            public int? FromMonth { get; set; }

            public int? FromYear { get; set; }

            public HealthRecordUnit Unit { get; set; }

            public List<ConfindenceHrItemObject> Chios { get; set; }
        }
    }
}