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

namespace Diagnosis.ViewModels
{
    public class PatientViewModel : CheckableBase, IEditableNesting
    {
        internal readonly Patient patient;
        internal Func<CourseViewModel> OpenedCourseGetter;
        internal Action<CourseViewModel> OpenedCourseSetter;

        private CoursesManager coursesManager;
        private EventMessageHandlersManager msgManager = new EventMessageHandlersManager();

        private bool _canAddFirstHr;

        #region IEditableNesting

        public Editable Editable { get; private set; }

        /// <summary>
        /// Пациент пустой, если пусты все его курсы.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return Courses.All(x => x.IsEmpty);
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
                    OnPropertyChanged("IsMale");
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

        #endregion Model related

        public ICommand FirstHrCommand
        {
            get
            {
                return new RelayCommand(
                                          () =>
                                          {
                                              // go to courses tabitem - save patient first
                                              Editable.Commit();
                                          }, () => CanAddFirstHr);
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

        public ObservableCollection<CourseViewModel> Courses { get { return coursesManager.Courses; } }

        public CourseViewModel SelectedCourse
        {
            get
            {
                return OpenedCourseGetter != null ? OpenedCourseGetter() : null;
            }
            set
            {
                OpenedCourseSetter(value);
            }
        }

        public ICommand StartCourseCommand
        {
            get
            {
                return EntityProducers.DoctorsProducer.CurrentDoctor.StartCourseCommand;
            }
        }


        public bool NoCourses
        {
            get
            {
                return Courses.Count == 0;
            }
        }

        public string SearchText
        {
            get
            {
                return patient.FullName;
            }
        }

        public PatientViewModel(Patient p)
        {
            Contract.Requires(p != null);
            this.patient = p;

            Editable = new Editable(patient);

            coursesManager = new CoursesManager(this);
            coursesManager.CoursesLoaded += (s, e) =>
            {
                Courses.CollectionChanged += (s1, e1) =>
                {
                    OnPropertyChanged("NoCourses");
                };
            };

            if (!(this is UnsavedPatientViewModel))
                AfterPatientLoaded();

            Editable.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "IsEditorActive")
                {
                    // Для сохранения при выходе из редактора.
                    // При обратном переходе может редактироваться запись — не сохраняем.
                    if (!Editable.IsEditorActive)
                    {
                        if (!Editable.Commit())
                            Editable.IsEditorActive = false;
                    }
                }
            };
        }

        /// <summary>
        /// Только для сохраненного пациента.
        /// </summary>
        public void AfterPatientLoaded()
        {
            Properties = new ObservableCollection<PropertyViewModel>(
                EntityProducers.PropertyProducer.GetPatientProperties(patient));
            OnPropertyChanged("Properties");
        }

        /// <summary>
        /// Вызывается при смене открытого курса.
        /// </summary>
        internal void OnOpenedCourseChanged()
        {
            OnPropertyChanged("SelectedCourse");
        }

        #region Subscriptions

        public void Subscribe()
        {
            msgManager.Add(new[] {
                this.Subscribe(Events.PropertySelectedValueChanged, (e) =>
                {
                    var property = e.GetValue<PropertyViewModel>(MessageKeys.Property);

                    OnPropertyValueChanged(property);
                })
            });
        }

        public void Unsubscribe()
        {
            msgManager.Dispose();
        }

        private void OnPropertyValueChanged(PropertyViewModel propertyVM)
        {
            if (!(propertyVM.SelectedValue is EmptyPropertyValue))
            {
                patient.SetPropertyValue(propertyVM.property, propertyVM.SelectedValue);
            }
        }

        #endregion Subscriptions

        public override string ToString()
        {
            return patient.ToString();
        }
    }

    internal class UnsavedPatientViewModel : PatientViewModel
    {
        public event PatientEventHandler PatientCreated;

        private static Random rnd = new Random();

        /// <summary>
        /// For patient registration. First Editable.Committed raises PatientCreated.
        /// </summary>
        public UnsavedPatientViewModel()
            : base(new Patient())
        {
            Editable.IsEditorActive = true;
            CanAddFirstHr = true;

            Label = rnd.Next(1, 500).ToString();

            Editable.Committed += OnFirstCommit;
        }

        private void OnFirstCommit(object sender, EditableEventArgs e)
        {
            Editable.Committed -= OnFirstCommit;
            CanAddFirstHr = false;
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