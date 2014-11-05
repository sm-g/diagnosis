using Diagnosis.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Iesi.Collections.Generic;

namespace Diagnosis.Models
{
    public class Doctor : EntityBase, IDomainObject, IMan
    {
        private Iesi.Collections.Generic.ISet<Course> courses = new HashedSet<Course>();
        private Iesi.Collections.Generic.ISet<Appointment> appointments;
        private string _fn;
        private string _ln;
        private string _mn;
        private int _settings;
        private DoctorSettings _docSettings;

        public virtual DoctorSettings DoctorSettings
        {
            get { return _docSettings; }
            set
            {
                _docSettings = value;
                _settings = (int)_docSettings;
            }
        }

        public virtual string FirstName
        {
            get
            {
                return _fn;
            }
            set
            {
                Contract.Requires(!String.IsNullOrWhiteSpace(value));
                _fn = value.Trim();
            }
        }

        public virtual string MiddleName
        {
            get
            {
                return _mn;
            }
            set
            {
                _mn = value.TrimedOrNull();
            }
        }

        public virtual string LastName
        {
            get
            {
                return _ln;
            }
            set
            {
                Contract.Requires(!String.IsNullOrWhiteSpace(value));
                _ln = value.Trim();
            }
        }

        public virtual bool IsMale { get; set; }

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

        public virtual Speciality Speciality { get; set; }

        public virtual IEnumerable<Course> Courses
        {
            get
            {
                return courses;
            }
        }

        public virtual IEnumerable<Appointment> Appointments
        {
            get
            {
                return appointments;
            }
        }

        public virtual Course StartCourse(Patient patient)
        {
            Contract.Requires(patient != null);
            Contract.Ensures(patient.Courses.Contains(Contract.Result<Course>()));

            var course = new Course(patient, this);
            courses.Add(course);
            patient.AddCourse(course);
            return course;
        }

        public Doctor(string lastName, string firstName, string middleName = null, Speciality speciality = null)
        {
            Contract.Requires(lastName != null);
            Contract.Requires(firstName != null);

            LastName = lastName;
            FirstName = firstName;
            MiddleName = middleName;
            Speciality = speciality;
            IsMale = true;
        }

        protected Doctor()
        {
        }

        public override string ToString()
        {
            return LastName + " " + FirstName + " " + MiddleName;
        }
    }
}