using Diagnosis.Common;
using System.Linq;
using Iesi.Collections.Generic;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System;

namespace Diagnosis.Models
{
    public class HealthRecord : EntityBase<Guid>, IDomainObject
    {
        private Iesi.Collections.Generic.ISet<HrItem> hrItems = new HashedSet<HrItem>();
        private int? _year;
        private int? _month;
        private int? _day;
        private bool _isDeleted;
        private HrCategory _category;
        private DateOffset _dateOffset;
        private HealthRecordUnits _unit;

        public virtual event NotifyCollectionChangedEventHandler ItemsChanged;

        public virtual Patient Patient { get; protected set; }
        public virtual Course Course { get; protected set; }
        public virtual Appointment Appointment { get; protected set; }
        public virtual Doctor Doctor { get; protected set; }

        public virtual IHrsHolder Holder
        {
            get
            {
                return (IHrsHolder)Patient ?? (IHrsHolder)Course ?? Appointment;
            }
        }

        /// <summary>
        /// Указывает, что запись помечена на удаление.
        /// </summary>
        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
            set
            {
                if (_isDeleted == value)
                    return;
                EditHelper.Edit<bool, Guid>(() => IsDeleted);
                _isDeleted = value;
                OnPropertyChanged("IsDeleted");
            }
        }

        public virtual HrCategory Category
        {
            get { return _category; }
            set
            {
                if (_category == value)
                    return;

                EditHelper.Edit<HrCategory, Guid>(() => Category);
                _category = value;
                OnPropertyChanged("Category");
            }
        }

        public virtual int? FromDay
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

        public virtual int? FromMonth
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
        public virtual int Ord { get; set; }
        public virtual DateTime CreatedAt { get; protected set; }

        public virtual DateOffset DateOffset
        {
            get
            {
                if (_dateOffset == null)
                {
                    DateTime now = Appointment != null ? Appointment.DateAndTime : DateTime.Now; // TODO createdat for hr

                    _dateOffset = new DateOffset(FromYear, FromMonth, FromDay, () => now);
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
                                FromMonth = _dateOffset.Month;
                                break;

                            case "Day":
                                FromDay = _dateOffset.Day;
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


        public virtual Iesi.Collections.Generic.ISet<HrItem> HrItems
        {
            get { return hrItems; }
            protected internal set
            {
                hrItems = value;
                OnPropertyChanged("HrItems");
            }
        }
        public virtual IEnumerable<Measure> Measures
        {
            get { return hrItems.Where(x => x.Measure != null).Select(x => x.Measure); }
        }
        public virtual IEnumerable<Word> Words
        {
            get { return hrItems.Where(x => x.Word != null).Select(x => x.Word); }
        }

        public HealthRecord(Appointment appointment, Doctor author)
        {
            Contract.Requires(appointment != null);
            Contract.Requires(author != null);

            Appointment = appointment;
            Doctor = author;
        }
        public HealthRecord(Course course, Doctor author)
        {
            Contract.Requires(course != null);
            Contract.Requires(author != null);

            Course = course;
            Doctor = author;
        }
        public HealthRecord(Patient patient, Doctor author)
        {
            Contract.Requires(patient != null);
            Contract.Requires(author != null);

            Patient = patient;
            Doctor = author;

        }

        protected HealthRecord()
        {
        }

        public virtual void AddItem(HrItem item)
        {
            Contract.Requires(item != null);
            var hrItemsCopy = new HashedSet<HrItem>(hrItems);
            if (hrItems.Add(item))
            {
                EditHelper.Edit("HrItems", hrItemsCopy);
                if (InEdit)
                {
                    IsDirty = true;
                }
                OnItemsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
                // order устанавливается в редакторе записи
            }
        }

        public virtual void RemoveItem(HrItem item)
        {
            Contract.Requires(item != null);
            var hrItemsCopy = new HashedSet<HrItem>(hrItems);
            if (hrItems.Remove(item))
            {
                EditHelper.Edit("HrItems", hrItemsCopy);
                if (InEdit)
                {
                    IsDirty = true;
                }
                OnItemsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            }
        }

        public virtual IEnumerable<IHrItemObject> GetOrderedEntities()
        {
            return from item in HrItems
                   orderby item.Ord
                   select item.Entity;
        }

        public override string ToString()
        {
            return string.Format("hr {0} {1} {2}", Id, Category, DateOffset);
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