using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System;

namespace Diagnosis.Models
{
    public class Doctor
    {
        ISet<Course> courses = new HashSet<Course>();
        ISet<Appointment> appointments = new HashSet<Appointment>();
        string _fn;
        string _ln;
        string _mn;

        public virtual int Id { get; protected set; }
        public virtual string FirstName
        {
            get
            {
                return _fn;
            }
            set
            {
                Contract.Requires(!String.IsNullOrEmpty(value));
                _fn = value;
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
                if (value == "")
                {
                    _mn = null;
                }
                else
                {
                    _mn = value;
                }
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
                Contract.Requires(!String.IsNullOrEmpty(value));
                _ln = value;
            }
        }
        public virtual bool IsMale { get; set; }
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

        protected Doctor() { }
    }
}
