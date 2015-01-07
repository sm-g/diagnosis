using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class HealthRecordManager : DisposableBase
    {
        private readonly IHrsHolder holder;
        private readonly PropertyChangedEventHandler onHrVmPropChanged;

        public event EventHandler<DomainEntityEventArgs> Undeleted;

        public HealthRecordManager(IHrsHolder holder, PropertyChangedEventHandler onHrVmPropChanged)
        {
            this.holder = holder;
            this.onHrVmPropChanged = onHrVmPropChanged;
            holder.HealthRecordsChanged += holder_HealthRecordsChanged;

            var hrVMs = holder.HealthRecords.Select(hr => CreateViewModel(hr));
            HealthRecords = new ObservableCollection<ShortHealthRecordViewModel>(hrVMs);
            DeletedHealthRecords = new ObservableCollection<ShortHealthRecordViewModel>();
        }

        public ObservableCollection<ShortHealthRecordViewModel> HealthRecords { get; private set; }

        private ObservableCollection<ShortHealthRecordViewModel> DeletedHealthRecords { get; set; }

        private void holder_HealthRecordsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (HealthRecord item in e.NewItems)
                {
                    var hrVM = CreateViewModel(item);
                    HealthRecords.Add(hrVM);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (HealthRecord item in e.OldItems)
                {
                    var hrVM = HealthRecords.Where(vm => vm.healthRecord == item).FirstOrDefault();
                    HealthRecords.Remove(hrVM);
                }
            }
        }

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

        private ShortHealthRecordViewModel CreateViewModel(HealthRecord hr)
        {
            hr.PropertyChanged += hr_PropertyChanged;
            var vm = new ShortHealthRecordViewModel(hr);
            vm.PropertyChanged += onHrVmPropChanged;
            return vm;
        }

        private void hr_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var hr = sender as HealthRecord;
            if (e.PropertyName == "IsDeleted")
            {
                if (hr.IsDeleted)
                {
                    var vm = HealthRecords.Where(x => x.healthRecord == hr).First();
                    DeletedHealthRecords.Add(vm);
                    HealthRecords.Remove(vm);
                    var undoActions = new Action[] {
                        () => hr.IsDeleted = false,
                        () => {
                            holder.RemoveHealthRecord(hr);
                            DeletedHealthRecords.Remove(vm);
                        }
                    };
                    this.Send(Event.ShowUndoOverlay, new object[] { undoActions, typeof(HealthRecord) }.AsParams(MessageKeys.UndoOverlay, MessageKeys.Type));
                }
                else
                {
                    var vm = DeletedHealthRecords.Where(x => x.healthRecord == hr).FirstOrDefault();
                    if (vm != null)
                    {
                        DeletedHealthRecords.Remove(vm);
                        HealthRecords.Add(vm);
                        OnUndeleted(vm.healthRecord);
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

        protected virtual void OnUndeleted(HealthRecord hr)
        {
            var h = Undeleted;
            if (h != null)
            {
                h(this, new DomainEntityEventArgs(hr));
            }
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