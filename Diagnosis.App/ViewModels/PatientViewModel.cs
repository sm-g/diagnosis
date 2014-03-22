using Diagnosis.App;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class PatientViewModel : CheckableBase, ISearchable
    {
        internal Patient patient;

        private PropertyManager _propManager;
        private List<EventMessageHandler> msgHandlers = new List<EventMessageHandler>();

        public string FirstName
        {
            get
            {
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                return patient.FirstName;
            }
            set
            {
                if (patient.FirstName != value)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        patient.FirstName = value;
                    }
                    OnPropertyChanged(() => FirstName);
                    OnPropertyChanged(() => ShortName);
                    MarkDirty();
                }
            }
        }

        public string MiddleName
        {
            get
            {
                return patient.MiddleName ?? "";
            }
            set
            {
                if (patient.MiddleName != value)
                {
                    patient.MiddleName = value;
                    OnPropertyChanged(() => MiddleName);
                    OnPropertyChanged(() => ShortName);
                    MarkDirty();
                }
            }
        }

        public string LastName
        {
            get
            {
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                return patient.LastName;
            }
            set
            {
                if (patient.LastName != value)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        patient.LastName = value;
                    }
                    OnPropertyChanged(() => LastName);
                    OnPropertyChanged(() => ShortName);
                    MarkDirty();
                }
            }
        }

        public int Age
        {
            get
            {
                int age = DateTime.Today.Year - patient.BirthDate.Year;
                if (patient.BirthDate > DateTime.Today.AddYears(-age))
                    age--;
                return age;
            }
            set
            {
                int year = DateTime.Today.Year - value;
                if (new DateTime(value, patient.BirthDate.Month, patient.BirthDate.Day) < DateTime.Today.AddYears(-value)) // TODO
                    year--;
                BirthYear = year;
            }
        }

        public int BirthYear
        {
            get
            {
                return patient.BirthDate.Year;
            }
            set
            {
                if (patient.BirthDate.Year != value && value >= 0 && value <= DateTime.Today.Year)
                {
                    patient.BirthDate = new DateTime(value, patient.BirthDate.Month, patient.BirthDate.Day);
                    OnPropertyChanged(() => Age);
                    OnPropertyChanged(() => BirthYear);
                    MarkDirty();
                }
            }
        }

        public int BirthMonth
        {
            get
            {
                return patient.BirthDate.Month;
            }
            set
            {
                if (patient.BirthDate.Month != value && value >= 1 && value <= 12)
                {
                    patient.BirthDate = new DateTime(patient.BirthDate.Year, value, patient.BirthDate.Day);
                    OnPropertyChanged(() => Age);
                    OnPropertyChanged(() => BirthMonth);
                    MarkDirty();
                }
            }
        }

        public int BirthDay
        {
            get
            {
                return patient.BirthDate.Day;
            }
            set
            {
                if (patient.BirthDate.Day != value && value >= 1 && value <= 31)
                {
                    patient.BirthDate = new DateTime(patient.BirthDate.Year, patient.BirthDate.Month, value);

                    OnPropertyChanged(() => Age);
                    OnPropertyChanged(() => BirthDay);
                    MarkDirty();
                }
            }
        }

        public bool IsMale
        {
            get
            {
                return patient.IsMale;
            }
            set
            {
                if (patient.IsMale != value)
                {
                    patient.IsMale = value;
                    OnPropertyChanged(() => IsMale);
                    MarkDirty();
                }
            }
        }

        public string Snils
        {
            get
            {
                return patient.SNILS;
            }
            set
            {
                if (patient.SNILS != value)
                {
                    patient.SNILS = value;
                    OnPropertyChanged(() => Snils);
                    MarkDirty();
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

        public ObservableCollection<CourseViewModel> Courses
        {
            get;
            private set;
        }


        private CourseViewModel _selectedCourse;
        public CourseViewModel SelectedCourse
        {
            get
            {
                return _selectedCourse;
            }
            set
            {
                if (_selectedCourse != value)
                {
                    _selectedCourse = value;
                    _selectedCourse.IsSelected = true;
                    OnPropertyChanged(() => SelectedCourse);
                }
            }
        }

        public ObservableCollection<PropertyViewModel> Properties
        {
            get;
            private set;
        }

        public PropertyManager PropertyManager
        {
            get
            {
                return _propManager;
            }
            set
            {
                if (_propManager != value)
                {
                    _propManager = value;
                    OnPropertyChanged(() => PropertyManager);
                }
            }
        }


        private DoctorViewModel _doctor;
        public DoctorViewModel CurrentDoctor
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
                    OnPropertyChanged(() => CurrentDoctor);
                }
            }
        }

        public void SetDoctorVM(DoctorViewModel doctor)
        {
            Contract.Requires(doctor != null);

            CurrentDoctor = doctor;
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

        public PatientViewModel(Patient p, PropertyManager propManager)
        {
            Contract.Requires(p != null);
            Contract.Requires(propManager != null);

            patient = p;
            PropertyManager = propManager;

            Properties = new ObservableCollection<PropertyViewModel>(PropertyManager.GetPatientProperties(patient));
            Courses = new ObservableCollection<CourseViewModel>(patient.Courses.Select(i => new CourseViewModel(i)));

            if (Courses.Count > 0)
            {
                SelectedCourse = Courses.Last();
            }
        }

        public void Subscribe()
        {
            msgHandlers = new List<EventMessageHandler>()
            {
                this.Subscribe((int)EventID.SymptomCheckedChanged, (e) =>
                {
                    var symptom = e.GetValue<SymptomViewModel>(Messages.Symptom);
                    var isChecked = e.GetValue<bool>(Messages.CheckedState);

                    OnSymptomCheckedChanged(symptom, isChecked);
                }),
                this.Subscribe((int)EventID.PropertySelectedValueChanged, (e) =>
                {
                    var property = e.GetValue<PropertyViewModel>(Messages.Property);

                    OnPropertyValueChanged(property);
                }),
                 this.Subscribe((int)EventID.CourseStarted, (e) =>
                {
                    var course = e.GetValue<Course>(Messages.Course);

                    OnCourseStarted(course);
                }),
            };
        }

        public void Unsubscribe()
        {
            foreach (var h in msgHandlers)
            {
                h.Dispose();
            }
        }

        private void OnPropertyValueChanged(PropertyViewModel propertyVM)
        {
            MarkDirty();
            patient.SetPropertyValue(propertyVM.Property, propertyVM.SelectedValue);
        }

        private void OnCourseStarted(Course course)
        {
            Courses.Add(new CourseViewModel(course));
            MarkDirty();
        }
    }
}