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
                return patient.LastName ?? "";
            }
            set
            {
                if (patient.LastName != value)
                {
                    patient.LastName = value;

                    OnPropertyChanged(() => LastName);
                    OnPropertyChanged(() => ShortName);
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

        public string FullName
        {
            get
            {
                return LastName + " " + FirstName + " " + MiddleName;
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
                    var app = e.GetValue<AppointmentViewModel>(Messages.Appointment);

                    OnAppointmentAdded(app);
                }),
                this.Subscribe((int)EventID.HealthRecordSelected, (e) =>
                {
                    var hr = e.GetValue<HealthRecordViewModel>(Messages.HealthRecord);

                    OnHealthRecordSelected(hr);
                }),
                this.Subscribe((int)EventID.WordCheckedChanged, (e) =>
                {
                    var symptom = e.GetValue<WordViewModel>(Messages.Word);
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

        private void OnAppointmentAdded(AppointmentViewModel app)
        {
            Editable.MarkDirty();
        }

        private void OnHealthRecordSelected(HealthRecordViewModel hr)
        {
            hrSelecting = true;
            hr.MakeCurrent();
            hrSelecting = false;
        }

        private void OnSymptomCheckedChanged(WordViewModel symptom, bool isChecked)
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
    }

    class NewPatientViewModel : PatientViewModel
    {
        #region Fields

        string fn;
        string mn;
        string ln;
        int? by;
        byte? bm;
        byte? bd;
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
                    fn = value;
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
                    ln = value;
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
                try
                {
                    int age = DateTime.Today.Year - BirthYear.Value;

                    if (new DateTime(BirthYear.Value, BirthMonth.Value, BirthDay.Value) > DateTime.Today.AddYears(-age))
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
                if (value.HasValue)
                {
                    int year = DateTime.Today.Year - value.Value;

                    // TODO check setting year for 29 feb case
                    if (BirthMonth.HasValue && BirthDay.HasValue &&
                        new DateTime(year, BirthMonth.Value, BirthDay.Value) < DateTime.Today.AddYears(-value.Value))
                        year--;

                    BirthYear = year;
                }
            }
        }

        public new int? BirthYear
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

        public new byte? BirthMonth
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

        public new byte? BirthDay
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
            IsMale = true;

            Editable.Committed += (s, e) =>
                OnPatientCreated(new PatientEventArgs(
                    new PatientViewModel(
                        new Patient(LastName,
                                    FirstName,
                                    MiddleName,
                                    BirthYear, BirthMonth, BirthDay,
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