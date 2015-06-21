using Diagnosis.Common;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class PatientViewModel : CheckableBase
    {
        internal readonly Patient patient;

        public PatientViewModel(Patient p)
        {
            Contract.Requires(p != null);
            this.patient = p;
            this.validatableEntity = p;

            patient.PropertyChanged += patient_PropertyChanged;
        }

        #region Model

        public string FirstName
        {
            get { return patient.FirstName ?? ""; }
            set { patient.FirstName = value; }
        }

        public string MiddleName
        {
            get { return patient.MiddleName ?? ""; }
            set { patient.MiddleName = value; }
        }

        public string LastName
        {
            get { return patient.LastName ?? ""; }
            set { patient.LastName = value; }
        }

        public string FullName
        {
            get { return NameFormatter.GetFullName(patient); }
            set
            {
                if (null != value)
                {
                    var names = value.Split(' ');

                    LastName = names[0];
                    FirstName = names.Length > 1 ? names[1] : null;
                    MiddleName = names.Length > 2 ? string.Join(" ", names.Skip(2)) : null; // если больше 2 пробелов
                }
            }
        }

        public string FullNameOrCreatedAt
        {
            get { return patient.FullNameOrCreatedAt; }
        }

        public DateTime CreatedAt
        {
            get { return patient.CreatedAt; }
        }

        public int? Age
        {
            get { return patient.Age; }
            set { patient.Age = value; }
        }

        public int? BirthYear
        {
            get { return patient.BirthYear; }
            set { patient.BirthYear = value; }
        }

        public int? BirthMonth
        {
            get { return patient.BirthMonth; }
            set { patient.BirthMonth = value; }
        }

        public int? BirthDay
        {
            get { return patient.BirthDay; }
            set { patient.BirthDay = value; }
        }

        public DateTime LastHrUpdatedAt
        {
            get { return patient.GetLastHrUpdatedAt(); }
        }

        public bool IsMale
        {
            get { return patient.IsMale.HasValue && patient.IsMale.Value; }
            set { if (value) patient.IsMale = true; }
        }

        public bool IsFemale
        {
            get { return patient.IsMale.HasValue && !patient.IsMale.Value; }
            set { if (value) patient.IsMale = false; }
        }

        public bool UnknownSex
        {
            get { return patient.IsMale == null; }
            set { if (value) patient.IsMale = null; }
        }
        #endregion Model

        public RelayCommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                       {
                           this.Send(Event.EditPatient, patient.AsParams(MessageKeys.Patient));
                       });
            }
        }

        public RelayCommand StartCourseCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var course = patient.AddCourse(AuthorityController.CurrentDoctor);
                });
            }
        }

        public bool NoCourses
        {
            get
            {
                return patient.Courses.Count() == 0;
            }
        }

        private void patient_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
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
            }
            base.Dispose(disposing);
        }
    }
}