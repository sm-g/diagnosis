using Diagnosis.Core;
using Iesi.Collections.Generic;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;

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
        private Symptom _symptom;
        private Category _category;
        private IcdDisease _disease;
        private DateOffset _dateOffset;
        private HealthRecordUnits _unit;

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

                EditHelper.Edit("FromDay", _day);
                _day = value;
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

                EditHelper.Edit("FromMonth", _month);
                _month = value;
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

                EditHelper.Edit("FromYear", _year);
                _year = value;

                OnPropertyChanged("FromYear");
            }
        }

        public virtual HealthRecordUnits Unit
        {
            get
            {
                return _unit;
            }
            set
            {
                if (_unit == value)
                    return;

                EditHelper.Edit("Unit", _unit);
                _unit = value;

                OnPropertyChanged("Unit");
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
                    this.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName.StartsWith("From"))
                            try
                            {
                                switch (e.PropertyName)
                                {
                                    case "FromDay":
                                        _dateOffset.Day = FromDay;
                                        break;

                                    case "FromMonth":
                                        _dateOffset.Month = FromMonth;
                                        break;

                                    case "FromYear":
                                        _dateOffset.Year = FromYear;
                                        break;
                                }
                            }
                            catch
                            {
                                // не меняем DateOffset, компоненты даты поменяются потом
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

        /// <summary>
        /// Порядок слов симптома при редактировании записи, начиная с 0. В симптоме max 9 слов.
        /// </summary>
        public virtual string WordsOrder { get; set; }

        /// <summary>
        /// Последовательность количества подряд идущих слов и измерений при редактировании записи, начиная с количства слов.
        /// 11 - слово и измерение,
        /// 012 - измерение и два слова
        /// </summary>
        public virtual string WordsMeasuresSequence { get; set; }

        public HealthRecord(Appointment appointment)
        {
            Contract.Requires(appointment != null);

            Appointment = appointment;
            WordsOrder = "";
            WordsMeasuresSequence = "";
        }

        public virtual void AddMeasure(Measure m)
        {
            Contract.Requires(m != null);

            if (measures.Add(m))
            {
                OnMeasuresChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, m));
                // measure.order устанавливается перед добавлением в редакторе записи
            }
        }

        public virtual void RemoveMeasure(Measure m)
        {
            Contract.Requires(m != null);

            if (measures.Remove(m))
            {
                OnMeasuresChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, m));
                // удаление не влияет на порядок других измерений
            }
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