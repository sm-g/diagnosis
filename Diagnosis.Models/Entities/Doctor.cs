using Diagnosis.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class Doctor : EntityBase
    {
        private ISet<Course> courses = new HashSet<Course>();
        private ISet<Appointment> appointments = new HashSet<Appointment>();
        private string _fn;
        private string _ln;
        private string _mn;
        private int _settings;
        private DoctorSettings _docSettings;
        private ObservableCollection<Course> _courses;

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

        public virtual ReadOnlyCollection<Course> Courses
        {
            get
            {
                return new ReadOnlyCollection<Course>(
                    new List<Course>(courses));
            }
        }

        public virtual ReadOnlyCollection<Appointment> Appointments
        {
            get
            {
                return new ReadOnlyCollection<Appointment>(
                    new List<Appointment>(appointments));
            }
        }

        public virtual Course StartCourse(Patient patient)
        {
            Contract.Requires(patient != null);

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