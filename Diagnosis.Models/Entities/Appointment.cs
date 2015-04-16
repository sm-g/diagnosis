using Diagnosis.Common;
using Diagnosis.Models.Validators;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    public class Appointment : ValidatableEntity<Guid>, IDomainObject, IHaveAuditInformation, IHrsHolder, IComparable<Appointment>
    {
        private ISet<HealthRecord> healthRecords = new HashSet<HealthRecord>();
        private DateTime _dateTime;
        private DateTime _updatedAt;
        private DateTime _createdAt;

        public Appointment(Course course, Doctor doctor)
            : this()
        {
            Contract.Requires(course != null);
            Contract.Requires(doctor != null);

            Course = course;
            Doctor = doctor;
            DateAndTime = DateTime.Now;
        }

        protected internal Appointment()
        {
            _createdAt = DateTime.Now;
            _updatedAt = DateTime.Now;
        }

        public virtual event NotifyCollectionChangedEventHandler HealthRecordsChanged;

        public virtual Course Course { get; protected internal set; }

        public virtual Doctor Doctor { get; set; }

        public virtual DateTime DateAndTime
        {
            get { return _dateTime; }
            set { SetProperty(ref _dateTime, value, "DateAndTime"); }
        }

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

        public virtual IEnumerable<HealthRecord> HealthRecords
        {
            get { return healthRecords.OrderBy(x => x.Ord); }
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
        /// Меняет дату так, чтобы осмотр входил в интервал курса.
        /// </summary>
        public virtual void FitDateToCourse()
        {
            Contract.Ensures(
                Course.Start <= DateAndTime && Course.IsEnded ?
                Course.Start > Course.End || Course.End >= DateAndTime.Date : true); // курс может быть в некорректном состоянии (начало>конец)
            Contract.Ensures(DateAndTime.TimeOfDay == Contract.OldValue(DateAndTime).TimeOfDay);

            if (Course.Start > DateAndTime)
                DateAndTime = Course.Start.Add(DateAndTime.TimeOfDay);
            else if (Course.IsEnded && Course.End < DateAndTime)
                DateAndTime = Course.End.Value.Add(DateAndTime.TimeOfDay);

            Course.ResetValidationCache();
        }
        public override string ToString()
        {
            return string.Format("app {0:d} {1}", DateAndTime, Doctor);
        }

        public override ValidationResult SelfValidate()
        {
            return new AppointmentValidator().Validate(this);
        }

        public virtual int CompareTo(IHrsHolder h)
        {
            var app = h as Appointment;
            if (app != null)
                return this.CompareTo(app);

            return -1;
        }

        public virtual int CompareTo(Appointment other)
        {
            if (this.Course != other.Course)
                return this.Course.CompareTo(other.Course);

            return this.DateAndTime.CompareTo(other.DateAndTime);
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
}