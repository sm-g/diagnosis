using Diagnosis.Common;
using Diagnosis.Common.Types;
using Diagnosis.Common.Util;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public enum DateUnit
    {
        Day,
        Week,
        Month,
        Year
    }
    [Serializable]
    public class DateOffset : NotifyPropertyChangedBase, IComparable, IDomainObject
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(DateOffset));
        private DateTime _now = DateTime.Today;
        private int? _offset;
        private DateUnit _unit;
        private int? _year;
        private int? _month;
        private int? _day;
        private bool _dateCut;
        private ReentrantFlag inSetting = new ReentrantFlag();

        public DateOffset(int? year, int? month, int? day, Func<DateTime> now = null)
        {
            if (now != null)
                Now = now();

            CutsDate = true;
            SetDate(year, month, day);
        }
        public DateOffset(DateTime dt, Func<DateTime> now = null)
            : this(dt.Year, dt.Month, dt.Day, now)
        {
        }

        public DateOffset(int? offset, DateUnit unit, Func<DateTime> now = null)
            : this()
        {
            // для тестов
            if (now != null)
                Now = now();

            SetOffset(offset, unit, true);
        }

        protected DateOffset() { }
        /// <summary>
        /// Момент, с которого считается смещение.
        /// </summary>
        public virtual DateTime Now
        {
            get { return _now; }
            set
            {
                if (_now != value)
                {
                    _now = value;
                    SetDate(Year, Month, Day);
                }
            }
        }

        public virtual int? Year
        {
            get
            {
                return _year;
            }
            set
            {
                Contract.Ensures(_year == value);
                if (_year != value)
                {
                    //   logger.DebugFormat("{0}, set year = {1}", this, value);
                    SetDate(value, Month, Day);
                    _year = value;
                    OnPropertyChanged("Year");
                    OnPropertyChanged("IsEmpty");
                }
            }
        }

        /// <summary>
        /// 1..12
        /// </summary>
        public virtual int? Month
        {
            get
            {
                return _month;
            }
            set
            {
                Contract.Ensures(_month == value);
                if (_month != value)
                {
                    //  logger.DebugFormat("{0}, set month = {1}", this, value);
                    SetDate(Year, value, Day);
                    _month = value;
                    OnPropertyChanged("Month");
                    OnPropertyChanged("IsEmpty");
                }
            }
        }

        /// <summary>
        /// 1..
        /// </summary>
        public virtual int? Day
        {
            get
            {
                return _day;
            }
            set
            {
                Contract.Ensures(_day == value);
                if (_day != value)
                {
                    //   logger.DebugFormat("{0}, set day = {1}", this, value);
                    SetDate(Year, Month, value);
                    _day = value;
                    OnPropertyChanged("Day");
                    OnPropertyChanged("IsEmpty");
                }
            }
        }

        /// <summary>
        /// Смещение относительно даты, возвращаемой Now.
        /// </summary>
        public virtual int? Offset
        {
            get
            {
                return _offset;
            }
            set
            {
                if (_offset != value)
                {
                    _offset = value;
                    //   logger.DebugFormat("{0}, set offset = {1}", this, value);
                    SetOffset(value, Unit, true);
                    OnPropertyChanged("Offset");
                }
            }
        }

        /// <summary>
        /// Единица измерения смещения. Устанавливается после Year, Month, Day.
        /// </summary>
        public virtual DateUnit Unit
        {
            get
            {
                return _unit;
            }
            set
            {
                if (_unit != value)
                {
                    _unit = value;
                    //    logger.DebugFormat("{0}, set unit = {1}", this, value);
                    try
                    {
                        SetOffset(Offset, value);
                    }
                    catch (Exception e)
                    {
                        logger.WarnFormat("{0}", e.Message);
                    }
                    OnPropertyChanged("Unit");
                }
            }
        }
        /// <summary>
        /// Пустая дата.
        /// </summary>
        public virtual bool IsEmpty
        {
            get
            {
                return Year == null && Month == null && Day == null;

            }
        }

        /// <summary>
        /// Убирать несущественные части даты.
        /// По умолчанию true.
        ///
        /// Unit = Month -> day = null.
        /// Month = null -> day = null.
        /// </summary>
        public virtual bool CutsDate
        {
            get
            {
                return _dateCut;
            }
            set
            {
                if (_dateCut != value)
                {
                    _dateCut = value;
                    OnPropertyChanged(() => CutsDate);
                }
            }
        }

        /// <summary>
        /// Задает смещение и единицу.
        /// </summary>
        /// <param name="forceSetDateByOffsetUnit">Установка даты при задании только смещения или создании объекта.</param>
        private void SetOffset(int? offset, DateUnit unit, bool forceSetDateByOffsetUnit = false)
        {
            if (inSetting.CanEnter)
            {
                using (inSetting.Enter())
                {
                    Offset = offset;
                    Unit = unit;

                    if (forceSetDateByOffsetUnit)
                    {
                        SetDateByOffsetUnit();
                        return;
                    }
                    SetDateByOffsetUnit();

                }
            }
        }

        /// <summary>
        /// Задает дату.
        /// </summary>
        private void SetDate(int? year, int? month, int? day)
        {
            if (inSetting.CanEnter)
            {
                using (inSetting.Enter())
                {
                    var yearWas = _year.HasValue;
                    var monthWas = _month.HasValue;

                    // нулевые значения допустимы
                    var y = year != 0 ? year : null;
                    var m = month != 0 ? month : null;
                    var d = day != 0 ? day : null;
                    _year = y;
                    _month = m;
                    _day = d;

                    // убираем остаток даты
                    if (CutsDate && yearWas)
                    {
                        if (y == null) // clear year
                        {
                            Month = null;
                            Day = null;
                        }
                        else if (monthWas && m == null) // clear month
                        {
                            Day = null;
                        }
                    }

                    // если нет года или года и месяца, считаем их из Now
                    // только год - не считаем месяц из Now
                    if (d != null && m == null && y == null)
                    {
                        Month = Now.Month; // set day
                        Year = Now.Year;
                    }
                    if (m != null && !monthWas && y == null)
                    {
                        Year = Now.Year; // set month
                    }

                    // не может быть день без месяца
                    if (CutsDate && _year != null && _month == null)
                    {
                        Day = null;
                    }

                    ValidateDate();

                    if (IsEmpty)
                    {
                        Offset = null;
                        return;
                    }

                    SetOffsetUnitByDateSilly(_year.HasValue);
                }
            }
        }

        /// <summary>
        /// При при установке смещения или единицы изменяются значения Day, Month, Year.
        /// Смещение в неделях задает дату с точностью в день.
        /// </summary>
        private void SetDateByOffsetUnit()
        {
            Contract.Ensures(Contract.OldValue(_offset) == Contract.ValueAtReturn(out _offset));
            Contract.Ensures(Contract.OldValue(_unit) == Contract.ValueAtReturn(out _unit));

            if (Offset == null)
            {
                Year = null;
                Month = null;
                Day = null;
                return;
            }
            switch (Unit)
            {
                case DateUnit.Day:
                    var date = Now.AddDays(-Offset.Value);
                    Year = date.Year;
                    Month = date.Month;
                    Day = date.Day;
                    break;

                case DateUnit.Week:
                    date = Now.AddDays(-Offset.Value * 7);
                    Year = date.Year;
                    Month = date.Month;
                    Day = date.Day;
                    break;

                case DateUnit.Month:
                    if (Offset < Now.Month)
                    {
                        Year = Now.Year;
                        Month = Now.Month - Offset;
                    }
                    else
                    {
                        var a = Offset - Now.Month;
                        Year = Now.Year - a / 12 - 1;
                        Month = 12 - a % 12;
                    }
                    if (CutsDate)
                    {
                        Day = null;
                    }
                    break;

                case DateUnit.Year:
                    Year = Now.Year - Offset;
                    if (CutsDate)
                    {
                        Month = null;
                        Day = null;
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException("Unit");
            }

            ValidateDate();
        }



        /// <summary>
        /// Установка даты меняет единицу измерения и смещение c максимальной точностью.
        /// </summary>
        private void SetOffsetUnitByDateSilly(bool yearWas)
        {
            Contract.Ensures(Contract.OldValue(_year) == Contract.ValueAtReturn(out _year));
            Contract.Ensures(Contract.OldValue(_month) == Contract.ValueAtReturn(out _month));
            Contract.Ensures(Contract.OldValue(_day) == Contract.ValueAtReturn(out _day));
            if (Year.HasValue)
            {
                if (Month.HasValue)
                {
                    if (Day.HasValue)
                    {
                        // y m d
                        Offset = (Now - (DateTime)this).Days;
                        Unit = DateUnit.Day;
                        // не ставим недели, чтобы не менять давность при изменении дня
                    }
                    else
                    {
                        // y m _
                        Offset = DateHelper.GetTotalMonthsBetween(Now, Year.Value, Month.Value);
                        Unit = DateUnit.Month;
                    }
                }
                else
                {
                    // y _ ? - день игнорируем
                    Offset = Now.Year - Year.Value;
                    Unit = DateUnit.Year;
                }
            }
            else if (!Month.HasValue)
            {
                // _ _ d
                Offset = Now.Day - Day.Value;
                Unit = DateUnit.Day;
            }
        }



        /// <summary>
        /// Возвращает DateTime представление для объекта DateOffset, отсутствующие значения заменены 1.
        /// </summary>
        public virtual DateTime GetSortingDate()
        {
            int year = Year ?? 1;
            int month = Month ?? 1;
            int day = Day ?? 1;
            return new DateTime(year, month, day);
        }

        /// <summary>
        /// Проверяет дату, опционально исправляет.
        /// </summary>
        /// <returns>true, если было исправление.</returns>
        private bool ValidateDate()
        {
            var m = Month;
            var d = Day;
            if (DateHelper.CheckAndCorrectDate(Year, ref m, ref d))
            {
                Month = m;
                Day = d;
                return true;
            }
            return false;
        }
        #region operators

        public static bool operator <(DateOffset do1, DateOffset do2)
        {
            return do1.CompareTo(do2) == -1;
        }

        public static bool operator >(DateOffset do1, DateOffset do2)
        {
            return do1.CompareTo(do2) == 1;
        }

        public static bool operator <=(DateOffset do1, DateOffset do2)
        {
            return !(do1 > do2);
        }

        public static bool operator >=(DateOffset do1, DateOffset do2)
        {
            return !(do1 < do2);
        }

        public static bool operator ==(DateOffset do1, DateOffset do2)
        {
            if (object.ReferenceEquals(do1, null) || object.ReferenceEquals(do2, null))
            {
                return object.ReferenceEquals(do1, do2);
            }
            // offset и unit могут совпадать с разными датами

            return do1.Year == do2.Year &&
                do1.Month == do2.Month &&
                do1.Day == do2.Day &&
                do1.Unit == do2.Unit;
        }

        public static bool operator !=(DateOffset do1, DateOffset do2)
        {
            return !(do1 == do2);
        }

        public static explicit operator DateTime(DateOffset d)
        {
            if (d.Day == null || d.Month == null || d.Year == null)
                throw new NotSupportedException();

            return new DateTime(d.Year.Value, d.Month.Value, d.Day.Value);
        }

        public static explicit operator DateOffset(DateTime d)
        {
            return new DateOffset(d);
        }

        #endregion operators

        public override string ToString()
        {
            return string.Format("{0} {1} {2}.{3}.{4}",
                Offset,
                Unit,
                Year ?? 0, Month ?? 0, Day ?? 0);
        }

        public override bool Equals(object obj)
        {
            var other = obj as DateOffset;
            if (other == null)
                return false;
            return this == other;
        }

        public override int GetHashCode()
        {
            // по значению
            unchecked
            {
                int hash = 17;
                if (Year != null)
                    hash = hash * 23 + Year.GetHashCode();
                if (Month != null)
                    hash = hash * 23 + Month.GetHashCode();
                if (Day != null)
                    hash = hash * 23 + Day.GetHashCode();
                hash = hash * 23 + Unit.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Сравниваем дату, а не точку отсчета или смещение.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual int CompareTo(object obj)
        {
            var other = obj as DateOffset;
            if (other == null)
                return 1;

            if (other == this)
                return 0;

            if (this.GetSortingDate() < other.GetSortingDate())
                return -1;
            return 1;
        }
    }
}
