using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using System;

namespace Diagnosis.Models
{
    public class HealthRecord
    {
        ISet<PatientRecordProperty> recordProperties = new HashSet<PatientRecordProperty>();
        int? _year;
        byte? _month;
        byte? _day;

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
                if (value <= DateTime.Today.Year)
                {
                    _year = value;
                    CheckDate();
                }
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
                if (value >= 0 && value <= 12)
                {
                    _month = value > 0 ? value : null;
                    CheckDate();

                }
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
                if (value >= 0 && value <= 31)
                {
                    _day = value > 0 ? value : null;
                    CheckDate();
                }
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

        void CheckDate()
        {
            Checkers.CheckDate(FromYear, FromMonth, FromDay);
        }

        protected HealthRecord() { }
    }
}
