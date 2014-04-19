using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class AppointmentViewModel : CheckableBase
    {
        private Appointment appointment;

        CourseViewModel courseVM;

        private DoctorViewModel _doctor;
        private HealthRecordViewModel _selectedHealthRecord;
        private ICommand _addHealthRecord;

        #region CheckableBase

        public override void OnCheckedChanged()
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
                    OnPropertyChanged(() => IsDoctorFromCourse);
                }
            }
        }

        public DateTime DateTime
        {
            get
            {
                return appointment.DateAndTime;
            }
        }

        public ObservableCollection<HealthRecordViewModel> HealthRecords { get; private set; }

        public bool IsDoctorFromCourse
        {
            get
            {
                return Doctor == courseVM.LeadDoctor;
            }
        }

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

                    this.Send((int)EventID.HealthRecordSelected, new HealthRecordSelectedParams(_selectedHealthRecord).Params);
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
                            var hrVM = AddHealthRecord();

                            SelectedHealthRecord = hrVM;
                        }));
            }
        }

        private HealthRecordViewModel AddHealthRecord()
        {
            var hr = appointment.AddHealthRecord();
            var hrVM = new HealthRecordViewModel(hr);
            SubscribeHR(hrVM);
            HealthRecords.Add(hrVM);
            return hrVM;
        }

        public AppointmentViewModel(Appointment appointment, CourseViewModel courseVM)
        {
            Contract.Requires(appointment != null);
            Contract.Requires(courseVM != null);

            this.appointment = appointment;
            this.courseVM = courseVM;

            Doctor = EntityManagers.DoctorsManager.GetByModel(appointment.Doctor);

            var hrVMs = appointment.HealthRecords.Select(hr => new HealthRecordViewModel(hr)).ToList();
            hrVMs.ForAll(hr => SubscribeHR(hr));
            HealthRecords = new ObservableCollection<HealthRecordViewModel>(hrVMs);
        }

        private void SubscribeHR(HealthRecordViewModel hr)
        {
            hr.Editable.ModelPropertyChanged += (s, e) =>
            {
                this.Send((int)EventID.HealthRecordChanged,
                    new HealthRecordChangedParams(e.viewModel as HealthRecordViewModel).Params);
            };
        }

        public override string ToString()
        {
            return DateTime.ToShortDateString() + ' ' + Doctor;
        }
    }
}