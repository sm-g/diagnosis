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
        private bool _autoCorrection;
        private bool _dateCut;

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
        /// Единица измерения смещения. Меняется день, меясяц, год.
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
        /// Обрезать лишние части даты при изменении единицы смещения. (unit = Month -> day = null)
        /// </summary>
        public bool DateCut
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
                    OnPropertyChanged(() => DateCut);
                }
            }
        }

        /// <summary>
        /// Исправлять число при изменении месяца, чтобы дата оставалась корректной.
        /// По умолчанию true.
        /// 31 dec. month = nov -> day == 30.
        /// 29 feb 2012. year = 2013 -> day == 28.
        /// </summary>
        public bool AutoCorrection
        {
            get
            {
                return _autoCorrection;
            }
            set
            {
                if (_autoCorrection != value)
                {
                    _autoCorrection = value;
                    OnPropertyChanged(() => AutoCorrection);
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

        public string UnitString
        {
            get
            {
                return FormatUnit(Offset, Unit);
            }
        }

        /// <summary>
        /// Добавляет указанное количество дней, недель (как 7 дней), месяцев или лет.
        /// Нельзя прибавить дни, если Unit - месяц или год и т.п.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="unit"></param>
        public void Add(int value, DateUnits unit)
        {
            if (unit.CompareTo(Unit) < 0 && !(unit == DateUnits.Day && Unit == DateUnits.Week))
                throw new ArgumentException("Can not add such part of date to current dateoffset.");

            DateTime dt;
            switch (unit)
            {
                case DateUnits.Day:
                    dt = new DateTime(Year.Value, Month.Value, Day.Value).AddDays(value);
                    SetDate(dt.Year, dt.Month, dt.Day);
                    break;

                case DateUnits.Week:
                    dt = new DateTime(Year.Value, Month.Value, Day.Value).AddDays(value * 7);
                    SetDate(dt.Year, dt.Month, dt.Day);
                    break;

                case DateUnits.Month:
                    dt = new DateTime(Year.Value, Month.Value, 1).AddMonths(value);
                    Month = dt.Month;
                    Year = dt.Year;
                    break;

                case DateUnits.Year:
                    dt = new DateTime(Year.Value, 1, 1).AddYears(value);
                    Year = dt.Year;
                    break;

                default:
                    break;
            }
        }

        private DateTime Now { get { return NowDate(); } }

        /// <summary>
        /// При задании смещения изменяются значения Day, Month, Year.
        /// Если DateCut, то несущественные части даты отбрасываются.
        /// Смещение в неделях задает дату с точностью в день.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="unit"></param>
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
                        if (DateCut)
                        {
                            Day = null;
                        }
                        break;

                    case DateUnits.Year:
                        Year = Now.Year - Offset;
                        if (DateCut)
                        {
                            Month = null;
                            Day = null;
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("Unit");
                }
            }
            setting = false;
        }


        private void SetDate(int? year, int? month, int? day)
        {
            // нулевые значения допустимы
            var y = year != 0 ? year : null;
            var m = month != 0 ? month : null;
            var d = day != 0 ? day : null;

            if (AutoCorrection)
            {
                DateHelper.CheckAndCorrectDate(y, ref m, ref d);
            }
            else
            {
                DateHelper.CheckDate(y, m, d);
            }

            setting = true;
            Year = y;
            Month = m;
            Day = d;

            if (Year == null && Month == null && Day == null)
            {
                Offset = null;
                setting = false;
                return;
            }
            bool yearWas = Year.HasValue;
            if (!Year.HasValue)
            {
                Year = Now.Year;
            }

            if (Month.HasValue)
            {
                if (Day.HasValue)
                {
                    // ? m d
                    Offset = (Now - new DateTime(Year.Value, Month.Value, Day.Value)).Days;
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
                    // ? m _
                    Offset = (Now.Year - Year.Value) * 12 + Now.Month - Month.Value;
                    Unit = DateUnits.Month;
                }
            }
            else
            {
                if (yearWas)
                {
                    // y _ ? - день игнорируем
                    Offset = Now.Year - Year.Value;
                    Unit = DateUnits.Year;
                }
                else
                {
                    // _ _ d
                    Offset = Now.Day - Day.Value;
                    Unit = DateUnits.Day;

                    Month = Now.Month;
                }
            }

            setting = false;
        }

        public DateOffset(int? year, int? month, int? day)
            : this()
        {
            SetDate(year, month, day);
        }

        public DateOffset(int? year, int? month, int? day, Func<DateTime> now)
            : this()
        {
            Contract.Requires(now != null);
            NowDate = now;
            SetDate(year, month, day);
        }

        public DateOffset(DateTime dt)
            : this()
        {
            SetDate(dt.Year, dt.Month, dt.Day);
        }

        public DateOffset(DateTime dt, Func<DateTime> now)
            : this()
        {
            Contract.Requires(now != null);
            NowDate = now;
            SetDate(dt.Year, dt.Month, dt.Day);
        }

        public DateOffset(int? offset, DateUnits unit)
            : this()
        {
            SetOffset(offset, unit);
        }

        public DateOffset(int? offset, DateUnits unit, Func<DateTime> now)
            : this()
        {
            Contract.Requires(now != null);
            NowDate = now;
            SetOffset(offset, unit);
        }

        public DateOffset(DateOffset dateOffset, Func<DateTime> now)
            : this()
        {
            Contract.Requires(dateOffset != null);
            Contract.Requires(now != null);
            NowDate = now;
            SetOffset(dateOffset.Offset, dateOffset.Unit);
        }

        private DateOffset()
        {
            AutoCorrection = true;
            DateCut = true;
        }

        public static bool operator <(DateOffset do1, DateOffset do2)
        {
            if (do1.Unit == do2.Unit)
            {
                // давность больше - дата меньше
                return do1.Offset > do2.Offset;
            }

            if (do1.Unit == DateUnits.Week)
            {
                var d1 = (DateOffset)do1.MemberwiseClone();
                d1.SetOffset(do1.Offset * 7, DateUnits.Day);
                return d1 < do2;
            }
            else if (do2.Unit == DateUnits.Week)
            {
                var d2 = (DateOffset)do2.MemberwiseClone();
                d2.SetOffset(do2.Offset * 7, DateUnits.Day);
                return do1 < d2;
            }

            if (!do1.Month.HasValue || !do2.Month.HasValue)
            {
                // сравниваем месяц и год или день и год
                return do1.Year < do2.Year;
            }
            // сравниваем день и месяц
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
            return do1.Offset == do2.Offset && do1.Unit == do2.Unit;
        }

        public static bool operator !=(DateOffset do1, DateOffset do2)
        {
            return !(do1 == do2);
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

        /// <summary>
        /// Возвращает DateTime представление для объекта DateOffset, если возможно.
        /// </summary>
        public DateTime? GetNullableDateTime()
        {
            return DateHelper.NullableDate(Year, Month, Day);
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}.{3}.{4}", Offset, UnitString, Year ?? 0, Month ?? 0, Day ?? 0);
        }
    }
}