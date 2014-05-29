using Diagnosis.Core;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using System.Diagnostics.Contracts;
using Diagnosis.Models;

namespace Diagnosis.App.ViewModels
{
    public class SearchResultViewModel : ViewModelBase
    {
        readonly HealthRecord hr;
        readonly HrSearchOptions options;

        public SearchResultViewModel(HealthRecord hr, HrSearchOptions options)
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

        private RelayCommand _openHr;

        /// <summary>
        /// Gets the OpenHealthRecord.
        /// </summary>
        public ICommand OpenHealthRecord
        {
            get
            {
                return _openHr ?? (_openHr = new RelayCommand(
                    () =>
                    {
                        this.Send((int)EventID.OpenHealthRecord, new OpenHealthRecordParams(FoundHealthRecord).Params);
                    }));
            }
        }
    }
}