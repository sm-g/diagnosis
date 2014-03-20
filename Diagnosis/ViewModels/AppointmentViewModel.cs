using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class AppointmentViewModel : CheckableBase
    {
        private Appointment appointment;
        private DoctorViewModel _doctor;
        private ICommand _addHealthRecord;

        #region CheckableBase

        public override string Name
        {
            get
            {
                return "Осмотр";
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        protected override void OnCheckedChanged()
        {
            throw new NotImplementedException();
        }

        #endregion CheckableBase

        public DoctorViewModel Doctor
        {
            get
            {
                return _doctor;
            }
            set
            {
                if (_doctor != value)
                {
                    _doctor = value;
                    OnPropertyChanged(() => Doctor);
                }
            }
        }

        public DateTime DateTime
        {
            get
            {
                return appointment.DateTime;
            }
        }

        public ObservableCollection<HealthRecord> HealthRecords { get; private set; }

        public ICommand AddHealthRecordCommand
        {
            get
            {
                return _addHealthRecord
                    ?? (_addHealthRecord = new RelayCommand(() =>
                        {
                            var hr = appointment.AddHealthRecord();
                            HealthRecords.Add(hr);
                        }));
            }
        }

        public AppointmentViewModel(Appointment appointment)
        {
            Contract.Requires(appointment != null);

            this.appointment = appointment;
            Doctor = new DoctorViewModel(appointment.Doctor);
            HealthRecords = new ObservableCollection<HealthRecord>();
        }
    }
}