using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Diagnosis.Core;

namespace Diagnosis.App.ViewModels
{
    public class HealthRecordManager
    {
        private readonly AppointmentViewModel appVM;
        private ObservableCollection<HealthRecordViewModel> _healthRecords;
        private HealthRecordRepository repo = new HealthRecordRepository();

        public event EventHandler HealthRecordsLoaded;

        public event PropertyChangedEventHandler HrPropertyChanged;

        public ObservableCollection<HealthRecordViewModel> HealthRecords
        {
            get
            {
                if (_healthRecords == null)
                {
                    _healthRecords = MakeHealthRecords();

                    appVM.SubscribeEditableNesting(HealthRecords);
                    OnHealthRecordsLoaded();
                }
                return _healthRecords;
            }
        }

        public HealthRecordManager(AppointmentViewModel appVM)
        {
            this.appVM = appVM;
        }

        public HealthRecordViewModel AddHealthRecord()
        {
            var last = appVM.SelectedHealthRecord ?? HealthRecords.LastOrDefault();
            var hr = appVM.appointment.AddHealthRecord();
            if (last != null)
            {
                // копируем категории из последней записи
                hr.Category = last.healthRecord.Category;
            }

            var hrVM = MakeHealthRecordVM(hr);
            HealthRecords.Add(hrVM);
            return hrVM;
        }

        private HealthRecordViewModel MakeHealthRecordVM(HealthRecord hr)
        {
            var hrVM = new HealthRecordViewModel(hr);

            SubscribeHr(hrVM);
            return hrVM;
        }

        protected virtual void OnHealthRecordsLoaded()
        {
            var h = HealthRecordsLoaded;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }

        protected virtual void OnHrPropertyChanged(PropertyChangedEventArgs e)
        {
            var h = HrPropertyChanged;
            if (h != null)
            {
                h(this, e);
            }
        }

        private ObservableCollection<HealthRecordViewModel> MakeHealthRecords()
        {
            IList<HealthRecordViewModel> hrVMs;
            using (var tester = new PerformanceTester((ts) => Debug.Print("making healthrecords for {0}: {1}", appVM, ts)))
            {
                hrVMs = appVM.appointment.HealthRecords.Select(hr => MakeHealthRecordVM(hr)).ToList();
            }
            var healthRecords = new ObservableCollection<HealthRecordViewModel>(hrVMs);
            return healthRecords;
        }

        private void SubscribeHr(HealthRecordViewModel hrVM)
        {
            hrVM.PropertyChanged += hr_PropertyChanged;
            hrVM.Editable.Deleted += hr_Deleted;
            hrVM.Editable.Reverted += hr_Reverted;
            hrVM.Editable.Committed += hr_Committed;
            hrVM.Editable.DirtyChanged += hr_DirtyChanged;
        }

        private void UnsubscribeHr(HealthRecordViewModel hrVM)
        {
            hrVM.PropertyChanged -= hr_PropertyChanged;
            hrVM.Editable.Deleted -= hr_Deleted;
            hrVM.Editable.Reverted -= hr_Reverted;
            hrVM.Editable.Committed -= hr_Committed;
            hrVM.Editable.DirtyChanged -= hr_DirtyChanged;
        }

        private void hr_Committed(object sender, EditableEventArgs e)
        {
            var hr = e.entity as HealthRecord;
            repo.SaveOrUpdate(hr);
        }

        private void hr_Reverted(object sender, EditableEventArgs e)
        {
            var hr = e.entity as HealthRecord;
            repo.Refresh(hr);
        }

        private void hr_Deleted(object sender, EditableEventArgs e)
        {
            var hr = e.entity as HealthRecord;
            appVM.appointment.DeleteHealthRecord(hr);

            var hrVM = HealthRecords.Where(vm => vm.healthRecord == hr).FirstOrDefault();
            HealthRecords.Remove(hrVM);
            UnsubscribeHr(hrVM);
        }

        private void hr_DirtyChanged(object sender, EditableEventArgs e)
        {
            appVM.Editable.IsDirty = HealthRecords.Any(vm => vm.Editable.IsDirty);
        }

        private void hr_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnHrPropertyChanged(e);
        }
    }
}