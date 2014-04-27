using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class PatientViewModel : CheckableBase
    {
        internal readonly Patient patient;

        private DoctorViewModel _doctor;
        private CoursesManager _coursesManager;
        private List<EventMessageHandler> msgHandlers = new List<EventMessageHandler>();
        public IEditable Editable { get; private set; }

        #region Model related

        public int ID
        {
            get
            {
                return patient.Id;
            }
        }

        public string FirstName
        {
            get
            {
                return patient.FirstName ?? "";
            }
            set
            {
                if (patient.FirstName != value)
                {
                    patient.FirstName = value;

                    OnPropertyChanged(() => FirstName);
                    OnPropertyChanged(() => ShortName);
                    OnPropertyChanged(() => FullName);
                    OnPropertyChanged(() => SearchText);
                    OnPropertyChanged(() => NoName);
                    Editable.MarkDirty();
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
                    OnPropertyChanged(() => FullName);
                    OnPropertyChanged(() => SearchText);
                    OnPropertyChanged(() => NoName);
                    Editable.MarkDirty();
                }
            }
        }

        public string LastName
        {
            get
            {
                return patient.LastName ?? "";
            }
            set
            {
                if (patient.LastName != value)
                {
                    patient.LastName = value;

                    OnPropertyChanged(() => LastName);
                    OnPropertyChanged(() => ShortName);
                    OnPropertyChanged(() => FullName);
                    OnPropertyChanged(() => SearchText);
                    OnPropertyChanged(() => NoName);
                    Editable.MarkDirty();
                }
            }
        }

        public int? Age
        {
            get
            {
                return patient.Age;
            }
            set
            {
                if (patient.Age != value)
                {
                    patient.Age = value;
                    OnPropertyChanged(() => Age);
                    OnPropertyChanged(() => BirthYear);
                    Editable.MarkDirty();
                }
            }
        }

        public int? BirthYear
        {
            get
            {
                return patient.BirthYear;
            }
            set
            {
                if (patient.BirthYear != value)
                {
                    patient.BirthYear = value;
                    OnPropertyChanged(() => Age);
                    OnPropertyChanged(() => BirthYear);
                    Editable.MarkDirty();
                }
            }
        }

        public byte? BirthMonth
        {
            get
            {
                return patient.BirthMonth;
            }
            set
            {
                if (patient.BirthMonth != value)
                {
                    patient.BirthMonth = value;
                    OnPropertyChanged(() => Age);
                    OnPropertyChanged(() => BirthMonth);
                    Editable.MarkDirty();
                }
            }
        }

        public byte? BirthDay
        {
            get
            {
                return patient.BirthDay;
            }
            set
            {
                if (patient.BirthDay != value)
                {
                    patient.BirthDay = value;
                    OnPropertyChanged(() => Age);
                    OnPropertyChanged(() => BirthDay);
                    Editable.MarkDirty();
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
                    Editable.MarkDirty();
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

        public string FullName
        {
            get
            {
                return LastName + " " + FirstName + " " + MiddleName;
            }
        }

        public bool NoName
        {
            get
            {
                return patient.LastName == null && patient.MiddleName == null && patient.FirstName == null;
            }
        }


        public ObservableCollection<PropertyViewModel> Properties
        {
            get;
            private set;
        }

        #endregion Model related

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

        public string SearchText
        {
            get
            {
                return FullName;
            }
        }

        public void SetDoctorVM(DoctorViewModel doctor)
        {
            Contract.Requires(doctor != null);

            CurrentDoctor = doctor;
        }

        public void OpenLastAppointment()
        {
            // последний курс или новый, если курсов нет
            var lastCourse = CoursesManager.Courses.FirstOrDefault();
            if (lastCourse == null)
            {
                CurrentDoctor.StartCourse(this);
            }
            else
            {
                CoursesManager.SelectedCourse = lastCourse;
            }

            // последняя встреча в течение часа или новая
            var lastApp = CoursesManager.SelectedCourse.LastAppointment; // в курсе всегда есть встреча
            if (DateTime.UtcNow - lastApp.DateTime > TimeSpan.FromHours(1))
            {
                CoursesManager.SelectedCourse.AddAppointment();
            }
            else
            {
                CoursesManager.SelectedCourse.SelectedAppointment = lastApp;
            }
        }

        public PatientViewModel(Patient p)
        {
            Contract.Requires(p != null);

            patient = p;
            Editable = new EditableBase(this, switchedOn: true);

            if (!(this is UnsavedPatientViewModel))
                AfterPatientLoaded();
        }

        public void AfterPatientLoaded()
        {
            Properties = new ObservableCollection<PropertyViewModel>(EntityManagers.PropertyManager.GetPatientProperties(patient));
            CoursesManager = new CoursesManager(this);
        }

        #region Event handlers

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
                    var app = e.GetValue<AppointmentViewModel>(Messages.Appointment);

                    OnAppointmentAdded(app);
                }),
                this.Subscribe((int)EventID.HealthRecordSelected, (e) =>
                {
                    var hr = e.GetValue<HealthRecordViewModel>(Messages.HealthRecord);

                    OnHealthRecordSelected(hr);
                }),
                this.Subscribe((int)EventID.HealthRecordChanged, (e) =>
                {
                    var hr = e.GetValue<HealthRecordViewModel>(Messages.HealthRecord);

                    OnHealthRecordChanged(hr);
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
            patient.SetPropertyValue(propertyVM.property, propertyVM.SelectedValue);
            Editable.MarkDirty();
        }

        private void OnCourseStarted(Course course)
        {
            CoursesManager.AddCourse(course);
            CoursesManager.SelectedCourse.AddAppointment();
            Editable.MarkDirty();
        }

        private void OnAppointmentAdded(AppointmentViewModel app)
        {
            CoursesManager.SelectedCourse.SelectedAppointment.AddHealthRecord();
            Editable.MarkDirty();
        }

        private void OnHealthRecordSelected(HealthRecordViewModel hr)
        {
            hr.MakeCurrent();
        }

        private void OnHealthRecordChanged(HealthRecordViewModel hr)
        {
            Editable.MarkDirty();
        }
        #endregion Event handlers

        #region Comparsion

        public static int CompareByFullName(PatientViewModel x, PatientViewModel y)
        {
            if (x == null)
            {
                if (y == null)
                    return 0;
                else
                    return -1; // y greater
            }
            else
            {
                if (y == null)
                    return 1;
                else
                    return x.FullName.CompareTo(y.FullName);
            }
        }

        #endregion Comparsion

        public override string ToString()
        {
            return FullName;
        }
    }

    class UnsavedPatientViewModel : PatientViewModel
    {
        public event PatientEventHandler PatientCreated;

        /// <summary>
        /// For patient registration. First Editable.Committed raises PatientCreated.
        /// </summary>
        public UnsavedPatientViewModel()
            : base(new Patient())
        {
            Editable.Committed += OnFirstCommit;
        }
        private void OnFirstCommit(object sender, EditableEventArgs e)
        {
            Editable.Committed -= OnFirstCommit;
            var h = PatientCreated;
            if (h != null)
            {
                h(this, new PatientEventArgs(this));
            }
        }

    }

    public delegate void PatientEventHandler(object sender, PatientEventArgs e);

    public class PatientEventArgs : EventArgs
    {
        public PatientViewModel patientVM;

        public PatientEventArgs(PatientViewModel p)
        {
            patientVM = p;
        }
    }
}