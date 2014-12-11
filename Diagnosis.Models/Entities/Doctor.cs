using Diagnosis.Common;
using Diagnosis.Models.Validators;
using FluentValidation.Results;
using Iesi.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    public class Doctor : ValidatableEntity<Guid>, IDomainObject, IMan, IUser
    {
        private Iesi.Collections.Generic.ISet<Appointment> appointments = new HashedSet<Appointment>();
        private Iesi.Collections.Generic.ISet<Course> courses = new HashedSet<Course>();
        private string _fn;
        private string _ln;
        private string _mn;
        private int _settings;
        private DoctorSettings _docSettings;
        private bool _isMale;
        private Speciality _speciality;
        private Passport passport;

        public virtual DoctorSettings DoctorSettings
        {
            get { return _docSettings; }
            set
            {
                _docSettings = value;
                _settings = (int)_docSettings;
                SetProperty(ref _docSettings, value, () => DoctorSettings);
            }
        }

        public virtual string FirstName
        {
            get { return _fn; }
            set
            {
                SetProperty(ref _fn, value.TrimedOrNull(), "FirstName");
            }
        }

        public virtual string MiddleName
        {
            get { return _mn; }
            set
            {
                SetProperty(ref _mn, value.TrimedOrNull(), "MiddleName");
            }
        }

        public virtual string LastName
        {
            get { return _ln; }
            set
            {
                Contract.Requires(!String.IsNullOrWhiteSpace(value));
                SetProperty(ref _ln, value.Trim(), "LastName");
            }
        }

        public virtual bool IsMale
        {
            get { return _isMale; }
            set { SetProperty(ref _isMale, value, () => IsMale); }
        }

        public virtual int Settings
        {
            get { return _settings; }
            set
            {
                if (value >= 0)
                {
                    _settings = value;
                    _docSettings = (DoctorSettings)value;
                }
            }
        }

        public virtual Speciality Speciality
        {
            get { return _speciality; }
            set
            {
                if (value == Speciality.Null) value = null;
                SetProperty(ref _speciality, value, () => Speciality);
            }
        }

        public virtual Passport Passport
        {
            get { return passport; }
            set
            {
                SetProperty(ref passport, value, () => Passport);
            }
        }

        public virtual IEnumerable<Appointment> Appointments
        {
            get { return appointments; }
        }

        public virtual IEnumerable<Course> Courses
        {
            get { return courses; }
        }

        public virtual string FullName
        {
            get
            {
                return LastName + " " + FirstName + " " + MiddleName;
            }
        }

        public virtual Course StartCourse(Patient patient)
        {
            Contract.Requires(patient != null);
            Contract.Ensures(patient.Courses.Contains(Contract.Result<Course>()));

            var course = new Course(patient, this);
            patient.AddCourse(course);
            return course;
        }

        public Doctor(string lastName, string firstName = null, string middleName = null, Speciality speciality = null)
        {
            Contract.Requires(!lastName.IsNullOrEmpty());

            LastName = lastName;
            FirstName = firstName;
            MiddleName = middleName;
            Speciality = speciality;
            IsMale = true;
            Passport = new Passport(this);
        }

        protected Doctor()
        {
        }

        public override string ToString()
        {
            return FullName;
        }

        public override ValidationResult SelfValidate()
        {
            return new DoctorValidator().Validate(this);
        }


    }
}