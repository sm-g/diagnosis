using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Diagnosis.App.ViewModels
{
    public class PatientViewModel : CheckableBase
    {
        internal Patient patient;

        private DoctorViewModel _doctor;
        private CoursesManager _coursesManager;
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

        public CoursesManager CoursesManager
        {
            get
            {
                return _coursesManager;
            }
            set
            {
                if (_coursesManager != value)
                {
                    _coursesManager = value;
                    OnPropertyChanged(() => CoursesManager);
                }
            }
        }

        public ObservableCollection<PropertyViewModel> Properties
        {
            get;
            private set;
        }

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

        public void Subscribe()
        {
            msgHandlers = new List<EventMessageHandler>()
            {
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
                 this.Subscribe((int)EventID.AppointmentAdded, (e) =>
                {
                    var app = e.GetValue<Appointment>(Messages.Appointment);

                    OnAppointmentAdded(app);
                }),
                this.Subscribe((int)EventID.HealthRecordSelected, (e) =>
                {
                    var hr = e.GetValue<HealthRecordViewModel>(Messages.HealthRecord);

                    OnHealthRecordSelected(hr);
                }),
                this.Subscribe((int)EventID.SymptomCheckedChanged, (e) =>
                {
                    var symptom = e.GetValue<SymptomViewModel>(Messages.Symptom);
                    var isChecked = e.GetValue<bool>(Messages.CheckedState);

                    OnSymptomCheckedChanged(symptom, isChecked);
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

        public PatientViewModel(Patient p)
        {
            Contract.Requires(p != null);

            patient = p;

            Properties = new ObservableCollection<PropertyViewModel>(EntityManagers.PropertyManager.GetPatientProperties(patient));
            CoursesManager = new CoursesManager(this);
        }

        private void OnPropertyValueChanged(PropertyViewModel propertyVM)
        {
            MarkDirty();
            patient.SetPropertyValue(propertyVM.Property, propertyVM.SelectedValue);
        }

        private void OnCourseStarted(Course course)
        {
            CoursesManager.AddCourse(course);
            MarkDirty();
        }

        private void OnAppointmentAdded(Appointment app)
        {
            MarkDirty();
        }

        private void OnHealthRecordSelected(HealthRecordViewModel hr)
        {
            hr.MakeCurrent();
        }

        private void OnSymptomCheckedChanged(SymptomViewModel symptom, bool isChecked)
        {
            MarkDirty();
        }
    }
}