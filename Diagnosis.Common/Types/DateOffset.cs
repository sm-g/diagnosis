using Diagnosis.Common.Util;
using log4net;
using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Diagnosis.Common.Types
{
    public enum DateUnit
    {
        Day,
        Week,
        Month,
        Year
    }

    /// <summary>
    /// Неполная дата со смещением относительно некоторой даты в днях, неделях, месяцах или годах, зависит от полноты даты.
    /// Заданная днём, месяцем и годом дата будет иметь смещение в неделях, если число дней нацело делится на 7.
    /// При указании даты отсутствующий более крупный компонент считается сегодняшним (_ _ d -> now.y now.m d).
    /// </summary>
    public class DateOffset : NotifyPropertyChangedBase, IComparable // should be struct
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(DateOffset));
        private DateTime _now = DateTime.Now;
        private int? _offset;
        private DateUnit _unit;
        private int? _year;
        private int? _month;
        private int? _day;
        private bool _autoCorrection;
        private bool _dateCut;
        private bool _unitFixed;

        private DateSetting _dateSetting;
        private UnitSetting _unitSetting;
        private ReentrantFlag inSetting = new ReentrantFlag();

        /// <summary>
        /// Момент, с которого считается смещение.
        /// </summary>
        public DateTime Now
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

        public int? Year
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
        public int? Month
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
        public int? Day
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
                    SetOffset(value, Unit, true);
                    OnPropertyChanged("Offset");
                }
            }
        }

        /// <summary>
        /// Единица измерения смещения. Устанавливается после Year, Month, Day.
        /// </summary>
        public DateUnit Unit
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
                    if (inSetting.CanEnter)
                    {
                        UnitFixed = true;
                    }
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
        /// Убирать несущественные части даты.
        /// По умолчанию true.
        ///
        /// Unit = Month -> day = null.
        /// Month = null -> day = null.
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
#if !DEBUG
                    if (value == UnitSetting.RoundsOffset && DateSettingStrategy == DateSetting.SetsUnitSilly)
                    {
                        DateSettingStrategy = DateSetting.RoundsUnit;
                    }
                    if (value == UnitSetting.SetsDate && DateSettingStrategy == DateSetting.SavesUnit)
                    {
                        DateSettingStrategy = DateSetting.RoundsUnit;
                    }
#endif
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
#if !DEBUG
                    if (value == DateSetting.SetsUnitSilly && UnitSettingStrategy == UnitSetting.RoundsOffset)
                    {
                        UnitSettingStrategy = UnitSetting.SetsDate;
                    }
                    if (value == DateSetting.SavesUnit && UnitSettingStrategy == UnitSetting.SetsDate)
                    {
                        UnitSettingStrategy = UnitSetting.RoundsOffset;
                    }
#endif
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
        public void Add(int value, DateUnit unit)
        {
            if (unit.CompareTo(Unit) < 0 && !(unit == DateUnit.Day && Unit == DateUnit.Week))
                throw new ArgumentException("Can not add such part of date to current dateoffset.");
            Contract.EndContractBlock();

            DateTime dt;
            switch (unit)
            {
                case DateUnit.Day:
                    dt = new DateTime(Year.Value, Month.Value, Day.Value).AddDays(value);
                    SetDate(dt.Year, dt.Month, dt.Day);
                    //Year = dt.Year;
                    //Month = dt.Month;
                    //Day = dt.Day;
                    break;

                case DateUnit.Week:
                    dt = new DateTime(Year.Value, Month.Value, Day.Value).AddDays(value * 7);
                    SetDate(dt.Year, dt.Month, dt.Day);
                    // Year = dt.Year;
                    //Month = dt.Month;
                    //Day = dt.Day;
                    break;

                case DateUnit.Month:
                    dt = new DateTime(Year.Value, Month.Value, 1).AddMonths(value);
                    Month = dt.Month;
                    Year = dt.Year;
                    break;

                case DateUnit.Year:
                    dt = new DateTime(Year.Value, 1, 1).AddYears(value);
                    Year = dt.Year;
                    break;

                default:
                    break;
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

                    switch (UnitSettingStrategy)
                    {
                        case UnitSetting.RoundsOffset:
                            RoundOffset();
                            break;

                        case UnitSetting.SetsDate:
                            SetDateByOffsetUnit();
                            break;
                    }
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

                    switch (DateSettingStrategy)
                    {
                        case DateSetting.SetsUnitSilly:
                            SetOffsetUnitByDateSilly(_year.HasValue);
                            break;

                        case DateSetting.RoundsUnit:
                            SetOffsetUnitByDateRound();
                            break;

                        case DateSetting.SavesUnit:
                            if (UnitFixed)
                                RoundOffset();
                            else
                                SetOffsetUnitByDateRound(); // from ctor
                            break;
                    }
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

            if (ValidateDate())
            {
                Debug.Assert(!CutsDate);
            }
        }

        /// <summary>
        /// Округляет смещение.
        /// При укрупнении единицы смещение считается для полной даты с 1 вместо отсутствующих значений.
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
                case DateUnit.Day:
                    Offset = (Now - GetSortingDate()).Days;
                    break;

                case DateUnit.Week:
                    Offset = (Now - GetSortingDate()).Days / 7;
                    break;

                case DateUnit.Month:
                    if (Month.HasValue)
                    {
                        Offset = DateHelper.GetTotalMonthsBetween(Now, Year.Value, Month.Value);
                    }
                    else
                    {
                        Offset = DateHelper.GetTotalMonthsBetween(Now, Year.Value, 1);
                    }
                    break;

                case DateUnit.Year:
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
        /// Установка даты меняет единицу измерения и смещение на наиболее подходящие.
        /// </summary>
        private void SetOffsetUnitByDateRound()
        {
            Contract.Requires(Year != null);
            Contract.Ensures(Contract.OldValue(_year) == Contract.ValueAtReturn(out _year));
            Contract.Ensures(Contract.OldValue(_month) == Contract.ValueAtReturn(out _month));
            Contract.Ensures(Contract.OldValue(_day) == Contract.ValueAtReturn(out _day));

            if (Month == null) // _ _ y (или d _ y без автообрезания)
            {
                Offset = Now.Year - Year.Value;
                Unit = DateUnit.Year;
            }
            else if (Day == null) // _ m y
            {
                SetOffsetUnitMonthOrYear();
            }
            else // d m y
            {
                var days = (Now - (DateTime)this).Days;
                if (days < 7) // меньше недели - дни
                {
                    Offset = days;
                    Unit = DateUnit.Day;
                }
                else if (days < 4 * 7) // меньше месяца - недели
                {
                    Offset = days / 7;
                    Unit = DateUnit.Week;
                }
                else
                {
                    SetOffsetUnitMonthOrYear();

                }
            }
        }

        private void SetOffsetUnitMonthOrYear()
        {
            var months = DateHelper.GetTotalMonthsBetween(Now, Year.Value, Month.Value);
            if (months < 12)
            {
                Offset = months;
                Unit = DateUnit.Month;
            }
            else
            {
                Offset = Now.Year - Year.Value;
                Unit = DateUnit.Year;
            }
        }

        /// <summary>
        /// Возвращает DateTime представление для объекта DateOffset, отсутствующие значения заменены 1.
        /// </summary>
        public DateTime GetSortingDate()
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
                Now = now();
            if (settings != null)
                Settings = settings;
            SetDate(year, month, day);
        }

        public DateOffset(DateTime dt, Func<DateTime> now = null, DateOffsetSettings settings = null)
            : this(dt.Year, dt.Month, dt.Day, now, settings)
        {
        }

        public DateOffset(int? offset, DateUnit unit, Func<DateTime> now = null, DateOffsetSettings settings = null)
            : this()
        {
            if (now != null)
                Now = now();
            if (settings != null)
                Settings = settings;
            SetOffset(offset, unit, true);
        }

        /// <summary>
        /// Для смены nowdate. (поиск)
        /// </summary>
        /// <param name="dateOffset"></param>
        /// <param name="now"></param>
        public DateOffset(DateOffset dateOffset, Func<DateTime> now)
            : this()
        {
            Contract.Requires(dateOffset != null);
            Contract.Requires(now != null);
            Now = now();
            Settings = dateOffset.Settings;
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

            /// <summary>
            /// Исправлять число при изменении месяца, чтобы дата оставалась корректной.
            /// По умолчанию true.
            /// 31 dec. month = nov -> day == 30.
            /// 29 feb 2012. year = 2013 -> day == 28.
            /// </summary>
            public bool AutoCorrection { get; private set; }

            /// <summary>
            /// Убирать несущественные части даты (Unit = Month -> day = null).
            /// По умолчанию true.
            /// (5 дней - дата с точностью до дня - меняем на 5 лет - день и месяц пропадают)
            /// </summary>
            public bool CutsDate { get; private set; }

            public DateOffsetSettings(UnitSetting unit = UnitSetting.SetsDate, DateSetting date = DateSetting.SetsUnitSilly, bool autoCorrection = true, bool cutsDate = true)
            {
                this.Unit = unit;
                this.Date = date;
                this.AutoCorrection = autoCorrection;
                this.CutsDate = cutsDate;
            }

            /// <summary>
            /// Точная взаимно-однозначная установка
            /// </summary>
            public static DateOffsetSettings ExactSetting()
            {
                return new DateOffsetSettings(UnitSetting.SetsDate, DateSetting.SetsUnitSilly, true, true);
            }

            /// <summary>
            /// Установить дату, откорректировать единицу округления на желаемую.
            /// При смене даты единица снова округляется.
            /// </summary>
            public static DateOffsetSettings Rounding()
            {
                return new DateOffsetSettings(UnitSetting.RoundsOffset, DateSetting.RoundsUnit, true, true);
            }

            /// <summary>
            /// Установить дату и единицу на сохраненные, смещение округляется по единице.
            /// </summary>
            public static DateOffsetSettings OnLoading()
            {
                return new DateOffsetSettings(UnitSetting.RoundsOffset, DateSetting.SetsUnitSilly, true, true);
            }

            public static bool operator ==(DateOffsetSettings do1, DateOffsetSettings do2)
            {
                if (object.ReferenceEquals(do1, null) || object.ReferenceEquals(do2, null))
                {
                    return object.ReferenceEquals(do1, do2);
                }
                return do1.Unit == do2.Unit
                    && do1.Date == do2.Date
                    && do1.AutoCorrection == do2.AutoCorrection
                    && do1.CutsDate == do2.CutsDate;
            }

            public static bool operator !=(DateOffsetSettings do1, DateOffsetSettings do2)
            {
                return !(do1 == do2);
            }

            public override bool Equals(object obj)
            {
                var other = obj as DateOffsetSettings;
                if (other == null)
                    return false;
                return this == other;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + Unit.GetHashCode();
                    hash = hash * 23 + Date.GetHashCode();
                    hash = hash * 23 + AutoCorrection.GetHashCode();
                    hash = hash * 23 + CutsDate.GetHashCode();
                    return hash;
                }
            }
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
#if !DEBUG
            Contract.Invariant(!(DateSettingStrategy == DateSetting.SavesUnit && UnitSettingStrategy == UnitSetting.SetsDate));
            Contract.Invariant(!(DateSettingStrategy == DateSetting.SetsUnitSilly && UnitSettingStrategy == UnitSetting.RoundsOffset));
#endif
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}.{3}.{4}", Offset, DateOffsetFormatter.GetUnitString(Offset, Unit), Year ?? 0, Month ?? 0, Day ?? 0);
        }

        public override bool Equals(object obj)
        {
            var other = obj as DateOffset;
            if (other == null)
                return false;
            return this == other && this.Settings == other.Settings;
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

        public int CompareTo(object obj)
        {
            var other = obj as DateOffset;
            if (other == null)
                return 1;

            if (other == this)
                return 0;

            if (this.Unit == other.Unit)
            {
                // давность больше - дата меньше
                if (this.Offset > other.Offset)
                    return -1;
                return 1;
            }

            // для недель сравниваем дни
            if (this.Unit == DateUnit.Week)
            {
                var thisInDays = (DateOffset)this.MemberwiseClone();
                thisInDays.SetOffset(this.Offset * 7, DateUnit.Day);
                return thisInDays.CompareTo(other);
            }
            else if (other.Unit == DateUnit.Week)
            {
                var otherInDays = (DateOffset)other.MemberwiseClone();
                otherInDays.SetOffset(other.Offset * 7, DateUnit.Day);
                return this.CompareTo(otherInDays);
            }

            if (!this.Month.HasValue || !other.Month.HasValue)
            {
                // сравниваем месяц и год или день и год
                if (this.Year < other.Year)
                    return -1;
                return 1;
            }
            // сравниваем день и месяц
            if (this.Month < other.Month)
                return -1;
            return 1;
        }
    }
}