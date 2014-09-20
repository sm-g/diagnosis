using Diagnosis.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using Iesi.Collections.Generic;
using System.Collections.Specialized;

namespace Diagnosis.Models
{
    public class HealthRecord : EntityBase, IDomainEntity
    {
        private Iesi.Collections.Generic.ISet<PatientRecordProperty> recordProperties;
        private Iesi.Collections.Generic.ISet<Measure> measures = new HashedSet<Measure>();
        private int? _year;
        private byte? _month;
        private byte? _day;
        private string _comment;
        private decimal? _numVal;
        private Symptom _symptom;
        private Category _category;
        private IcdDisease _disease;
        private DateOffset _dateOffset;

        public virtual event NotifyCollectionChangedEventHandler MeasuresChanged;

        public virtual Appointment Appointment { get; protected set; }

        public virtual string Comment
        {
            get { return _comment; }
            set
            {
                if (_comment == value)
                    return;
                EditHelper.Edit(() => Comment);
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

                EditHelper.Edit(() => Category);
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
                EditHelper.Edit(() => Disease);
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

                EditHelper.Edit(() => Symptom);
                _symptom = value;
                OnPropertyChanged("Symptom");
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
                if (value == null || (value >= 0 && value <= 31))
                {
                    EditHelper.Edit("FromDay", _day);
                    _day = value > 0 ? value : null;
                    DateOffset.Day = value;
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
                if (value == null || (value >= 0 && value <= 12))
                {
                    EditHelper.Edit("FromMonth", _month);
                    _month = value > 0 ? value : null;
                    DateOffset.Month = value;
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
                if (value == null || (value <= DateTime.Today.Year))
                {
                    EditHelper.Edit("FromYear", _year);
                    _year = value;
                    DateOffset.Year = value;
                }

                CheckDate();
                OnPropertyChanged("FromYear");
            }
        }

        public virtual DateOffset DateOffset
        {
            get
            {
                if (_dateOffset == null)
                {
                    _dateOffset = new DateOffset(FromYear, FromMonth, FromDay, () => Appointment.DateAndTime);
                    _dateOffset.PropertyChanged += (s, e) =>
                    {
                        switch (e.PropertyName)
                        {
                            case "Year":
                                FromYear = _dateOffset.Year;
                                break;

                            case "Month":
                                FromMonth = (byte?)_dateOffset.Month;
                                break;

                            case "Day":
                                FromDay = (byte?)_dateOffset.Day;
                                break;
                        }
                    };
                }
                return _dateOffset;
            }
        }

        public virtual IEnumerable<PatientRecordProperty> RecordProperties
        {
            get { return recordProperties; }
        }
        public virtual IEnumerable<Measure> Measures
        {
            get { return measures; }
        }

        public HealthRecord(Appointment appointment)
        {
            Contract.Requires(appointment != null);

            Appointment = appointment;
        }

        public virtual void AddMeasure(Measure m)
        {
            Contract.Requires(m != null);

            if (measures.Add(m))
                OnMeasuresChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, m));
        }
        public virtual void RemoveMeasure(Measure m)
        {
            Contract.Requires(m != null);

            if (measures.Remove(m))
                OnMeasuresChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, m));
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
            return string.Format("{0} {1} {2} {3} {4} {5}", Id, Category, Symptom, Disease != null ? Disease.Title : "",
                new DateOffset(FromYear, FromMonth, FromDay, () => Appointment.DateAndTime), Comment);
        }

        protected virtual void OnMeasuresChanged(NotifyCollectionChangedEventArgs e)
        {
            var h = MeasuresChanged;
            if (h != null)
            {
                h(this, e);
            }
        }
    }
}