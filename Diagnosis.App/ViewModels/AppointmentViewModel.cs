using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Windows.Input;
using System.Linq;

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

        public ObservableCollection<HealthRecordViewModel> HealthRecords { get; private set; }


        private HealthRecordViewModel _selectedHealthRecord;
        public HealthRecordViewModel SelectedHealthRecord
        {
            get
            {
                return _selectedHealthRecord;
            }
            set
            {
                if (_selectedHealthRecord != value)
                {
                    _selectedHealthRecord = value;
                    _selectedHealthRecord.IsSelected = true;
                    OnPropertyChanged(() => SelectedHealthRecord);
                }
            }
        }

        public ICommand AddHealthRecordCommand
        {
            get
            {
                return _addHealthRecord
                    ?? (_addHealthRecord = new RelayCommand(() =>
                        {
                            var hr = appointment.AddHealthRecord();
                            HealthRecords.Add(new HealthRecordViewModel(hr));
                        }));
            }
        }

        public AppointmentViewModel(Appointment appointment)
        {
            Contract.Requires(appointment != null);

            this.appointment = appointment;
            Doctor = new DoctorViewModel(appointment.Doctor);
            HealthRecords = new ObservableCollection<HealthRecordViewModel>(appointment.HealthRecords.Select(hr => new HealthRecordViewModel(hr)));
        }
    }
}