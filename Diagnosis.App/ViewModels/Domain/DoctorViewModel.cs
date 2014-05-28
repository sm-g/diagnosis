using Diagnosis.Models;
using EventAggregator;
using System;
using System.Diagnostics.Contracts;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class DoctorViewModel : ViewModelBase
    {
        internal readonly Doctor doctor;

        private ICommand _startCourse;

        public IEditable Editable { get; private set; }

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

        public string Speciality
        {
            get
            {
                return doctor.Speciality.Title;
            }
        }

        private string FullName
        {
            get
            {
                return LastName + " " + FirstName + " " + MiddleName;
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
            Editable.MarkDirty();

            this.Send((int)EventID.CourseStarted, new CourseStartedParams(course).Params);
        }

        public DoctorViewModel(Doctor d)
        {
            Contract.Requires(d != null);
            doctor = d;

            Editable = new EditableBase(this);
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}