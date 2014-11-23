using log4net;
using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Diagnosis.Common
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
    /// При указании даты отсутствующий более крупный компонент считается сегодняшним (_ _ d -> now.y now.m d).
    /// </summary>
    public class DateOffset : NotifyPropertyChangedBase // should be struct
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(DateOffset));

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
        private bool _unitFixed;

        private DateSetting _dateSetting;
        private UnitSetting _unitSetting;
        private bool inSetting;

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
                    //   logger.DebugFormat("{0}, set year = {1}", this, value);
                    if (!inSetting)
                    {
                        SetDate(value, Month, Day);
                    }
                    OnPropertyChanged("Year");
                    OnPropertyChanged("IsEmpty");
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
                    //  logger.DebugFormat("{0}, set month = {1}", this, value);
                    if (!inSetting)
                    {
                        SetDate(Year, value, Day);
                    }
                    OnPropertyChanged("Month");
                    OnPropertyChanged("IsEmpty");
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
                    //   logger.DebugFormat("{0}, set day = {1}", this, value);
                    if (!inSetting)
                    {
                        SetDate(Year, Month, value);
                    }
                    OnPropertyChanged("Day");
                    OnPropertyChanged("IsEmpty");
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
                    //   logger.DebugFormat("{0}, set offset = {1}", this, value);
                    if (!inSetting)
                    {
                        SetOffset(value, Unit, true);
                    }
                    OnPropertyChanged("Offset");
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
                    //    logger.DebugFormat("{0}, set unit = {1}", this, value);
                    if (!inSetting)
                    {
                        UnitFixed = true;
                        SetOffset(Offset, value);
                    }
                    OnPropertyChanged("Unit");
                }
            }
        }

        /// <summary>
        /// Единица измерения зафиксирована и больше не меняется. (Если DateSetting SavesUnit)
        /// </summary>
        public bool UnitFixed
        {
            get
            {
                return _unitFixed;
            }
            set
            {
                if (_unitFixed != value && (value && DateSettingStrategy == DateSetting.SavesUnit))
                {
                    //   logger.DebugFormat("{0}, unit fixed", this);
                    _unitFixed = value;
                    OnPropertyChanged(() => UnitFixed);
                }
            }
        }

        /// <summary>
        /// Все настройки.
        /// </summary>
        public DateOffsetSettings Settings
        {
            get
            {
                return new DateOffsetSettings(UnitSettingStrategy, DateSettingStrategy, AutoCorrection, CutsDate);
            }
            set
            {
                UnitSettingStrategy = value.Unit;
                DateSettingStrategy = value.Date;
                AutoCorrection = value.AutoCorrection;
                CutsDate = value.CutsDate;
            }
        }

        /// <summary>
        /// Убирать несущественные части даты (Unit = Month -> day = null).
        /// По умолчанию true.
        /// (5 дней - дата с точностью до дня - меняем на 5 лет - день и месяц пропадают)
        /// </summary>
        public bool CutsDate
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

        public UnitSetting UnitSettingStrategy
        {
            get
            {
                return _unitSetting;
            }
            set
            {
                if (_unitSetting != value)
                {
                    _unitSetting = value;
                    //   logger.DebugFormat("{0}", value);
                    //if (value == UnitSetting.RoundsOffset && DateSettingStrategy == DateSetting.SetsUnitSilly)
                    //{
                    //    DateSettingStrategy = DateSetting.RoundsUnit;
                    //}
                    //if (value == UnitSetting.SetsDate && DateSettingStrategy == DateSetting.SavesUnit)
                    //{
                    //    DateSettingStrategy = DateSetting.RoundsUnit;
                    //}
                    OnPropertyChanged(() => UnitSettingStrategy);
                }
            }
        }

        public DateSetting DateSettingStrategy
        {
            get
            {
                return _dateSetting;
            }
            set
            {
                if (_dateSetting != value)
                {
                    _dateSetting = value;
                    //   logger.DebugFormat("{0}", value);
                    if (value != DateSetting.SavesUnit)
                    {
                        UnitFixed = false;
                    }
                    //if (value == DateSetting.SetsUnitSilly && UnitSettingStrategy == UnitSetting.RoundsOffset)
                    //{
                    //    UnitSettingStrategy = UnitSetting.SetsDate;
                    //}
                    //if (value == DateSetting.SavesUnit && UnitSettingStrategy == UnitSetting.SetsDate)
                    //{
                    //    UnitSettingStrategy = UnitSetting.RoundsOffset;
                    //}
                    OnPropertyChanged(() => DateSettingStrategy);
                }
            }
        }

        /// <summary>
        /// Пустая дата.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return Year == null && Month == null && Day == null;
            }
        }

        /// <summary>
        /// Добавляет указанное количество дней, недель (как 7 дней), месяцев или лет.
        /// Нельзя прибавить дни, если Unit - месяц или год и т.п.
        /// </summary>
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
        /// Задает смещение и единицу.
        /// </summary>
        /// <param name="forceSetDateByOffsetUnit">Установка даты при задании только смещения или создании объекта.</param>
        private void SetOffset(int? offset, DateUnits unit, bool forceSetDateByOffsetUnit = false)
        {
            inSetting = true;

            Offset = offset;
            Unit = unit;

            if (forceSetDateByOffsetUnit)
            {
                SetDateByOffsetUnit();
                inSetting = false;
                return;
            }

            switch (UnitSettingStrategy)
            {
                case UnitSetting.RoundsOffset:
                    RoundOffset();
                    break;

                case UnitSetting.SetsDate:
                    SetDateByOffsetUnit();
                    break;
            }
            inSetting = false;
        }

        /// <summary>
        /// Задает дату.
        /// </summary>
        private void SetDate(int? year, int? month, int? day)
        {
            // нулевые значения допустимы
            var y = year != 0 ? year : null;
            var m = month != 0 ? month : null;
            var d = day != 0 ? day : null;

            inSetting = true;
            Year = y;
            Month = m;
            Day = d;

            if (IsEmpty)
            {
                Offset = null;
                inSetting = false;
                return;
            }

            // если нет года или года и месяца, считаем их из Now
            bool yearWas = Year.HasValue;
            if (!Year.HasValue)
            {
                Year = Now.Year;
            }
            if (!Month.HasValue)
            {
                if (yearWas)
                {
                    if (CutsDate)
                    {
                        Day = null;
                    }
                }
                else
                {
                    Month = Now.Month;
                }
            }

            ValidateDate();

            switch (DateSettingStrategy)
            {
                case DateSetting.SetsUnitSilly:
                    SetOffsetUnitByDateSilly(yearWas);
                    break;

                case DateSetting.RoundsUnit:
                    SetOffsetUnitByDateRound();
                    break;

                case DateSetting.SavesUnit:
                    if (!UnitFixed)
                        SetOffsetUnitByDateRound();
                    else
                        RoundOffset(); // after ctor
                    break;
            }

            inSetting = false;
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
                    if (CutsDate)
                    {
                        Day = null;
                    }
                    break;

                case DateUnits.Year:
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

            if (ValidateDate())
            {
                Debug.Assert(!CutsDate);
            }
        }

        /// <summary>
        /// TODO Округляет смещение.
        /// </summary>
        private void RoundOffset()
        {
            Contract.Ensures(Contract.OldValue(_year) == Contract.ValueAtReturn(out _year));
            Contract.Ensures(Contract.OldValue(_month) == Contract.ValueAtReturn(out _month));
            Contract.Ensures(Contract.OldValue(_day) == Contract.ValueAtReturn(out _day));
            Contract.Ensures(Contract.OldValue(_unit) == Contract.ValueAtReturn(out _unit));

            if (IsEmpty)
            {
                Offset = null;
                return;
            }
            switch (Unit)
            {
                case DateUnits.Day:
                    if (Day.HasValue)
                    {
                        Offset = (Now - GetNullableDateTime().Value).Days;
                    }
                    else if (Month.HasValue)
                    {
                        Offset = 999;
                    }
                    else
                    {
                        Offset = 999; // как считать дни из месяцев и лет, укрупнение
                    }

                    break;

                case DateUnits.Week:
                    if (Day.HasValue)
                    {
                        Offset = (Now - GetNullableDateTime().Value).Days / 7;
                    }
                    else if (Month.HasValue)
                    {
                        Offset = 999;
                    }
                    else
                    {
                        Offset = 999;
                    }
                    break;

                case DateUnits.Month:
                    if (Month.HasValue)
                    {
                        Offset = GetTotalMonths();
                    }
                    else
                    {
                        Offset = 999;
                    }
                    break;

                case DateUnits.Year:
                    Offset = Now.Year - Year.Value;

                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Установка даты меняет единицу измерения и смещение c максимальной точностью.
        /// </summary>
        private void SetOffsetUnitByDateSilly(bool yearWas)
        {
            Contract.Ensures(Contract.OldValue(_year) == Contract.ValueAtReturn(out _year));
            Contract.Ensures(Contract.OldValue(_month) == Contract.ValueAtReturn(out _month));
            Contract.Ensures(Contract.OldValue(_day) == Contract.ValueAtReturn(out _day));

            if (Month.HasValue)
            {
                if (Day.HasValue)
                {
                    // ? m d
                    Offset = (Now - GetNullableDateTime().Value).Days;
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
                    Offset = GetTotalMonths();
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
                }
            }
        }

        /// <summary>
        /// Установка даты меняет единицу измерения и смещение на наиболее подходящие.
        /// </summary>
        private void SetOffsetUnitByDateRound()
        {
            Contract.Ensures(Contract.OldValue(_year) == Contract.ValueAtReturn(out _year));
            Contract.Ensures(Contract.OldValue(_month) == Contract.ValueAtReturn(out _month));
            Contract.Ensures(Contract.OldValue(_day) == Contract.ValueAtReturn(out _day));

            if (Month == null && (Day == null || !CutsDate))
            {
                Offset = Now.Year - Year.Value;
                Unit = DateUnits.Year;
            }
            else if (Day == null && Month != null)
            {
                var months = GetTotalMonths();
                if (months > 12)
                {
                    Offset = Now.Year - Year.Value;
                    Unit = DateUnits.Year;
                }
                else
                {
                    Offset = months;
                    Unit = DateUnits.Month;
                }
            }
            else
            {
                Debug.Assert(!IsEmpty);

                var days = (Now - GetNullableDateTime().Value).Days;
                if (days < 7)
                {
                    Offset = days;
                    Unit = DateUnits.Day;
                }
                else if (days < 4 * 7)
                {
                    Offset = days / 7;
                    Unit = DateUnits.Week;
                }
                else
                {
                    var months = GetTotalMonths();
                    if (months > 12)
                    {
                        Offset = Now.Year - Year.Value;
                        Unit = DateUnits.Year;
                    }
                    else
                    {
                        Offset = months;
                        Unit = DateUnits.Month;
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int GetTotalMonths()
        {
            return (Now.Year - Year.Value) * 12 + Now.Month - Month.Value;
        }

        /// <summary>
        /// Проверяет дату, опционально исправляет.
        /// </summary>
        /// <returns>true, если было исправление.</returns>
        private bool ValidateDate()
        {
            var m = Month;
            var d = Day;
            if (AutoCorrection)
            {
                if (DateHelper.CheckAndCorrectDate(Year, ref m, ref d))
                {
                    Month = m;
                    Day = d;
                    return true;
                }
            }
            else
            {
                DateHelper.CheckDate(Year, m, d);
            }
            return false;
        }

        #region ctors

        public DateOffset(int? year, int? month, int? day, Func<DateTime> now = null, DateOffsetSettings settings = null)
            : this()
        {
            if (now != null)
                NowDate = now;
            if (settings != null)
                Settings = settings;
            SetDate(year, month, day);
        }

        public DateOffset(DateTime dt, Func<DateTime> now = null, DateOffsetSettings settings = null)
            : this(dt.Year, dt.Month, dt.Day, now, settings)
        {
        }

        public DateOffset(int? offset, DateUnits unit, Func<DateTime> now = null, DateOffsetSettings settings = null)
            : this()
        {
            if (now != null)
                NowDate = now;
            if (settings != null)
                Settings = settings;
            SetOffset(offset, unit, true);
        }

        /// <summary>
        /// TODO Для смены nowdate при поиске.
        /// </summary>
        /// <param name="dateOffset"></param>
        /// <param name="now"></param>
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
            CutsDate = true;
        }

        #endregion ctors

        #region operators

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

        #endregion operators

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

        public enum UnitSetting
        {
            /// <summary>
            /// Установка единицы измерения задаёт дату.
            /// Не может быть вместе с DateSetting.SavesUnit
            /// </summary>
            SetsDate,
            /// <summary>
            /// Установка единицы измерения не меняет дату, меняется величина смещения.
            /// Не может быть вместе с DateSetting.SetsUnitSilly
            /// </summary>
            RoundsOffset,
        }

        public enum DateSetting
        {
            /// <summary>
            /// Установка даты меняет единицу измерения и смещение c максимальной точностью.
            /// Не может быть вместе с UnitSetting.RoundsOffset
            /// </summary>
            SetsUnitSilly,
            /// <summary>
            /// Установка даты меняет единицу измерения и смещение на наиболее подходящие. Дата 30 дней назад -> 1 месяц.
            /// </summary>
            RoundsUnit,
            /// <summary>
            /// Установка даты меняет единицу измерения и смещение на наиболее подходящие.
            /// После того, как единица была сохранена или установлена непосредственно,
            /// установка даты меняет единицу измерения только в сторону укрупнения.
            /// Не может быть вместе с UnitSetting.SetsDate
            /// </summary>
            SavesUnit,
        }

        public class DateOffsetSettings
        {
            public UnitSetting Unit { get; private set; }

            public DateSetting Date { get; private set; }

            public bool AutoCorrection { get; private set; }

            public bool CutsDate { get; private set; }

            public DateOffsetSettings(UnitSetting unit = UnitSetting.SetsDate, DateSetting date = DateSetting.SetsUnitSilly, bool autoCorrection = true, bool cutsDate = true)
            {
                this.Unit = unit;
                this.Date = date;
                this.AutoCorrection = autoCorrection;
                this.CutsDate = cutsDate;
            }
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            //Contract.Invariant(!(DateSettingStrategy == DateSetting.SavesUnit && UnitSettingStrategy == UnitSetting.SetsDate));
            //Contract.Invariant(!(DateSettingStrategy == DateSetting.SetsUnitSilly && UnitSettingStrategy == UnitSetting.RoundsOffset));
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}.{3}.{4}", Offset, DateOffset.FormatUnit(Offset, Unit), Year ?? 0, Month ?? 0, Day ?? 0);
        }
    }
}