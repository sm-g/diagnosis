using Diagnosis.Models.Validators;
using FluentValidation.Results;
using Iesi.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class Course : ValidatableEntity<Guid>, IDomainObject, IHrsHolder
    {
        private Iesi.Collections.Generic.ISet<Appointment> appointments = new HashedSet<Appointment>();
        private Iesi.Collections.Generic.ISet<HealthRecord> healthRecords = new HashedSet<HealthRecord>();

        public virtual event NotifyCollectionChangedEventHandler HealthRecordsChanged;

        public virtual event NotifyCollectionChangedEventHandler AppointmentsChanged;

        private DateTime _start;
        private DateTime? _end;

        public virtual Patient Patient { get; protected set; }

        public virtual Doctor LeadDoctor { get; protected set; }

        public virtual DateTime Start
        {
            get
            {
                Contract.Ensures(Contract.Result<DateTime>().TimeOfDay == TimeSpan.Zero);
                return _start;
            }
            set { SetProperty(ref _start, value.Date, "Start"); }
        }

        public virtual DateTime? End
        {
            get
            {
                Contract.Ensures(Contract.Result<DateTime?>() == null || Contract.Result<DateTime?>().Value.TimeOfDay == TimeSpan.Zero);
                return _end;
            }
            set
            {
                if (SetProperty(ref _end, value.HasValue ? value.Value.Date : value, "End"))
                    OnPropertyChanged(() => IsEnded);
            }
        }

        public virtual bool IsEnded { get { return End.HasValue; } }

        public virtual IEnumerable<Appointment> Appointments
        {
            get { return appointments; }
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

        protected internal Course()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="doctor">Доктор, ведущий осмотр. Если null, осмотр ведет доктор курса.</param>
        /// <returns></returns>
        public virtual Appointment AddAppointment(Doctor doctor)
        {
            Contract.Requires(!IsEnded);
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

        public virtual HealthRecord AddHealthRecord(Doctor author)
        {
            var hr = new HealthRecord(this, author);
            healthRecords.Add(hr);
            OnHealthRecordsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, hr));

            return hr;
        }

        public virtual void RemoveHealthRecord(HealthRecord hr)
        {
            if (healthRecords.Remove(hr))
                OnHealthRecordsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, hr));
        }

        public virtual void Finish()
        {
            Contract.Requires(!IsEnded);
            End = DateTime.Now;
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

        public override ValidationResult SelfValidate()
        {
            return new CourseValidator().Validate(this);
        }
    }

    /// <summary>
    /// Use OrderBy to show first earlier cousrses.
    /// </summary>
    public class CompareCourseByDate : IComparer<Course>
    {
        public int Compare(Course x, Course y)
        {
            if (x.IsEnded && y.IsEnded)
            {
                // оба курса закончились — больше тот, что кончился позже
                // закончились в один день — больше тот, что начался позже 
                var end = x.End.Value.CompareTo(y.End.Value);
                if (end == 0)
                {
                    return x.Start.CompareTo(y.Start);
                }
                else
                {
                    return end;
                }
            }
            else
            {
                // больше тот, что продолжается
                if (x.IsEnded)
                    return -1;
                if (y.IsEnded)
                    return 1;

                // оба продолжаются — больше тот, что начался позже
                return x.Start.CompareTo(y.Start);
            }
        }
    }
}