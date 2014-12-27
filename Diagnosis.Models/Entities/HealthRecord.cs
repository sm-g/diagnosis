using Diagnosis.Common;
using Iesi.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;

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
        private DateTime _createdAt;

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
                SetProperty(ref _isDeleted, value, () => IsDeleted);
            }
        }

        public virtual HrCategory Category
        {
            get { return _category; }
            set
            {
                if (value == HrCategory.Null) value = null;
                SetProperty(ref _category, value, () => Category);
            }
        }

        public virtual int? FromDay
        {
            get { return _day; }
            set
            {
                SetProperty(ref _day, value, () => FromDay);
            }
        }

        public virtual int? FromMonth
        {
            get { return _month; }
            set
            {
                SetProperty(ref _month, value, () => FromMonth);
            }
        }

        public virtual int? FromYear
        {
            get { return _year; }
            set
            {
                SetProperty(ref _year, value, () => FromYear);
            }
        }

        public virtual HealthRecordUnits Unit
        {
            get { return _unit; }
            set
            {
                SetProperty(ref _unit, value, () => Unit);
            }
        }

        public virtual int Ord { get; set; }

        public virtual DateTime CreatedAt
        {
            get { return _createdAt; }
            protected set
            {
                _createdAt = value;
                DateOffset.Now = value;
            }
        }

        public virtual DateOffset DateOffset
        {
            get
            {
                if (_dateOffset == null)
                {
                    _dateOffset = new DateOffset(FromYear, FromMonth, FromDay,
                        () => CreatedAt != DateTime.MinValue ? CreatedAt : DateTime.Now,
                        DateOffset.DateOffsetSettings.OnLoading());

                    if (Unit != HealthRecordUnits.NotSet &&
                        Unit != HealthRecordUnits.ByAge
                        || _dateOffset.DateSettingStrategy == DateOffset.DateSetting.SavesUnit)
                    {
                        // фиксируем единицу
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
                                if (_dateOffset.UnitFixed ||
                                    Unit != HealthRecordUnits.ByAge &&
                                    Unit != HealthRecordUnits.NotSet)
                                {
                                    Unit = _dateOffset.Unit.ToHealthRecordUnit();
                                }
                                break;
                        }
                        OnPropertyChanged(() => DateOffset);
                    };
                    this.PropertyChanged += (s, e) => // подписываемся в первую очередь
                    {
                        try
                        {
                            switch (e.PropertyName)
                            {
                                case "FromDay":
                                    DateOffset.Day = FromDay;
                                    break;

                                case "FromMonth":
                                    DateOffset.Month = FromMonth;
                                    break;

                                case "FromYear":
                                    DateOffset.Year = FromYear;
                                    break;

                                case "Unit":
                                    var doUnit = Unit.ToDateOffsetUnit();
                                    DateOffset.Unit = doUnit ?? DateOffset.Unit; // меняем Unit на конкретную часть даты
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
            : this()
        {
            Contract.Requires(appointment != null);
            Contract.Requires(author != null);

            Appointment = appointment;
            Doctor = author;
        }

        public HealthRecord(Course course, Doctor author)
            : this()
        {
            Contract.Requires(course != null);
            Contract.Requires(author != null);

            Course = course;
            Doctor = author;
        }

        public HealthRecord(Patient patient, Doctor author)
            : this()
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