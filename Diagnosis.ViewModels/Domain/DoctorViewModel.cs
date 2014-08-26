using Diagnosis.Models;
using Diagnosis.Core;
using EventAggregator;
using System;
using System.Diagnostics.Contracts;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class DoctorViewModel : ViewModelBase
    {
        internal readonly Doctor doctor;
        SettingsViewModel _settings;

        private ICommand _startCourse;

        public Editable Editable { get; private set; }

        public string FirstName
        {
            get
            {
                return doctor.FirstName;
            }
            set
            {
                if (doctor.FirstName != value)
                {
                    doctor.FirstName = value;
                    OnPropertyChanged(() => FirstName);
                }
            }
        }

        public string MiddleName
        {
            get
            {
                return doctor.MiddleName ?? "";
            }
            set
            {
                if (doctor.MiddleName != value)
                {
                    doctor.MiddleName = value;
                    OnPropertyChanged(() => MiddleName);
                }
            }
        }

        public string LastName
        {
            get
            {
                return doctor.LastName;
            }
            set
            {
                if (doctor.LastName != value)
                {
                    doctor.LastName = value;
                    OnPropertyChanged(() => LastName);
                }
            }
        }

        public bool IsMale
        {
            get
            {
                return doctor.IsMale;
            }
            set
            {
                if (doctor.IsMale != value)
                {
                    doctor.IsMale = value;
                    OnPropertyChanged(() => IsMale);
                }
            }
        }

        public SettingsViewModel Settings
        {
            get
            {
                return _settings ?? (_settings = new SettingsViewModel(this));
            }
        }

        public string Speciality
        {
            get
            {
                return doctor.Speciality.Title;
            }
        }

        public ICommand StartCourseCommand
        {
            get
            {
                return _startCourse
                    ?? (_startCourse = new RelayCommand<PatientViewModel>(
                                          (patientVM) =>
                                          {
                                              StartCourse(patientVM);
                                          }));
            }
        }

        public void StartCourse(PatientViewModel patientVM)
        {
            var course = doctor.StartCourse(patientVM.patient);
        }

        public DoctorViewModel(Doctor d)
        {
            Contract.Requires(d != null);
            doctor = d;

            Editable = new Editable(doctor);
        }

        public override string ToString()
        {
            return doctor.ToString();
        }
    }
}