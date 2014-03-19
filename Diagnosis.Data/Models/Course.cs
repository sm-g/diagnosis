using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class Course
    {
        ISet<Appointment> appointments = new HashSet<Appointment>();

        public virtual int Id { get; protected set; }
        public virtual Patient Patient { get; protected set; }
        public virtual Doctor LeadDoctor { get; protected set; }
        public virtual DateTime Start { get; protected set; }
        public virtual DateTime? End { get; set; }
        public virtual ReadOnlyCollection<Appointment> Appointments
        {
            get
            {
                return new ReadOnlyCollection<Appointment>(
                    new List<Appointment>(appointments));
            }
        }

        public virtual Appointment AddAppointment(Doctor doctor = null)
        {
            var a = new Appointment(this, doctor ?? LeadDoctor);
            appointments.Add(a);
            return a;
        }

        public Course(Patient patient, Doctor doctor)
        {
            Contract.Requires(patient != null);
            Contract.Requires(doctor != null);

            Patient = patient;
            LeadDoctor = doctor;
            Start = DateTime.Today;
        }

        protected Course() { }
    }
}
