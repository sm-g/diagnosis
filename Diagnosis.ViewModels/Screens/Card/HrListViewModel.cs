using Diagnosis.Common;
using Diagnosis.Common.Util;
using Diagnosis.Models;
using Diagnosis.Models.Enums;
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
    [DebuggerDisplay("HrList for {holder}")]
    public partial class HrListViewModel : ViewModelBase
    {
        /// <summary>
        /// When fixing duplicates in List.SelectedItems
        /// </summary>
        public bool inRemoveDup;

        /// <summary>
        /// If set, selection changes not meaningfull.
        /// </summary>
        internal readonly FlagActionWrapper<IList<ShortHealthRecordViewModel>> preserveSelected;

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
        private HrViewColumn _sort = HrViewColumn.None; // to change in ctor
        private HrViewColumn _group = HrViewColumn.None;
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
            Sortings = new List<HrViewColumn>() {
#if DEBUG
               HrViewColumn.None,
#endif
               HrViewColumn.Ord,
               HrViewColumn.Category,
               HrViewColumn.Date,
               HrViewColumn.CreatedAt,
            };
            Groupings = new List<HrViewColumn>() {
               HrViewColumn.None,
               HrViewColumn.Category,
               HrViewColumn.CreatedAt,
            };

            var sortPropNames = Sortings.Select(x => x.ToSortingProperty());
            var groupPropNames = Groupings.Select(x => x.ToGroupingProperty());

            doNotNotifyLastSelectedChanged = new FlagActionWrapper(() =>
            {
                OnPropertyChanged(() => LastSelected);
                logger.DebugFormat("(bulk) selected in order\n{0}", string.Join("\n", selectedOrder));
            });

            preserveSelected = new FlagActionWrapper<IList<ShortHealthRecordViewModel>>((hrs) =>
            {
                hrs.ForEach(vm => vm.IsSelected = true);
                // fix new selected item appears in listbox after movement hrs from diff categories in grouped by category
                // TODO fix when diff createdAt
                HealthRecords.Except(hrs).ForEach(x => x.IsSelected = false);
            });

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
                    if (sortPropNames.Contains(e.PropertyName) ||
                        groupPropNames.Contains(e.PropertyName))
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

            Grouping = HrViewColumn.Category;
            Sorting = HrViewColumn.Ord;
            SetHrExtra(HealthRecords);

            DropHandler = new DropTargetHandler(this);
            DragHandler = new DragSourceHandler();

            IsDragSourceEnabled = true;
            IsDropTargetEnabled = true;
            IsRectSelectEnabled = true;

            SelectHealthRecord(hrViewer.GetLastSelectedFor(holder));
        }

        public event EventHandler<ListEventArgs<HealthRecord>> SaveNeeded;

        public IList<HrViewColumn> Sortings { get; private set; }

        public IList<HrViewColumn> Groupings { get; private set; }

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

        public IList<ShortHealthRecordViewModel> SelectedHealthRecords
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

                    int current = GetCurrentSelectedIndex(up);

                    object group;
                    bool crossBorder;
                    int newIndex = GetMovementTargetIndex(up, current, out group, out crossBorder);
                    //var x = newIndex < HealthRecordsView.Count ? HealthRecordsView[newIndex] : HealthRecordsView[newIndex - 1];
                    Reorder(SelectedHealthRecords, newIndex, group);

                    logger.DebugFormat("end move hrs");
                }, (up) =>
                {
                    if (SelectedHealthRecord == null || !CanReorder)
                        return false;

                    int current = GetCurrentSelectedIndex(up);
                    // нельзя перемещать
                    // за границы
                    if (up && current == 0 || !up && current >= HealthRecordsView.Count - 1)
                        return false;

                    object targetGroup;
                    bool crossBorder;
                    int newIndex = GetMovementTargetIndex(up, current, out targetGroup, out crossBorder);
                    // и в друрую группу, если нельзя дропнуть в нее первую выбранную запись
                    return CanDropTo(SelectedHealthRecord.ToEnumerable(), targetGroup);
                }));
            }
        }

        /// <summary>
        /// Индекс целевой позиции в представлении при переупорядочивании.
        /// </summary>
        /// <param name="up">Направление перемещения записей</param>
        /// <param name="current">Индекс текущей записи в представлении</param>
        /// <param name="targetGroup">Объект целевой группы</param>
        /// <param name="crossBorder">Целевая группа не совпадает с группой SelectedHealthRecord</param>
        /// <returns></returns>
        private int GetMovementTargetIndex(bool up, int current, out object targetGroup, out bool crossBorder)
        {
            int newIndex = current;
            if (up && current > 0)
                newIndex--;
            else if (!up && current < HealthRecordsView.Count - 1)
                newIndex++;

            targetGroup = GetGroupObject(HealthRecordsView[newIndex]);
            crossBorder = targetGroup != GetGroupObject(SelectedHealthRecord);

            // под границей вверх - оставляем индекс прежним, будет меняться группа
            // не над границей группы вниз — через 2 (move = remove + insert)
            if (up && crossBorder)
                newIndex = current;
            else if (!up && !crossBorder)
                newIndex++;

            // над границей вниз - меняем группу, в конце вниз - отдельный случай

            return newIndex;
        }

        /// <summary>
        /// Индекс текущей записи в представлении при переупорядочивании.
        /// </summary>
        /// <param name="up">Направление перемещения записей</param>
        /// <returns></returns>
        internal int GetCurrentSelectedIndex(bool up)
        {
            var selectedIndexes = SelectedHealthRecords.Select(v => HealthRecordsView.IndexOf(v));
            var allNear = selectedIndexes.OrderBy(x => x).IsSequential();

            int current;
            if (allNear)
                current = up ? selectedIndexes.Min() : selectedIndexes.Max(); // ближайшая по направлению перемещения
            else
                current = HealthRecordsView.IndexOf(SelectedHealthRecord); // первая запись
            return current;
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

        public HrViewColumn Sorting
        {
            get
            {
                return _sort;
            }
            set
            {
                Contract.Ensures(Grouping == Contract.OldValue(Grouping));
                Contract.Ensures(Sortings.Contains(Sorting));
                if (_sort != value)
                {
#if DEBUG
                    if (value == HrViewColumn.None)
                        view.SortDescriptions.Clear();
                    else
#endif
                        using (view.DeferRefresh())
                        {
                            view.SortDescriptions.Clear();

                            // сначала сортируем по группам
                            AddSortForGrouping(Grouping);

                            // основная сортировка
                            var sort = new SortDescription(value.ToSortingProperty(), ListSortDirection.Ascending);
                            if (view.SortDescriptions.IndexOf(sort) == -1)
                                view.SortDescriptions.Add(sort);

                            // сортировка по порядку всегда есть
                            var ordSd = new SortDescription(HrViewColumn.Ord.ToSortingProperty(), ListSortDirection.Ascending);
                            if (view.SortDescriptions.IndexOf(ordSd) == -1)
                                view.SortDescriptions.Add(ordSd);
                        }

                    _sort = value;

                    SetHrExtra(HealthRecords);
                    OnPropertyChanged(() => Sorting);
                    CanReorder = (Sorting == HrViewColumn.Ord
#if DEBUG
 || Sorting == HrViewColumn.None
#endif
);
                }
            }
        }

        public HrViewColumn Grouping
        {
            get
            {
                return _group;
            }
            set
            {
                Contract.Ensures(Sorting == Contract.OldValue(Sorting));
                Contract.Ensures(Groupings.Contains(Grouping));
                if (_group != value)
                {
                    using (view.DeferRefresh())
                    {
                        // убрать сортировку по старой группировке, если Sorting не по ней
                        // TODO заново отсортировать, если порядок сортировки при группировке другой
                        var oldGroupingSd = new SortDescription(_group.ToSortingProperty(), ListSortDirection.Ascending);
                        view.SortDescriptions.Remove(oldGroupingSd);
                        if (Sorting == _group && oldGroupingSd.PropertyName != null)
                            view.SortDescriptions.Insert(0, oldGroupingSd);

                        view.GroupDescriptions.Clear();
                        if (value != HrViewColumn.None)
                        {
                            AddSortForGrouping(value);

                            var groupingGd = new PropertyGroupDescription(value.ToGroupingProperty());
                            view.GroupDescriptions.Add(groupingGd);
                        }
                    }
                    _group = value;

                    SetHrExtra(HealthRecords);
                    OnPropertyChanged(() => Grouping);
                }
            }
        }

        /// <summary>
        /// Добавляет сортировку при группировке (первой).
        /// </summary>
        /// <param name="value">Колонка для группировки</param>
        private void AddSortForGrouping(HrViewColumn value)
        {
            Contract.Ensures(value == HrViewColumn.None ||
                view.SortDescriptions[0].PropertyName == value.ToSortingProperty());

            if (value == HrViewColumn.None)
                return;

            var groupingSd = new SortDescription(value.ToSortingProperty(), ListSortDirection.Ascending);
            view.SortDescriptions.Remove(groupingSd);
            view.SortDescriptions.Insert(0, groupingSd);
        }

        /// <summary>
        /// Can drop to group.
        /// </summary>
        /// <param name="vms"></param>
        /// <param name="group">CollectionViewGroup or CollectionViewGroup.Name.</param>
        /// <returns></returns>
        internal bool CanDropTo(IEnumerable<ShortHealthRecordViewModel> vms, object group)
        {
            if (!CanReorder)
                return false;

            if (group is CollectionViewGroup)
                group = ((CollectionViewGroup)group).Name;

            // группы не важны
            if (group == null)
            {
                Contract.Assume(Grouping == HrViewColumn.None);
                return CanReorder;
            }

            // изменяем порядок внутри группы
            switch (Grouping)
            {
                case HrViewColumn.Category:
                    return true;

                case HrViewColumn.CreatedAt:
                    Contract.Assume(group is HrCreatedAtOffset);
                    return vms.All(vm => vm.GroupingCreatedAt.Equals((HrCreatedAtOffset)group)); // если все в одной группе
                default:
                    break;
            }
            return false;
        }

        private void Reorder(IEnumerable<object> data, int insertView, object targetGroup)
        {
            Contract.Requires(data.All(o => o is ShortHealthRecordViewModel));
            // don't change selection
            Contract.Ensures(Contract.OldValue<IEnumerable<ShortHealthRecordViewModel>>(SelectedHealthRecords).ScrambledEquals(SelectedHealthRecords));

            var hrs = data.Cast<ShortHealthRecordViewModel>().ToList();

            // insertView [0..view.Count]
            bool aboveBorder = false;
            if (0 < insertView && insertView < view.Count && targetGroup != null)
            {
                // gong can show adoner in both groups, above and below border
                // --- gr1
                // ord 0
                // --- gr2
                // ord 1

                var hrView = HealthRecordsView[insertView];
                var hrPrevView = HealthRecordsView[insertView - 1];

                // разные группы и у верхнего элемента — целевая
                aboveBorder = GetGroupObject(hrPrevView) != GetGroupObject(hrView)
                    && targetGroup == GetGroupObject(hrPrevView);
            }

            hrManager.Reorder(hrs, HealthRecordsView, insertView, targetGroup, aboveBorder, SetGroupObject);
        }

        internal object GetGroupObject(ShortHealthRecordViewModel vm)
        {
            switch (Grouping)
            {
                case HrViewColumn.Category:
                    return vm.Category;

                case HrViewColumn.CreatedAt:
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
                case HrViewColumn.Category:
                    // перенос в другую группу меняет категорию
                    Contract.Assume(group is HrCategory);
                    vm.healthRecord.Category = group as HrCategory;
                    break;

                case HrViewColumn.CreatedAt:
                default:
                    break;
            }
        }

        private void SetHrExtra(IList<ShortHealthRecordViewModel> vms)
        {
            // Показываем то, по чему сортируем, если нет группировки по этому же полю.

            Action<ShortHealthRecordViewModel> setter = (vm) => vm.SortingExtraInfo = "";

            switch (Sorting)
            {
                case HrViewColumn.Category:
                    if (Grouping != HrViewColumn.Category)
                        setter = (vm) => vm.SortingExtraInfo = vm.Category != null && vm.Category != HrCategory.Null ? vm.Category.ToString() : "";
                    break;

                case HrViewColumn.CreatedAt:
                    if (Grouping != HrViewColumn.CreatedAt)
                        setter = (vm) => vm.SortingExtraInfo = string.Format("{0} {1:H:mm}", DateFormatter.GetDateString(vm.CreatedAt), vm.CreatedAt);
                    break;

                case HrViewColumn.Date:
                    // дата записи всегда видна
                    break;

                default: // Ord
                    break;
            }

            vms.ForAll(setter);
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