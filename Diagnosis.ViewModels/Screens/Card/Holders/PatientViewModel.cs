﻿using Diagnosis.Common;
using Diagnosis.Models;
using EventAggregator;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;
using FluentValidation.Results;
using System;

namespace Diagnosis.ViewModels.Screens
{
    public class PatientViewModel : ViewModelBase
    {
        internal readonly Patient patient;
        private ShortCourseViewModel _selectedCourse;
        private CoursesManager coursesManager;

        #region Model
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

        public int? BirthMonth
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

        public int? BirthDay
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
                return patient.IsMale.HasValue && patient.IsMale.Value;
            }
            set
            {
                if (value)
                    patient.IsMale = true;
            }
        }

        public bool IsFemale
        {
            get
            {
                return patient.IsMale.HasValue && !patient.IsMale.Value;
            }
            set
            {
                if (value)
                    patient.IsMale = false;
            }
        }
        public bool UnknownSex
        {
            get
            {
                return patient.IsMale == null;
            }
            set
            {
                if (value)
                    patient.IsMale = null;
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

        public RelayCommand StartCourseCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var course = AuthorityController.CurrentDoctor.StartCourse(patient);
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
            this.validatableEntity = p;

            patient.PropertyChanged += patient_PropertyChanged;

            coursesManager = new CoursesManager(patient, onCoursesChanged: (s, e) =>
            {
                OnPropertyChanged("NoCourses");
            });
        }

        public void SelectCourse(Course course)
        {
            SelectedCourse = Courses.FirstOrDefault(x => x.course == course);
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

        public override string ToString()
        {
            return string.Format("{0} {1}", GetType().Name, patient);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                patient.PropertyChanged -= patient_PropertyChanged;
                coursesManager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}