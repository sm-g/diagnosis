using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class Course : IEntity
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doctor">Доктор, ведущий прием.</param>
        /// <returns></returns>
        public virtual Appointment AddAppointment(Doctor doctor)
        {
            Contract.Requires(!End.HasValue);

            var a = new Appointment(this, doctor ?? LeadDoctor);
            appointments.Add(a);
            return a;
        }
        public virtual void DeleteAppointment(Appointment app)
        {
            appointments.Remove(app);
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

        public override string ToString()
        {
            return string.Format("{0:d}, {1} apps {2} {3}", Start, Appointments.Count, Patient, LeadDoctor);
        }
    }

    public class CompareCourseByDate : IComparer<Course>
    {
        public int Compare(Course x, Course y)
        {
            if (x.End.HasValue && y.End.HasValue)
            {
                // оба курса закончились — больше тот, что кончился позже
                return x.End.Value.CompareTo(y.End.Value);
            }
            else
            {
                // больше тот, что продолжается
                if (x.End.HasValue)
                    return -1;
                if (y.End.HasValue)
                    return 1;

                // оба продолжаются — больше тот, что начался позже
                return x.Start.CompareTo(y.Start);
            }
        }

    }
}
