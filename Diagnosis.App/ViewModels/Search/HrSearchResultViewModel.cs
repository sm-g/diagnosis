using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using Diagnosis.App.Messaging;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class HrSearchResultViewModel : ViewModelBase
    {
        readonly HealthRecord hr;
        readonly HrSearchOptions options;

        private RelayCommand _openHr;

        public HrSearchResultViewModel(HealthRecord hr, HrSearchOptions options)
        {
            Contract.Requires(hr != null);
            Contract.Requires(options != null);

            this.hr = hr;
            this.options = options;
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

        public ReadOnlyCollection<HealthRecord> AppointmentHealthRecords
        {
            get
            {
                return hr.Appointment.HealthRecords;
            }
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
                return _openHr ?? (_openHr = new RelayCommand(
                    () =>
                    {
                        this.Send((int)EventID.OpenHealthRecord, new HealthRecordModelParams(FoundHealthRecord).Params);
                    }));
            }
        }
    }
}