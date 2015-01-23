using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class HealthRecordManager : DisposableBase
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HealthRecordManager));
        private readonly IHrsHolder holder;
        private readonly PropertyChangedEventHandler onHrVmPropChanged;
        private readonly ObservableCollection<ShortHealthRecordViewModel> inner;
        private readonly ObservableCollection<ShortHealthRecordViewModel> healthRecords;
        private readonly ObservableCollection<ShortHealthRecordViewModel> deletedHealthRecords;

        public HealthRecordManager(IHrsHolder holder, PropertyChangedEventHandler onHrVmPropChanged)
        {
            this.holder = holder;
            this.onHrVmPropChanged = onHrVmPropChanged;
            holder.HealthRecordsChanged += holder_HealthRecordsChanged;

            var hrs = holder.HealthRecords
                .OrderBy(hr => hr.Ord)
                .ToList();

            for (int i = 0; i < hrs.Count; i++)
            {
                hrs[i].Ord = i;
            }
            // all hrs with different order now

            var hrVMs = hrs
                .Select(hr => CreateViewModel(hr))
                .ToList();

            inner = new ObservableCollection<ShortHealthRecordViewModel>(hrVMs);

            healthRecords = new ObservableCollection<ShortHealthRecordViewModel>(hrVMs
                 .Where(hr => !hr.IsDeleted));
            deletedHealthRecords = new ObservableCollection<ShortHealthRecordViewModel>(hrVMs
                  .Where(hr => hr.IsDeleted));

            HealthRecords = new INCCReadOnlyObservableCollection<ShortHealthRecordViewModel>(healthRecords);
            DeletedHealthRecords = new INCCReadOnlyObservableCollection<ShortHealthRecordViewModel>(deletedHealthRecords);

            inner.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach (ShortHealthRecordViewModel vm in e.NewItems)
                    {
                        healthRecords.Add(vm);
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    foreach (ShortHealthRecordViewModel vm in e.OldItems)
                    {
                        healthRecords.Remove(vm);
                        deletedHealthRecords.Remove(vm);
                    }
                }
            };
        }

        public INCCReadOnlyObservableCollection<ShortHealthRecordViewModel> HealthRecords { get; private set; }

        public INCCReadOnlyObservableCollection<ShortHealthRecordViewModel> DeletedHealthRecords { get; set; }

        public List<HealthRecord> GetSelectedHrs()
        {
            return HealthRecords.Where(hr => hr.IsChecked)
                                .Select(vm => vm.healthRecord).ToList();
        }

        public void DeleteCheckedHealthRecords(bool withCancel = true)
        {
            GetSelectedHrs().ForAll(hr =>
            {
                if (withCancel)
                    hr.IsDeleted = true;
                else
                    holder.RemoveHealthRecord(hr);
            });
        }

        public void Reorder(IEnumerable<ShortHealthRecordViewModel> moved, IList<ShortHealthRecordViewModel> view, int insertView, object group,
            Func<ShortHealthRecordViewModel, object> GetGroupObject, Action<ShortHealthRecordViewModel, object> SetGroupObject)
        {
            Contract.Requires(insertView >= 0);
            Contract.Requires(moved != null);
            Contract.Requires(view != null);
            Contract.Assume(inner.IsOrdered(x => x.Ord));

            ShortHealthRecordViewModel hrView;

            // move hrs
            foreach (var hr in moved)
            {
                var old = inner.IndexOf(hr);
                var oldView = view.IndexOf(hr);

                // insertView [0..view.Count]

                // insert above group border or at the end
                bool aboveBorderOrAtEnd = insertView == view.Count;
                if (0 < insertView && insertView < view.Count && group != null)
                {
                    // gong can show adoner in both groups, above and below border

                    hrView = (ShortHealthRecordViewModel)view[insertView];
                    var hrPrevView = (ShortHealthRecordViewModel)view[insertView - 1];

                    // var groups = view.Groups;
                    // разные группы и у верхнего элемента — целевая
                    aboveBorderOrAtEnd = GetGroupObject(hrPrevView) != GetGroupObject(hrView)
                        && group == GetGroupObject(hrPrevView);
                }

                // сравниваем с элементом ниже указателя или с тем, что над границей/над концом списка, если указатель прямо под ним
                var toCompareView = aboveBorderOrAtEnd ? insertView - 1 : insertView;
                var toCompareHr = (ShortHealthRecordViewModel)view[toCompareView];
                var toCompare = inner.IndexOf(toCompareHr);

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
                    group);
                if (old != dest)
                {
                    inner.Move(old, dest);
                }
            }

            // set group
            if (group != null)
                foreach (var hr in moved)
                {
                    SetGroupObject(hr, group);
                }

            // set order
            SetOrder();

            // sort visible collections
            healthRecords.Sort(x => x.Ord);
            deletedHealthRecords.Sort(x => x.Ord);
        }

        public void Reorder(IEnumerable<ShortHealthRecordViewModel> moved, IList<ShortHealthRecordViewModel> view, int insertView)
        {
            Reorder(moved, view, insertView, null, null, null);
        }

        private void SetOrder()
        {
            Contract.Ensures(inner.IsStrongOrdered(x => x.Ord));
            for (int i = 0; i < inner.Count; i++)
            {
                inner[i].healthRecord.Ord = i;
            }
        }

        private ShortHealthRecordViewModel CreateViewModel(HealthRecord hr)
        {
            hr.PropertyChanged += hr_PropertyChanged;
            var vm = new ShortHealthRecordViewModel(hr);
            vm.PropertyChanged += onHrVmPropChanged;
            return vm;
        }

        private void holder_HealthRecordsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (HealthRecord item in e.NewItems)
                {
                    Contract.Assume(!item.IsDeleted);
                    var hrVM = CreateViewModel(item);
                    item.Ord = inner.Count; // add to the end
                    inner.Add(hrVM);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (HealthRecord item in e.OldItems)
                {
                    var hrVM = inner.Where(vm => vm.healthRecord == item).FirstOrDefault();
                    inner.Remove(hrVM);
                }
            }
        }

        private void hr_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var hr = sender as HealthRecord;
            if (e.PropertyName == "IsDeleted")
            {
                if (hr.IsDeleted)
                {
                    var vm = HealthRecords.Where(x => x.healthRecord == hr).First();
                    deletedHealthRecords.Add(vm);
                    healthRecords.Remove(vm);
                    var undoDoActions = new Action[] {
                        () => hr.IsDeleted = false,
                        () => {
                            holder.RemoveHealthRecord(hr);
                            deletedHealthRecords.Remove(vm);
                        }
                    };
                    this.Send(Event.ShowUndoOverlay, new object[] { undoDoActions, typeof(HealthRecord) }.AsParams(MessageKeys.UndoDoActions, MessageKeys.Type));
                }
                else
                {
                    var vm = DeletedHealthRecords.Where(x => x.healthRecord == hr).FirstOrDefault();
                    if (vm != null)
                    {
                        deletedHealthRecords.Remove(vm);

                        // после первого видимого выше в списке
                        var notDelAt = inner.IndexOf(vm) - 1;
                        while (notDelAt >= 0 && inner[notDelAt].IsDeleted)
                            notDelAt--;
                        var dest = notDelAt < 0 ? 0 : healthRecords.IndexOf(inner[notDelAt]) + 1;
                        healthRecords.Insert(dest, vm);
                    }
                }
            }
        }

        /// <summary>
        /// Реальное удаление удаленных записей.
        /// </summary>
        internal void MakeDeletions()
        {
            this.Send(Event.HideOverlay, typeof(HealthRecord).AsParams(MessageKeys.Type));
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    holder.HealthRecordsChanged -= holder_HealthRecordsChanged;
                    foreach (var shortHrVm in HealthRecords)
                    {
                        shortHrVm.PropertyChanged -= onHrVmPropChanged;
                        shortHrVm.healthRecord.PropertyChanged -= hr_PropertyChanged;
                        shortHrVm.Dispose();
                    }
                    foreach (var shortHrVm in DeletedHealthRecords)
                    {
                        shortHrVm.PropertyChanged -= onHrVmPropChanged;
                        shortHrVm.healthRecord.PropertyChanged -= hr_PropertyChanged;
                        shortHrVm.Dispose();
                    }

                    MakeDeletions();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}