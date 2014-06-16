using Diagnosis.Core;
using Diagnosis.Data;
using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class HealthRecordManager
    {
        private readonly AppointmentViewModel appVM;
        private ObservableCollection<HealthRecordViewModel> _healthRecords;

        public event EventHandler HealthRecordsLoaded;
        public event PropertyChangedEventHandler HrPropertyChanged;

        public ObservableCollection<HealthRecordViewModel> HealthRecords
        {
            get
            {
                if (_healthRecords == null)
                {
                    _healthRecords = MakeHealthRecords();
                    OnHealthRecordsLoaded();
                    AfterHealthRecordsLoaded();
                }
                return _healthRecords;
            }
        }

        public HealthRecordManager(AppointmentViewModel appVM)
        {
            this.appVM = appVM;
        }

        public void AddHealthRecord()
        {
            var last = appVM.SelectedHealthRecord ?? HealthRecords.LastOrDefault();
            var hr = appVM.appointment.AddHealthRecord();
            if (last != null)
            {
                // копируем категории из последней записи
                hr.Category = last.healthRecord.Category;
            }
        }

        internal HealthRecordViewModel MakeHealthRecordVM(HealthRecord hr)
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
            var hrVMs = appVM.appointment.HealthRecords.Select(hr => new HealthRecordViewModel(hr)).ToList();
            hrVMs.ForAll(hr => SubscribeHr(hr));

            var healthRecords = new ObservableCollection<HealthRecordViewModel>(hrVMs);
            Console.WriteLine("make hrs for {0}", this);
            return healthRecords;
        }
        private void AfterHealthRecordsLoaded()
        {
            appVM.SubscribeEditableNesting(HealthRecords);
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
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(hr);
                transaction.Commit();
            }
        }

        private void hr_Reverted(object sender, EditableEventArgs e)
        {
            var hr = e.entity as HealthRecord;
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Refresh(hr);
            }
        }

        private void hr_Deleted(object sender, EditableEventArgs e)
        {
            var hr = e.entity as HealthRecord;
            appVM.appointment.DeleteHealthRecord(hr);

            var hrVM = HealthRecords.Where(vm => vm.healthRecord == hr).FirstOrDefault();
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