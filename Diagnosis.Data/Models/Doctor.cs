using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class Doctor
    {
        ISet<Course> courses = new HashSet<Course>();
        ISet<Appointment> appointments = new HashSet<Appointment>();

        public virtual int Id { get; protected set; }
        public virtual string FirstName { get; set; }
        public virtual string MiddleName { get; set; }
        public virtual string LastName { get; set; }
        public virtual bool IsMale { get; set; }
        public virtual string Speciality { get; set; }
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

        public virtual void StartCourse(Patient patient)
        {
            Contract.Requires(patient != null);

            var course = new Course(patient, this);
            courses.Add(course);
        }

        public Doctor(string lastName, string firstName, string speciality = "")
        {
            Contract.Requires(lastName != null);
            Contract.Requires(firstName != null);

            LastName = lastName;
            FirstName = firstName;
            Speciality = speciality;
            IsMale = true;
        }

        protected Doctor() { }
    }
}
