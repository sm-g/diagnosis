using System;
using System.Diagnostics.Contracts;

namespace Diagnosis.Core
{
    public enum DateUnits
    {
        Day,
        Week,
        Month,
        Year
    }

    /// <summary>
    /// Неполная дата со смещением относительно сегодня в днях, неделях, месяцах или годах, зависит от полноты даты.
    /// Заданная днём, месяцем и годом дата будет иметь смещение в неделях, если число дней нацело делится на 7.
    /// </summary>
    public class DateOffset : NotifyPropertyChangedBase
    {
        private static string[] days = new string[3] { "день", "дня", "дней" };
        private static string[] weeks = new string[3] { "неделя", "недели", "недель" };
        private static string[] months = new string[3] { "месяц", "месяца", "месяцев" };
        private static string[] years = new string[3] { "год", "года", "лет" };
        private int? _offset;
        private DateUnits _unit;
        private int? _year;
        private int? _month;
        private int? _day;

        private bool setting;

        readonly public Func<DateTime> NowDate = () => DateTime.Today;

        public int? Year
        {
            get
            {
                return _year;
            }
            set
            {
                if (_year != value)
                {
                    _year = value;
                    if (!setting)
                    {
                        SetDate(value, Month, Day);
                    }
                    OnPropertyChanged("Year");
                }
            }
        }

        public int? Month
        {
            get
            {
                return _month;
            }
            set
            {
                if (_month != value)
                {
                    _month = value;
                    if (!setting)
                    {
                        SetDate(Year, value, Day);
                    }
                    OnPropertyChanged("Month");
                }
            }
        }

        public int? Day
        {
            get
            {
                return _day;
            }
            set
            {
                if (_day != value)
                {
                    _day = value;
                    if (!setting)
                    {
                        SetDate(Year, Month, value);
                    }
                    OnPropertyChanged("Day");
                }
            }
        }

        /// <summary>
        /// Смещение относительно даты, возвращаемой NowDate.
        /// </summary>
        public int? Offset
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
                    if (!setting)
                    {
                        SetOffset(value, Unit);
                    }
                    OnPropertyChanged("Offset");
                    OnPropertyChanged("IsEmpty");
                }
            }
        }
        /// <summary>
        /// Единица измерения смещения.
        /// </summary>
        public DateUnits Unit
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
                    if (!setting)
                    {
                        SetOffset(Offset, value);
                    }
                    OnPropertyChanged("Unit");
                }
            }
        }

        /// <summary>
        /// Пустая дата, когда смещение не задано.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return Offset == null;
            }
        }

        /// <summary>
        /// Возвращает DateTime представление для объекта DateOffset, если возможно.
        /// </summary>
        public DateTime? DateTimeNullable
        {
            get
            {
                return DateHelper.NullableDate(Year, Month, Day);
            }
        }

        public string UnitString
        {
            get
            {
                return FormatUnit(Offset, Unit);
            }
        }

        private DateTime Now { get { return NowDate(); } }

        private void SetOffset(int? offset, DateUnits unit)
        {
            setting = true;

            Offset = offset;
            Unit = unit;

            if (offset == null)
            {
                Year = null;
                Month = null;
                Day = null;
            }
            else
            {
                switch (Unit)
                {
                    case DateUnits.Day:
                        var date = Now.AddDays(-Offset.Value);
                        Year = date.Year;
                        Month = date.Month;
                        Day = date.Day;
                        break;

                    case DateUnits.Week:
                        date = Now.AddDays(-Offset.Value * 7);
                        Year = date.Year;
                        Month = date.Month;
                        Day = date.Day;
                        break;

                    case DateUnits.Month:
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
                        Day = null;
                        break;

                    case DateUnits.Year:
                        Year = Now.Year - Offset;
                        Month = null;
                        Day = null;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("Unit");
                }
            }
            setting = false;
        }

        private void SetDate(int? year, int? month, int? day)
        {

            setting = true;
            Year = year != 0 ? year : null;
            Month = month != 0 ? month : null;
            Day = day != 0 ? day : null;

            DateHelper.CheckDate(Year, Month, Day);

            int y;
            if (Year.HasValue)
            {
                y = Year.Value;
            }
            else
            {
                y = Now.Year;
            }

            if (Month.HasValue)
            {
                if (Day.HasValue)
                {
                    Offset = (Now - new DateTime(y, Month.Value, Day.Value)).Days;
                    if (Offset % 7 == 0 && Math.Abs(Offset.Value) > 1)
                    {
                        Offset /= 7;
                        Unit = DateUnits.Week;
                    }
                    else
                    {
                        Unit = DateUnits.Day;
                    }
                }
                else
                {
                    Offset = (Now.Year - y) * 12 + Now.Month - Month.Value;
                    Unit = DateUnits.Month;
                }

                if (!Year.HasValue)
                {
                    Year = Now.Year;
                }
            }
            else
            {
                if (Year.HasValue)
                {
                    Offset = Now.Year - y;
                    Unit = DateUnits.Year;
                }
                else
                {
                    if (Day.HasValue)
                    {
                        Offset = 0;
                        Unit = DateUnits.Day;

                        Month = Now.Month;
                        Year = Now.Year;
                    }
                    else
                    {
                        // year == null && month == null && day == null
                        Offset = null;
                    }
                }
            }

            setting = false;
        }


        public DateOffset(int? year, int? month, int? day)
        {
            SetDate(year, month, day);
        }

        public DateOffset(int? year, int? month, int? day, Func<DateTime> now)
        {
            Contract.Requires(now != null);
            NowDate = now;
            SetDate(year, month, day);
        }

        public DateOffset(int? offset, DateUnits unit)
        {
            SetOffset(offset, unit);
        }

        public DateOffset(int? offset, DateUnits unit, Func<DateTime> now)
        {
            Contract.Requires(now != null);
            NowDate = now;
            SetOffset(offset, unit);
        }

        public DateOffset(DateOffset dateOffset, Func<DateTime> now)
        {
            Contract.Requires(dateOffset != null);
            Contract.Requires(now != null);
            NowDate = now;
            SetOffset(dateOffset.Offset, dateOffset.Unit);
        }
        public static bool operator <(DateOffset do1, DateOffset do2)
        {
            if (do1.Unit == do2.Unit)
            {
                // давность больше - дата меньше
                return do1.Offset > do2.Offset;
            }
            if (!do1.Month.HasValue || !do2.Month.HasValue)
            {
                // месяц и год или день и год
                return do1.Year < do2.Year;
            }
            // день и месяц
            return do1.Month < do2.Month;
        }

        public static bool operator >(DateOffset do1, DateOffset do2)
        {
            if (do1.Unit == do2.Unit)
            {
                return do1.Offset < do2.Offset;
            }
            if (!do1.Month.HasValue || !do2.Month.HasValue)
            {
                return do1.Year > do2.Year;
            }
            return do1.Month > do2.Month;
        }

        public static bool operator <=(DateOffset do1, DateOffset do2)
        {
            if (do1.Unit == do2.Unit)
            {
                return do1.Offset >= do2.Offset;
            }
            if (!do1.Month.HasValue || !do2.Month.HasValue)
            {
                return do1.Year <= do2.Year;
            }
            return do1.Month <= do2.Month;
        }

        public static bool operator >=(DateOffset do1, DateOffset do2)
        {
            if (do1.Unit == do2.Unit)
            {
                return do1.Offset <= do2.Offset;
            }
            if (!do1.Month.HasValue || !do2.Month.HasValue)
            {
                return do1.Year >= do2.Year;
            }
            return do1.Month >= do2.Month;
        }

        public static string FormatUnit(int? offset, DateUnits unit)
        {
            if (offset == null)
                offset = 0;

            int ending = Plurals.GetPluralEnding(offset.Value);

            switch (unit)
            {
                case DateUnits.Day: return days[ending];
                case DateUnits.Week: return weeks[ending];
                case DateUnits.Month: return months[ending];
                case DateUnits.Year: return years[ending];
            }
            throw new ArgumentOutOfRangeException("unit");
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}.{3}.{4}", Offset, UnitString, Year ?? 0, Month ?? 0, Day ?? 0);
        }
    }
}