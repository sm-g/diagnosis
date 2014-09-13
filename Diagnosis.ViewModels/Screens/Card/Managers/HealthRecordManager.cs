﻿using Diagnosis.Core;
using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class HealthRecordManager : DisposableBase
    {
        private readonly Appointment app;
        private readonly PropertyChangedEventHandler onHrVmPropChanged;

        public HealthRecordManager(Appointment app, PropertyChangedEventHandler onHrVmPropChanged)
        {
            this.app = app;
            this.onHrVmPropChanged = onHrVmPropChanged;
            app.HealthRecordsChanged += app_HealthRecordsChanged;

            var hrVMs = app.HealthRecords.Select(hr => CreateViewModel(hr));
            HealthRecords = new ObservableCollection<ShortHealthRecordViewModel>(hrVMs);
            DeletedHealthRecords = new ObservableCollection<ShortHealthRecordViewModel>();
        }

        public ObservableCollection<ShortHealthRecordViewModel> HealthRecords { get; private set; }

        public ObservableCollection<ShortHealthRecordViewModel> DeletedHealthRecords { get; private set; }

        private void app_HealthRecordsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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

        public void DeleteCheckedHealthRecords()
        {
            HealthRecords.Where(hr => hr.IsChecked).ToList().ForAll(hr =>
            {
                hr.healthRecord.IsDeleted = true;
            });
        }

        private ShortHealthRecordViewModel CreateViewModel(HealthRecord hr)
        {
            //   hr.PropertyChanged += hr_PropertyChanged;
            var vm = new ShortHealthRecordViewModel(hr);
            vm.PropertyChanged += onHrVmPropChanged;
            return vm;
        }

        private void hr_Deleted(object sender, EditableEventArgs e)
        {
            var hr = e.entity as HealthRecord;
            app.RemoveHealthRecord(hr);

            var hrVM = HealthRecords.Where(vm => vm.healthRecord == hr).FirstOrDefault();
            HealthRecords.Remove(hrVM);
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
                }
                else
                {
                    var vm = DeletedHealthRecords.Where(x => x.healthRecord == hr).First();
                    DeletedHealthRecords.Remove(vm);
                    HealthRecords.Add(vm);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    app.HealthRecordsChanged -= app_HealthRecordsChanged;
                    foreach (var shortHrVm in HealthRecords)
                    {
                        shortHrVm.PropertyChanged -= onHrVmPropChanged;
                        shortHrVm.Dispose();
                    }
                    foreach (var shortHrVm in DeletedHealthRecords)
                    {
                        shortHrVm.PropertyChanged -= onHrVmPropChanged;
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