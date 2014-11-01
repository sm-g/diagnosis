using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Windows.Input;
using Diagnosis.Core;

namespace Diagnosis.ViewModels.Search
{
    // TODO записи везде
    public class HrSearchResultViewModel : ViewModelBase
    {
        readonly HealthRecord hr;
        readonly HrSearchOptions options;

        public HrSearchResultViewModel(HealthRecord hr, HrSearchOptions options)
        {
            Contract.Requires(hr != null);
            Contract.Requires(options != null);

            this.hr = hr;
            this.options = options;

            AppointmentHealthRecords = new ObservableCollection<HealthRecord>(hr.Appointment.HealthRecords);
            hr.Appointment.HealthRecordsChanged += Appointment_HealthRecordsChanged;
        }

        void Appointment_HealthRecordsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                AppointmentHealthRecords.Add(e.NewItems as HealthRecord);
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                AppointmentHealthRecords.Remove(e.NewItems as HealthRecord);

            }
        }


        public Patient Patient
        {
            get
            {
                return hr.Appointment.Course.Patient;
            }
        }

        public Course Course
        {
            get
            {
                return hr.Appointment.Course;
            }
        }
        public DateTime AppointmentDate
        {
            get
            {
                return hr.Appointment.DateAndTime;
            }
        }

        public ObservableCollection<HealthRecord> AppointmentHealthRecords
        {
            get;
            private set;
        }

        public HealthRecord FoundHealthRecord
        {
            get
            {
                return hr;
            }
        }

        public ICommand OpenHealthRecordCommand
        {
            get
            {
                return new RelayCommand(
                    () =>
                    {
                        this.Send(Events.OpenHealthRecord, FoundHealthRecord.AsParams(MessageKeys.HealthRecord));
                    });
            }
        }

        protected override void Dispose(bool disposing)
        {
            hr.Appointment.HealthRecordsChanged -= Appointment_HealthRecordsChanged;
            base.Dispose(disposing);
        }
    }
}