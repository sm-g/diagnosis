﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public class Appointment
    {
        ISet<HealthRecord> healthRecords = new HashSet<HealthRecord>();

        public virtual int Id { get; protected set; }
        public virtual Course Course { get; protected set; }
        public virtual Doctor Doctor { get; set; }
        public virtual DateTime DateAndTime { get; set; }
        public virtual ReadOnlyCollection<HealthRecord> HealthRecords
        {
            get
            {
                return new ReadOnlyCollection<HealthRecord>(
                    new List<HealthRecord>(healthRecords));
            }
        }

        public virtual HealthRecord AddHealthRecord()
        {
            var hr = new HealthRecord(this);
            if (HealthRecords.Count > 0)
            {
                // копируем категории из последней записи
                hr.Category = HealthRecords.Last().Category;
            }
            healthRecords.Add(hr);
            return hr;
        }

        public Appointment(Course course, Doctor doctor)
        {
            Contract.Requires(course != null);
            Contract.Requires(doctor != null);

            Course = course;
            Doctor = doctor;
            DateAndTime = DateTime.UtcNow;
        }

        protected Appointment() { }
    }
}
