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
        internal readonly Patient patient;

        private DoctorViewModel _doctor;
        private CoursesManager _coursesManager;
        private List<EventMessageHandler> msgHandlers = new List<EventMessageHandler>();
        private bool hrSelecting;

        public EditableBase Editable { get; private set; }

        #region Model related

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
                    Editable.MarkDirty();
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
                    Editable.MarkDirty();
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
                    Editable.MarkDirty();
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
                    Editable.MarkDirty();
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

        public string Name
        {
            get
            {
                return ShortName;
            }
        }

        public void SetDoctorVM(DoctorViewModel doctor)
        {
            Contract.Requires(doctor != null);

            CurrentDoctor = doctor;
        }

        public PatientViewModel(Patient p)
        {
            Contract.Requires(p != null);

            patient = p;
            Editable = new EditableBase(this, switchedOn: true);

            Properties = new ObservableCollection<PropertyViewModel>(EntityManagers.PropertyManager.GetPatientProperties(patient));
            CoursesManager = new CoursesManager(this);
        }

        protected PatientViewModel()
        {
            Editable = new EditableBase(this, switchedOn: true);
            Properties = new ObservableCollection<PropertyViewModel>();
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
                this.Subscribe((int)EventID.DiagnosisCheckedChanged, (e) =>
                {
                    var diagnosis = e.GetValue<DiagnosisViewModel>(Messages.Diagnosis);
                    var isChecked = e.GetValue<bool>(Messages.CheckedState);

                    OnDiagnosisCheckedChanged(diagnosis, isChecked);
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
            Editable.MarkDirty();
        }

        private void OnAppointmentAdded(Appointment app)
        {
            Editable.MarkDirty();
        }

        private void OnHealthRecordSelected(HealthRecordViewModel hr)
        {
            hrSelecting = true;
            hr.MakeCurrent();
            hrSelecting = false;
        }

        private void OnSymptomCheckedChanged(SymptomViewModel symptom, bool isChecked)
        {
            if (!hrSelecting)
                Editable.MarkDirty();
        }

        private void OnDiagnosisCheckedChanged(DiagnosisViewModel diagnosis, bool isChecked)
        {
            if (!hrSelecting)
                Editable.MarkDirty();
        }

        #endregion Event handlers

        #region Comparsion

        public static int CompareByName(PatientViewModel x, PatientViewModel y)
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
                    return x.Name.CompareTo(y.Name);
            }
        }

        #endregion Comparsion
    }

    class NewPatientViewModel : PatientViewModel
    {
        #region Fields

        string fn;
        string mn;
        string ln;
        int by;
        int bm;
        int bd;
        bool isMale;
        string snils;

        #endregion

        public event PatientEventHandler PatientCreated;

        public new string FirstName
        {
            get
            {
                return fn;
            }
            set
            {
                if (fn != value)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        fn = value;
                    }
                    OnPropertyChanged(() => FirstName);
                    OnPropertyChanged(() => ShortName);
                    Editable.MarkDirty();
                }
            }
        }

        public new string MiddleName
        {
            get
            {
                return mn;
            }
            set
            {
                if (mn != value)
                {
                    mn = value;
                    OnPropertyChanged(() => MiddleName);
                    OnPropertyChanged(() => ShortName);
                    Editable.MarkDirty();
                }
            }
        }

        public new string LastName
        {
            get
            {
                return ln;
            }
            set
            {
                if (ln != value)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        ln = value;
                    }
                    OnPropertyChanged(() => LastName);
                    OnPropertyChanged(() => ShortName);
                    Editable.MarkDirty();
                }
            }
        }

        public new int? Age
        {
            get
            {
                int age = DateTime.Today.Year - by;
                try
                {
                    if (new DateTime(by, bm, bd) > DateTime.Today.AddYears(-age))
                        age--;
                    return age;
                }
                catch
                {
                    return null;
                }

            }
            set
            {

            }
        }

        public new int BirthYear
        {
            get
            {
                return by;
            }
            set
            {
                if (by != value && value >= 0 && value <= DateTime.Today.Year)
                {
                    by = value;
                    OnPropertyChanged(() => Age);
                    OnPropertyChanged(() => BirthYear);
                    Editable.MarkDirty();
                }
            }
        }

        public new int BirthMonth
        {
            get
            {
                return bm;
            }
            set
            {
                if (bm != value && value >= 1 && value <= 12)
                {
                    bm = value;
                    OnPropertyChanged(() => Age);
                    OnPropertyChanged(() => BirthMonth);
                    Editable.MarkDirty();
                }
            }
        }

        public new int BirthDay
        {
            get
            {
                return bd;
            }
            set
            {
                if (bd != value && value >= 1 && value <= 31)
                {
                    bd = value;

                    OnPropertyChanged(() => Age);
                    OnPropertyChanged(() => BirthDay);
                    Editable.MarkDirty();
                }
            }
        }

        public new bool IsMale
        {
            get
            {
                return isMale;
            }
            set
            {
                if (isMale != value)
                {
                    isMale = value;
                    OnPropertyChanged(() => IsMale);
                    Editable.MarkDirty();
                }
            }
        }

        public new string Snils
        {
            get
            {
                return snils;
            }
            set
            {
                if (snils != value)
                {
                    snils = value;
                    OnPropertyChanged(() => Snils);
                    Editable.MarkDirty();
                }
            }
        }

        public NewPatientViewModel()
        {
            Editable.Committed += (s, e) =>
                OnPatientCreated(new PatientEventArgs(
                    new PatientViewModel(
                        new Patient(LastName,
                                    FirstName,
                                    MiddleName,
                                    new DateTime(BirthYear, BirthMonth, BirthDay),
                                    IsMale))
                        {
                            Snils = this.Snils
                        }
                    ));
        }

        protected virtual void OnPatientCreated(PatientEventArgs e)
        {
            var h = PatientCreated;
            if (h != null)
            {
                h(this, e);
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