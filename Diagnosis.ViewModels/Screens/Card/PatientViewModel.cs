using Diagnosis.Core;
using Diagnosis.Models;
using EventAggregator;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class PatientViewModel : ViewModelBase
    {
        internal readonly Patient patient;
        private ShortCourseViewModel _selectedCourse;
        private CoursesManager coursesManager;

        #region Model

        public string Label
        {
            get
            {
                return patient.Label;
            }
            set
            {
                patient.Label = value;
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
                patient.FirstName = value;
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
                patient.MiddleName = value;
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
                patient.LastName = value;
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
                patient.Age = value;
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
                patient.BirthYear = value;
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
                patient.BirthMonth = value;
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
                patient.BirthDay = value;
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
                patient.IsMale = value;
            }
        }
        #endregion Model related

        public ObservableCollection<ShortCourseViewModel> Courses { get { return coursesManager.Courses; } }

        public ShortCourseViewModel SelectedCourse
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
                    OnPropertyChanged(() => SelectedCourse);
                }
            }
        }

        public ICommand StartCourseCommand
        {
            get
            {
                return new RelayCommand(() => AuthorityController.CurrentDoctor.StartCourse(patient));
            }
        }

        public ObservableCollection<PropertyViewModel> Properties
        {
            get;
            private set;
        }

        public RelayCommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                       {
                           this.Send(Events.EditPatient, patient.AsParams(MessageKeys.Patient));
                       });
            }
        }

        public bool NoCourses
        {
            get
            {
                return Courses.Count == 0;
            }
        }

        public bool NoName
        {
            get
            {
                return patient.LastName == null && patient.MiddleName == null && patient.FirstName == null;
            }
        }

        public PatientViewModel(Patient p)
        {
            Contract.Requires(p != null);
            this.patient = p;

            coursesManager = new CoursesManager(patient);
            coursesManager.CoursesLoaded += (s, e) =>
            {
                Courses.CollectionChanged += (s1, e1) =>
                {
                    OnPropertyChanged("NoCourses");
                };
            };

            patient.PropertyChanged += patient_PropertyChanged;
            /// TODO patientproperties changes

            LoadProperties();
        }

        public void SelectCourse(Course course)
        {
            SelectedCourse = Courses.First(x => x.course == course);
        }

        private void patient_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            switch (e.PropertyName)
            {
                case "FirstName":
                case "LastName":
                case "MiddleName":
                    OnPropertyChanged("NoName");
                    break;

                default:
                    break;
            }
        }

        private void LoadProperties()
        {
            var allProperties = new Diagnosis.Data.Repositories.PropertyRepository().GetAll();

            var existingPatProps = patient.PatientProperties;

            var properties = new List<PropertyViewModel>(allProperties.Select(p => MakePropertyVM(p)));

            // указываем значение свойства из БД
            // если у пацента не указано какое-то свойство — добавляем это свойство с пустым значением
            foreach (var propVM in properties)
            {
                var pp = existingPatProps.FirstOrDefault(patProp => patProp.Property == propVM.property);
                if (pp != null)
                {
                    propVM.SelectedValue = pp.Value;
                }
                else
                {
                    propVM.SelectedValue = new EmptyPropertyValue(propVM.property);
                }
            }

            Properties = new ObservableCollection<PropertyViewModel>(properties);
        }

        private PropertyViewModel MakePropertyVM(Property p)
        {
            var vm = new PropertyViewModel(p);
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "SelectedValue")
                {
                    if (!(vm.SelectedValue is EmptyPropertyValue))
                    {
                        patient.SetPropertyValue(vm.property, vm.SelectedValue);
                    }
                }
            };
            return vm;
        }

        public override string ToString()
        {
            return patient.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            patient.PropertyChanged -= patient_PropertyChanged;

            base.Dispose(disposing);
        }
    }
}