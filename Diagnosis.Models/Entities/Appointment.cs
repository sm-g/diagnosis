﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace Diagnosis.Models
{
    public class Appointment : EntityBase, IDomainEntity
    {
        ISet<HealthRecord> healthRecords = new HashSet<HealthRecord>();

        public virtual event NotifyCollectionChangedEventHandler HealthRecordsChanged;

        public virtual Course Course { get; protected set; }
        public virtual Doctor Doctor { get; set; }
        public virtual DateTime DateAndTime { get; set; }
        public virtual IEnumerable<HealthRecord> HealthRecords
        {
            get
            {
                return healthRecords;
            }
        }

        public Appointment(Course course, Doctor doctor)
        {
            Contract.Requires(course != null);
            Contract.Requires(doctor != null);

            Course = course;
            Doctor = doctor;
            DateAndTime = DateTime.UtcNow;
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

        protected Appointment() { }

        public override string ToString()
        {
            return string.Format("{0:d} {1}", DateAndTime, Doctor);
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
