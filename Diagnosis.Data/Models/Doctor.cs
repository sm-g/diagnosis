using System.Collections.Generic;
using System.Collections.ObjectModel;

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
    }
}
