using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using Iesi.Collections.Generic;
using FluentValidation.Results;
using Diagnosis.Models.Validators;

namespace Diagnosis.Models
{
    public class Appointment : ValidatableEntity<Guid>, IDomainObject, IHrsHolder, IComparable<Appointment>
    {
        Iesi.Collections.Generic.ISet<HealthRecord> healthRecords = new HashedSet<HealthRecord>();
        private DateTime _dateTime;

        public virtual event NotifyCollectionChangedEventHandler HealthRecordsChanged;

        public virtual Course Course { get; protected set; }
        public virtual Doctor Doctor { get; set; }
        public virtual DateTime DateAndTime
        {
            get { return _dateTime; }
            set { SetProperty(ref _dateTime, value, "DateAndTime"); }
        }
        public virtual IEnumerable<HealthRecord> HealthRecords
        {
            get { return healthRecords; }
        }

        public Appointment(Course course, Doctor doctor)
        {
            Contract.Requires(course != null);
            Contract.Requires(doctor != null);

            Course = course;
            Doctor = doctor;
            DateAndTime = DateTime.Now;
        }

        protected Appointment() { }

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

        public override string ToString()
        {
            return string.Format("app {0:d} {1}", DateAndTime, Doctor);
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
    }
}
