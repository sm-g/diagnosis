using Diagnosis.Models.Validators;
using Diagnosis.Common;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    public class Course : ValidatableEntity<Guid>, IDomainObject, IHaveAuditInformation, IHrsHolder, IComparable<Course>
    {
        private ISet<Appointment> appointments = new HashSet<Appointment>();
        private ISet<HealthRecord> healthRecords = new HashSet<HealthRecord>();

        private DateTime _start;
        private DateTime? _end;
        private DateTime _updatedAt;
        private DateTime _createdAt;

        public Course(Patient patient, Doctor doctor)
            : this()
        {
            Contract.Requires(patient != null);
            Contract.Requires(doctor != null);

            Patient = patient;
            LeadDoctor = doctor;

            Start = DateTime.Today;
        }

        protected internal Course()
        {
            _createdAt = DateTime.Now;
            _updatedAt = DateTime.Now;
        }

        public virtual event NotifyCollectionChangedEventHandler HealthRecordsChanged;

        public virtual event NotifyCollectionChangedEventHandler AppointmentsChanged;

        public virtual Patient Patient { get; protected internal set; }

        public virtual Doctor LeadDoctor { get; protected internal set; }

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

        public virtual DateTime CreatedAt
        {
            get { return _createdAt; }
        }

        DateTime IHaveAuditInformation.CreatedAt
        {
            get { return _updatedAt; }
            set
            {
                _createdAt = value;
            }
        }

        DateTime IHaveAuditInformation.UpdatedAt
        {
            get { return _updatedAt; }
            set { SetProperty(ref _updatedAt, value, () => UpdatedAt); }
        }

        public virtual DateTime UpdatedAt
        {
            get { return _updatedAt; }
        }

        public virtual IEnumerable<Appointment> Appointments
        {
            get { return appointments; }
        }

        public virtual IEnumerable<HealthRecord> HealthRecords
        {
            get { return healthRecords.OrderBy(x => x.Ord); }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="doctor">Доктор, ведущий осмотр. Если null, осмотр ведет доктор курса.</param>
        /// <returns></returns>
        public virtual Appointment AddAppointment(Doctor doctor)
        {
            Contract.Ensures(Contract.Result<Appointment>().Course.Equals(this));

            var a = new Appointment(this, doctor ?? LeadDoctor);
            appointments.Add(a);
            a.Doctor.AddApp(a);

            a.FitDateToCourse();

            OnAppointmentsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, a));
            return a;
        }

        protected internal virtual void AddAppointment(Appointment app)
        {
            Contract.Requires(app.Course == null);

            if (appointments.Add(app))
            {
                app.Course = this;
                app.FitDateToCourse();

                if (app.Doctor == null)
                    app.Doctor = this.LeadDoctor;

                OnAppointmentsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, app));
            }
        }
        public virtual void RemoveAppointment(Appointment app)
        {
            Contract.Requires(app.IsEmpty());
            if (appointments.Remove(app))
            {
                app.Doctor.RemoveApp(app);
                OnAppointmentsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, app));
            }
        }

        public virtual HealthRecord AddHealthRecord(Doctor author)
        {
            var hr = new HealthRecord(this, author);
            healthRecords.Add(hr);
            author.AddHr(hr);
            OnHealthRecordsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, hr));

            return hr;
        }

        public virtual void RemoveHealthRecord(HealthRecord hr)
        {
            if (healthRecords.Remove(hr))
            {
                hr.OnDelete();
                OnHealthRecordsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, hr));
            }
        }

        /// <summary>
        /// Завершает курс с датой сегодня или датой последнего осмотра, если он после сегодня.
        /// </summary>
        public virtual void Finish()
        {
            Contract.Requires(!IsEnded);
            Contract.Ensures(IsEnded);
            Contract.Ensures(appointments.Count == 0 && End == DateTime.Today ||
                End >= GetOrderedAppointments().Last().DateAndTime.Date);

            var last = Appointments.OrderBy(x => x.DateAndTime).LastOrDefault();
            var end = last != null ? last.DateAndTime : DateTime.Now;
            End = end > DateTime.Now ? end : DateTime.Now;
        }

        public virtual void Open()
        {
            End = null;
        }

        /// <summary>
        /// Меняет начало-конец так, чтобы осмотры входили в этот интервал.
        /// </summary>
        public virtual void FitDatesToApps()
        {
            Contract.Ensures(appointments.Count == 0 ||
                Start <= GetOrderedAppointments().First().DateAndTime && IsEnded ?
                End >= GetOrderedAppointments().Last().DateAndTime.Date : true);

            var last = GetOrderedAppointments().LastOrDefault();
            var first = GetOrderedAppointments().FirstOrDefault();

            if (End.HasValue && last != null && last.DateAndTime > End.Value)
                End = last.DateAndTime;
            if (first != null && first.DateAndTime < Start)
                Start = first.DateAndTime;

            appointments.ForAll(x => x.ResetValidationCache());
        }

        /// <summary>
        /// Осмотры, отсортированные по дате. Первый — самый ранний осмотр.
        /// </summary>
        [Pure]
        public virtual IEnumerable<Appointment> GetOrderedAppointments()
        {
            Contract.Ensures(Contract.Result<IEnumerable<Appointment>>().IsOrdered(x => x.DateAndTime));

            return Appointments.OrderBy(a => a);
        }

        public override string ToString()
        {
            return string.Format("course {0:d}, {1} {2}", Start, Patient, LeadDoctor);
        }

        public override ValidationResult SelfValidate()
        {
            return new CourseValidator().Validate(this);
        }

        public virtual int CompareTo(IHrsHolder h)
        {
            var app = h as Appointment;
            if (app != null)
                return 1;

            var course = h as Course;
            if (course != null)
                return this.CompareTo(course);

            return -1; // h is Patient
        }

        public virtual int CompareTo(Course other)
        {
            if (this.Patient != other.Patient)
                return this.Patient.CompareTo(other.Patient);

            return new CourseEarlierFirst().Compare(this, other);
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

    internal class CourseEarlierFirst : Comparer<Course>
    {
        public override int Compare(Course x, Course y)
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