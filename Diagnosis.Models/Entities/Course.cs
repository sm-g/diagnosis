using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using Iesi.Collections.Generic;

namespace Diagnosis.Models
{
    public class Course : EntityBase, IDomainEntity
    {
        Iesi.Collections.Generic.ISet<Appointment> appointments = new HashedSet<Appointment>();

        public virtual event NotifyCollectionChangedEventHandler AppointmentsChanged;

        public virtual Patient Patient { get; protected set; }
        public virtual Doctor LeadDoctor { get; protected set; }
        public virtual DateTime Start { get; protected set; }
        public virtual DateTime? End { get; set; }

        public virtual IEnumerable<Appointment> Appointments
        {
            get
            {
                return appointments;
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

            OnAppointmentsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, a));
            return a;
        }
        public virtual void DeleteAppointment(Appointment app)
        {
            if (appointments.Remove(app))
                OnAppointmentsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, app));

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
            return string.Format("{0:d}, {1} {2}", Start, Patient, LeadDoctor);
        }

        protected virtual void OnAppointmentsChanged(NotifyCollectionChangedEventArgs e)
        {
            var h = AppointmentsChanged;
            if (h != null)
            {
                h(this, e);
            }
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
