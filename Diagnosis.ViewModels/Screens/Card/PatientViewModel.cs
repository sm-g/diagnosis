using Diagnosis.Core;
using Diagnosis.Models;
using EventAggregator;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;
using FluentValidation.Results;
using System;

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

        public override string this[string columnName]
        {
            get
            {
                var results = patient.SelfValidate();
                if (results == null)
                    return string.Empty;
                var message = results.Errors
                    .Where(x => x.PropertyName == columnName)
                    .Select(x => x.ErrorMessage)
                    .FirstOrDefault();
                return message != null ? message : string.Empty;
            }
        }

        public PatientViewModel(Patient p)
        {
            Contract.Requires(p != null);
            this.patient = p;
            patient.PropertyChanged += patient_PropertyChanged;

            coursesManager = new CoursesManager(patient, onCoursesChanged: (s, e) =>
            {
                OnPropertyChanged("NoCourses");
            });

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

            var propVms = new List<PropertyViewModel>(allProperties.Select(p =>
            {
                var vm = new PropertyViewModel(p);
                vm.PropertyChanged += propVm_PropertyChanged;
                return vm;
            }));

            // указываем значение свойства из БД
            // если у пацента не указано какое-то свойство — добавляем это свойство с пустым значением
            foreach (var propVM in propVms)
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

            Properties = new ObservableCollection<PropertyViewModel>(propVms);
        }

        void propVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedValue")
            {
                var propVM = sender as PropertyViewModel;
                if (!(propVM.SelectedValue is EmptyPropertyValue))
                {
                    patient.SetPropertyValue(propVM.property, propVM.SelectedValue);
                }
            }
        }

        public override string ToString()
        {
            return patient.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                patient.PropertyChanged -= patient_PropertyChanged;
                foreach (var item in Properties)
                {
                    item.PropertyChanged -= propVm_PropertyChanged;
                }
            }
            base.Dispose(disposing);
        }
    }
}