using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using Iesi.Collections.Generic;

namespace Diagnosis.Models
{
    public class Course : EntityBase, IDomainEntity, IHrsHolder
    {
        Iesi.Collections.Generic.ISet<Appointment> appointments = new HashedSet<Appointment>();
        Iesi.Collections.Generic.ISet<HealthRecord> healthRecords = new HashedSet<HealthRecord>();

        public virtual event NotifyCollectionChangedEventHandler HealthRecordsChanged;

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
        public virtual IEnumerable<HealthRecord> HealthRecords
        {
            get { return healthRecords; }
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doctor">Доктор, ведущий осмотр. Если null, осмотр ведет доктор курса.</param>
        /// <returns></returns>
        public virtual Appointment AddAppointment(Doctor doctor)
        {
            Contract.Requires(!End.HasValue);
            var a = new Appointment(this, doctor ?? LeadDoctor);
            appointments.Add(a);

            OnAppointmentsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, a));
            return a;
        }
        public virtual void RemoveAppointment(Appointment app)
        {
            Contract.Requires(app.IsEmpty());
            if (appointments.Remove(app))
                OnAppointmentsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, app));

        }

        public virtual HealthRecord AddHealthRecord()
        {
            var hr = new HealthRecord(this);
            healthRecords.Add(hr);
            OnHealthRecordsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, hr));

            return hr;
        }

        public virtual void RemoveHealthRecord(HealthRecord hr)
        {
            if (healthRecords.Remove(hr))
                OnHealthRecordsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, hr));
        }

        public override string ToString()
        {
            return string.Format("course {0:d}, {1} {2}", Start, Patient, LeadDoctor);
        }

        protected virtual void OnAppointmentsChanged(NotifyCollectionChangedEventArgs e)
        {
            var h = AppointmentsChanged;
            if (h != null)
            {
                h(this, e);
            }
        }
        protected virtual void OnHealthRecordsChanged(NotifyCollectionChangedEventArgs e)
        {
            var h = HealthRecordsChanged;
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
