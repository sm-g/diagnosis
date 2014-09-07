﻿using Diagnosis.Core;
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
    public class PatientViewModel : ViewModelBase
    {
        internal readonly Patient patient;
        private CoursesManager coursesManager;

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
                    OnPropertyChanged("NoName");
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
                    OnPropertyChanged("NoName");
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
                    OnPropertyChanged("NoName");
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

        public bool IsUnsaved
        {
            get
            {
                return patient.Id == 0;
            }
        }

        public ObservableCollection<ShortCourseViewModel> Courses { get { return coursesManager.Courses; } }

        private ShortCourseViewModel _selectedCourse;
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


        public bool NoCourses
        {
            get
            {
                return Courses.Count == 0;
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

            if (!IsUnsaved)
                LoadProperties();

        }

        public void SelectCourse(Course course)
        {
            SelectedCourse = Courses.First(x => x.course == course);
        }

        /// <summary>
        /// Только для сохраненного пациента.
        /// </summary>
        void LoadProperties()
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
            OnPropertyChanged("Properties");
        }

        PropertyViewModel MakePropertyVM(Property p)
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
    }

}