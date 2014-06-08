using Diagnosis.App.Messaging;
using Diagnosis.Core;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class PatientViewModel : CheckableBase, IEditableNesting
    {
        internal readonly Patient patient;

        private DoctorViewModel _doctor;
        private RelayCommand _firstHr;
        private bool _canAddFirstHr;
        private CoursesManager _coursesManager;
        private List<EventMessageHandler> msgHandlers = new List<EventMessageHandler>();

        #region IEditableNesting

        public Editable Editable { get; private set; }

        /// <summary>
        /// Пациент пустой, если пусты все его курсы.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return CoursesManager.Courses.All(x => x.IsEmpty);
            }
        }

        #endregion IEditableNesting

        #region Model related

        public string Label
        {
            get
            {
                return patient.Label;
            }
            set
            {
                if (patient.Label != value)
                {
                    patient.Label = value;
                    OnPropertyChanged("Label");
                    Editable.MarkDirty();
                }
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

                    OnPropertyChanged("FirstName");
                    OnPropertyChanged("SearchText");
                    OnPropertyChanged("NoName");
                    OnPropertyChanged("Self");
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

                    OnPropertyChanged("MiddleName");
                    OnPropertyChanged("SearchText");
                    OnPropertyChanged("NoName");
                    OnPropertyChanged("Self");
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

                    OnPropertyChanged("LastName");
                    OnPropertyChanged("SearchText");
                    OnPropertyChanged("NoName");
                    OnPropertyChanged("Self");
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
                    OnPropertyChanged("Age");
                    OnPropertyChanged("BirthYear");
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
                    OnPropertyChanged("Age");
                    OnPropertyChanged("BirthYear");
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
                    OnPropertyChanged("Age");
                    OnPropertyChanged("BirthMonth");
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
                    OnPropertyChanged("Age");
                    OnPropertyChanged("BirthDay");
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
                if (patient.IsMale != value && EntityManagers.PatientsManager.CurrentPatient == this) // fix binding when change CurrentScreen
                {
                    patient.IsMale = value;
                    OnPropertyChanged("IsMale");
                    Editable.MarkDirty();
                }
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

        internal string FullName
        {
            get
            {
                return LastName + " " + FirstName + " " + MiddleName;
            }
        }

        #endregion Model related

        public ICommand FirstHrCommand
        {
            get
            {
                return _firstHr
                    ?? (_firstHr = new RelayCommand(
                                          () =>
                                          {
                                              // go to courses tabitem - save patient first
                                              CanAddFirstHr = false;
                                              Editable.Commit(true);
                                          }, () => CanAddFirstHr));
            }
        }

        public bool CanAddFirstHr
        {
            get
            {
                return _canAddFirstHr;
            }
            set
            {
                if (_canAddFirstHr != value)
                {
                    _canAddFirstHr = value;
                    OnPropertyChanged(() => CanAddFirstHr);
                }
            }
        }

        public bool IsUnsaved
        {
            get
            {
                return this is UnsavedPatientViewModel;
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
                    OnPropertyChanged("CoursesManager");
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
                    OnPropertyChanged("CurrentDoctor");
                }
            }
        }

        public bool NoCourses
        {
            get
            {
                return CoursesManager.Courses.Count == 0;
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
            Editable = new Editable(this, switchedOn: true);
            CoursesManager = new CoursesManager(this);
            if (!(this is UnsavedPatientViewModel))
                AfterPatientLoaded();

            this.SubscribeEditableNesting(CoursesManager.Courses);
            CoursesManager.Courses.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged("NoCourses");
            };
            Editable.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "IsEditorActive")
                {
                    // Для сохранения при выходе из редактора.
                    // При обратном переходе может редактироваться запись — не сохраняем.
                    if (!Editable.IsEditorActive)
                    {
                        Editable.Commit();
                        Editable.IsEditorActive = false;
                    }
                }
            };
        }

        public void AfterPatientLoaded()
        {
            Properties = new ObservableCollection<PropertyViewModel>(
                EntityManagers.PropertyManager.GetPatientProperties(patient));
            OnPropertyChanged("Properties");
        }

        #region Subscriptions

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
                })
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
            if (!(propertyVM.SelectedValue is EmptyPropertyValue))
            {
                patient.SetPropertyValue(propertyVM.property, propertyVM.SelectedValue);
                Editable.MarkDirty();
            }
        }

        private void OnCourseStarted(Course course)
        {
            CoursesManager.AddCourse(course);
            Editable.MarkDirty();
        }

        #endregion Subscriptions

        #region Comparsion

        private static IComparer<string> emptyLastComparer = new EmptyStringsAreLast();

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
                    return emptyLastComparer.Compare(x.FullName, y.FullName);
            }
        }

        #endregion Comparsion

        public override string ToString()
        {
            return patient.Id + " " + Label + " " + FullName;
        }
    }

    internal class UnsavedPatientViewModel : PatientViewModel
    {
        public event PatientEventHandler PatientCreated;

        /// <summary>
        /// For patient registration. First Editable.Committed raises PatientCreated.
        /// </summary>
        public UnsavedPatientViewModel()
            : base(new Patient())
        {
            Editable.IsEditorActive = true;
            CanAddFirstHr = true;
            Editable.Committed += OnFirstCommit;
        }

        private void OnFirstCommit(object sender, EditableEventArgs e)
        {
            Editable.Committed -= OnFirstCommit;

            var h = PatientCreated;
            if (h != null)
            {
                h(this, new PatientEventArgs(this, !CanAddFirstHr));
            }
        }
    }

    public delegate void PatientEventHandler(object sender, PatientEventArgs e);

    public class PatientEventArgs : EventArgs
    {
        public PatientViewModel patientVM;
        public bool addFirstHr;

        [DebuggerStepThrough]
        public PatientEventArgs(PatientViewModel p, bool addFirstHr)
        {
            patientVM = p;
            this.addFirstHr = addFirstHr;
        }
    }
}