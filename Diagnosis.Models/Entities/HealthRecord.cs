using Diagnosis.Core;
using System.Linq;
using Iesi.Collections.Generic;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class HealthRecord : EntityBase, IDomainEntity
    {
        private Iesi.Collections.Generic.ISet<HrItem> hrItems = new HashedSet<HrItem>();
        private int? _year;
        private byte? _month;
        private byte? _day;
        private string _comment;
        private HrCategory _category;
        private DateOffset _dateOffset;
        private HealthRecordUnits _unit;

        public virtual event NotifyCollectionChangedEventHandler ItemsChanged;

        public virtual Patient Patient { get; protected set; }
        public virtual Course Course { get; protected set; }
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

        public virtual HrCategory Category
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
                    _dateOffset.Settings = new DateOffset.DateOffsetSettings(DateOffset.UnitSetting.RoundsOffset, DateOffset.DateSetting.SavesUnit);
                    if (Unit != HealthRecordUnits.NotSet) // фиксируем единицу
                    {
                        _dateOffset.Unit = Unit.ToDateOffsetUnit().Value;
                        _dateOffset.UnitFixed = true;
                    }

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

                            case "Unit":
                                if (_dateOffset.UnitFixed)
                                    Unit = _dateOffset.Unit.ToHealthRecordUnit();
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


        public virtual IEnumerable<HrItem> HrItems
        {
            get { return hrItems; }
        }
        public virtual IEnumerable<Measure> Measures
        {
            get { return hrItems.Where(x => x.Entity is Measure).Cast<Measure>(); }
        }
        public virtual IEnumerable<Word> Words
        {
            get { return hrItems.Where(x => x.Entity is Word).Cast<Word>(); }
        }

        public HealthRecord(Appointment appointment)
        {
            Contract.Requires(appointment != null);

            Appointment = appointment;
        }
        public HealthRecord(Course course)
        {
            Contract.Requires(course != null);

            Course = course;
        }
        public HealthRecord(Patient patient)
        {
            Contract.Requires(patient != null);

            Patient = patient;
        }

        protected HealthRecord()
        {
        }

        public virtual void AddItem(HrItem item)
        {
            Contract.Requires(item != null);
            EditHelper.Edit(() => HrItems);
            if (hrItems.Add(item))
            {
                OnItemsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
                // order устанавливается перед добавлением в редакторе записи
            }
        }

        public virtual void RemoveItem(HrItem item)
        {
            Contract.Requires(item != null);

            if (hrItems.Remove(item))
            {
                OnItemsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                // удаление не влияет на порядок других элементов
            }
        }
        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3}", Id, Category,
                new DateOffset(FromYear, FromMonth, FromDay, () => Appointment.DateAndTime), Comment);
        }

        protected virtual void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            var h = ItemsChanged;
            if (h != null)
            {
                h(this, e);
            }
        }
    }
}