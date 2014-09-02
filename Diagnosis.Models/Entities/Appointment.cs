using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public class Appointment : EntityBase
    {
        ISet<HealthRecord> healthRecords = new HashSet<HealthRecord>();
        ObservableCollection<HealthRecord> _healthRecords;

        public virtual Course Course { get; protected set; }
        public virtual Doctor Doctor { get; set; }
        public virtual DateTime DateAndTime { get; set; }
        public virtual ObservableCollection<HealthRecord> HealthRecords
        {
            get
            {
                return _healthRecords ?? (_healthRecords = new ObservableCollection<HealthRecord>(healthRecords));
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

        protected Appointment() { }

        public override string ToString()
        {
            return string.Format("{0:d} {1} hrs {2}", DateAndTime, HealthRecords.Count, Doctor);
        }
    }
}
