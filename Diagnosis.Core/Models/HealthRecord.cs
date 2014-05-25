using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using Diagnosis.Core;

namespace Diagnosis.Models
{
    public class HealthRecord
    {
        private ISet<PatientRecordProperty> recordProperties = new HashSet<PatientRecordProperty>();
        private int? _year;
        private byte? _month;
        private byte? _day;

        public virtual int Id { get; protected set; }

        public virtual Appointment Appointment { get; protected set; }

        public virtual string Comment { get; set; }

        public virtual Category Category { get; set; }

        public virtual IcdDisease Disease { get; set; }

        public virtual Symptom Symptom { get; set; }

        public virtual decimal? NumValue { get; set; }

        public virtual int? FromYear
        {
            get
            {
                return _year;
            }
            set
            {
                if (value == null)
                {
                    _year = value;
                }
                if (value <= DateTime.Today.Year)
                {
                    _year = value;
                }
                CheckDate();
            }
        }

        public virtual byte? FromMonth
        {
            get
            {
                return _month;
            }
            set
            {
                if (value == null)
                {
                    _month = value;
                }
                if (value >= 0 && value <= 12)
                {
                    _month = value > 0 ? value : null;
                }
                CheckDate();
            }
        }

        public virtual byte? FromDay
        {
            get
            {
                return _day;
            }
            set
            {
                if (value == null)
                {
                    _day = value;
                }
                if (value >= 0 && value <= 31)
                {
                    _day = value > 0 ? value : null;
                }
                CheckDate();
            }
        }

        public virtual DateOffset DateOffset
        {
            get
            {
                return new DateOffset(FromYear, FromMonth, FromDay, () => Appointment.DateAndTime);
            }
        }

        public virtual ReadOnlyCollection<PatientRecordProperty> RecordProperties
        {
            get
            {
                return new ReadOnlyCollection<PatientRecordProperty>(
                    new List<PatientRecordProperty>(recordProperties));
            }
        }

        public HealthRecord(Appointment appointment)
        {
            Contract.Requires(appointment != null);

            Appointment = appointment;
        }

        private void CheckDate()
        {
            Checkers.CheckDate(FromYear, FromMonth, FromDay);
        }

        protected HealthRecord()
        {
        }
    }
}