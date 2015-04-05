using Diagnosis.Common;
using Diagnosis.Common.Types;
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

        public INCCReadOnlyObservableCollection<ShortHealthRecordViewModel> DeletedHealthRecords { get; private set; }

        public List<HealthRecord> GetSelectedHrs()
        {
            return HealthRecords.Where(hr => hr.IsChecked)
                                .Select(vm => vm.healthRecord).ToList();
        }

        public void UnselectAll()
        {
            inner.ForAll(x => x.IsSelected = false);
        }

        public void UnselectExcept(ShortHealthRecordViewModel vm)
        {
            UnselectExcept(vm.ToEnumerable());
        }

        public void UnselectExcept(IEnumerable<ShortHealthRecordViewModel> vms)
        {
            inner.Except(vms).ForAll(x => x.IsSelected = false);
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

        public void Reorder(IEnumerable<ShortHealthRecordViewModel> moved, IList<ShortHealthRecordViewModel> view, int insertView, object targetGroup,
               bool aboveBorder, Action<ShortHealthRecordViewModel, object> SetGroupObject)
        {
            Contract.Requires(insertView >= 0);
            Contract.Requires(moved != null);
            Contract.Requires(view != null);
            Contract.Assume(inner.IsOrdered(x => x.Ord));

            // move hrs
            foreach (var hr in moved)
            {
                var old = inner.IndexOf(hr);
                var oldView = view.IndexOf(hr);

                // insert above group border or at the end
                var aboveBorderOrAtEnd = aboveBorder || insertView == view.Count;

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
                    targetGroup);
                if (old != dest)
                {
                    inner.Move(old, dest);
                }
            }

            // Dragging to other group makes in two passes: first change group, second reorder items in that group.

            // set group
            if (targetGroup != null)
                foreach (var hr in moved)
                {
                    SetGroupObject(hr, targetGroup);
                }

            // set order
            SetOrder();

            // sort visible collections
            healthRecords.Sort(x => x.Ord);
            deletedHealthRecords.Sort(x => x.Ord);
        }

        public void Reorder(IEnumerable<ShortHealthRecordViewModel> moved, IList<ShortHealthRecordViewModel> view, int insertView)
        {
            Reorder(moved, view, insertView, null, false, null);
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
                    var last = inner.LastOrDefault();
                    item.Ord = last != null ? last.Ord + 1 : 0; // add to the end
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

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    // just close without execute OnDo
                    this.Send(Event.HideOverlay, new object[] { typeof(HealthRecord), true }.AsParams(MessageKeys.Type, MessageKeys.Boolean));

                    holder.HealthRecordsChanged -= holder_HealthRecordsChanged;
                    foreach (var shortHrVm in inner)
                    {
                        shortHrVm.PropertyChanged -= onHrVmPropChanged;
                        shortHrVm.healthRecord.PropertyChanged -= hr_PropertyChanged;
                        shortHrVm.Dispose();
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}