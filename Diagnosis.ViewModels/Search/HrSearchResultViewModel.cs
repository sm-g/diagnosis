using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Windows.Input;
using Diagnosis.Core;

namespace Diagnosis.ViewModels
{
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
                return new RelayCommand(
                    () =>
                    {
                        this.Send(Events.OpenHealthRecord, FoundHealthRecord.AsParams(MessageKeys.HealthRecord));
                    });
            }
        }
    }
}