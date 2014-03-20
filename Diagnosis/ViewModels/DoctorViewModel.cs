using Diagnosis.Models;
using System;
using System.Diagnostics.Contracts;
using System.Windows.Input;
using EventAggregator;

namespace Diagnosis.ViewModels
{
    public class DoctorViewModel : CheckableBase, ISearchable
    {
        internal Doctor doctor;

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


        private string _speciality;
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

        public override bool IsReady
        {
            get
            {
                return base.IsReady && !IsSearchActive;
            }
        }

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

        #region ISearchable

        private ICommand _searchCommand;
        private bool _searchActive;
        private bool _searchFocused;

        public string Representation
        {
            get
            {
                return ShortName;
            }
        }

        public bool IsSearchActive
        {
            get
            {
                return _searchActive;
            }
            set
            {
                if (_searchActive != value && (IsReady || !value))
                {
                    _searchActive = value;
                    OnPropertyChanged(() => IsSearchActive);
                }
            }
        }

        public bool IsSearchFocused
        {
            get
            {
                return _searchFocused;
            }
            set
            {
                if (_searchFocused != value)
                {
                    _searchFocused = value;
                    OnPropertyChanged(() => IsSearchFocused);
                }
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                return _searchCommand
                    ?? (_searchCommand = new RelayCommand(
                                          () =>
                                          {
                                              IsSearchActive = !IsSearchActive;
                                          }
                                          ));
            }
        }

        #endregion ISearchable

        private ICommand _startCourse;

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