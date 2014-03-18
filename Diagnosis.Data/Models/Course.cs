using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Diagnosis.Models
{
    public class Course
    {
        ISet<Appointment> appointments = new HashSet<Appointment>();

        public virtual int Id { get; protected set; }
        public virtual Patient Patient { get; set; }
        public virtual Doctor LeadDoctor { get; set; }
        public virtual DateTime Start { get; set; }
        public virtual DateTime? End { get; set; }
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
