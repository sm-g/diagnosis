using Diagnosis.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class HealthRecord : EntityBase
    {
        private ISet<PatientRecordProperty> recordProperties = new HashSet<PatientRecordProperty>();
        private int? _year;
        private byte? _month;
        private byte? _day;
        private string _comment;
        private decimal? _numVal;
        private Symptom _symptom;
        private Category _category;
        private IcdDisease _disease;
        
        public virtual Appointment Appointment { get; protected set; }

        public virtual string Comment
        {
            get { return _comment; }
            set
            {
                if (_comment == value)
                    return;
                _comment = value.TrimedOrNull();
                OnPropertyChanged("Comment");
            }
        }

        public virtual Category Category
        {
            get { return _category; }
            set
            {
                if (_category == value)
                    return;
                _category = value;
                OnPropertyChanged("Category");
            }
        }

        public virtual IcdDisease Disease
        {
            get { return _disease; }
            set
            {
                if (_disease == value)
                    return;
                _disease = value;
                OnPropertyChanged("Disease");
            }
        }

        public virtual Symptom Symptom
        {
            get { return _symptom; }
            set
            {
                if (_symptom == value)
                    return;
                _symptom = value;
                OnPropertyChanged("Symptom");
            }
        }

        public virtual decimal? NumValue
        {
            get { return _numVal; }
            set
            {
                if (_numVal == value)
                    return;
                _numVal = value;
                OnPropertyChanged("NumValue");
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
                if (_day == value)
                    return;
                if (value == null)
                {
                    _day = value;
                }
                if (value >= 0 && value <= 31)
                {
                    _day = value > 0 ? value : null;
                }
                CheckDate();
                OnPropertyChanged("FromDay");
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
                if (_month == value)
                    return;
                if (value == null)
                {
                    _month = value;
                }
                if (value >= 0 && value <= 12)
                {
                    _month = value > 0 ? value : null;
                }
                CheckDate();
                OnPropertyChanged("FromMonth");
            }
        }

        public virtual int? FromYear
        {
            get
            {
                return _year;
            }
            set
            {
                if (_year == value)
                    return;
                if (value == null)
                {
                    _year = value;
                }
                if (value <= DateTime.Today.Year)
                {
                    _year = value;
                }
                CheckDate();
                OnPropertyChanged("FromYear");
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
            DateHelper.CheckDate(FromYear, FromMonth, FromDay);
        }

        protected HealthRecord()
        {
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} {4} {5} {6}", Id, Category, Symptom, NumValue != null ? NumValue.Value.ToString("G6") : "", Disease != null ? Disease.Title : "",
                new DateOffset(FromYear, FromMonth, FromDay, () => Appointment.DateAndTime), Comment);
        }
    }
}