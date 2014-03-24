using Diagnosis.Models;
using EventAggregator;
using System;
using System.Diagnostics.Contracts;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class DoctorViewModel : CheckableBase
    {
        internal readonly Doctor doctor;

        private string _speciality;
        private ICommand _startCourse;

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
                    OnPropertyChanged(() => ShortName);
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
                    OnPropertyChanged(() => ShortName);
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
                    OnPropertyChanged(() => ShortName);
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
                return _speciality;
            }
            set
            {
                if (_speciality != value)
                {
                    _speciality = value;
                    OnPropertyChanged(() => Speciality);
                }
            }
        }

        public string ShortName
        {
            get
            {
                return LastName + (FirstName.Length > 0 ? " " + FirstName[0] + "." + (MiddleName.Length > 0 ? " " + MiddleName[0] + "." : "") : "");
            }
        }

        #region CheckableBase

        public override string Name
        {
            get
            {
                return ShortName;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        protected override void OnCheckedChanged()
        {
        }

        #endregion CheckableBase

        public ICommand StartCourseCommand
        {
            get
            {
                return _startCourse
                    ?? (_startCourse = new RelayCommand<PatientViewModel>(
                                          (patientVM) =>
                                          {
                                              var course = doctor.StartCourse(patientVM.patient);
                                              MarkDirty();

                                              this.Send((int)EventID.CourseStarted, new CourseStartedParams(course).Params);
                                          }));
            }
        }

        public DoctorViewModel(Doctor d)
        {
            Contract.Requires(d != null);
            doctor = d;
        }
    }
}