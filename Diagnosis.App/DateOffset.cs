using System;
using System.Diagnostics.Contracts;

namespace Diagnosis.App
{
    public enum DateUnits
    {
        Day,
        Month,
        Year
    }
}

namespace Diagnosis.App.ViewModels
{
    /// <summary>
    /// Неполная дата со смещением относительно сегодня в днях, месяцах или годах, зависит от полноты даты.
    /// </summary>
    public class DateOffset : ViewModelBase
    {
        public const int MinYear = 1880;

        private int _offset;
        private DateUnits _unit;
        private int? _year;
        private int? _month;
        private int? _day;

        bool setting;

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
                    OnPropertyChanged(() => Year);
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
                    OnPropertyChanged(() => Month);
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
                    OnPropertyChanged(() => Day);
                }
            }
        }

        public int Offset
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
                    OnPropertyChanged(() => Unit);
                    OnPropertyChanged(() => Offset);
                }
            }
        }
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
                    OnPropertyChanged(() => Unit);

                }
            }
        }

        DateTime Now { get { return NowDate(); } }

        public void SetOffset(int offset, DateUnits unit)
        {
            setting = true;

            Offset = offset;
            Unit = unit;

            switch (Unit)
            {
                case DateUnits.Day:
                    var date = Now.AddDays(-Offset);
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

            setting = false;
        }

        public void SetDate(int? year, int? month, int? day)
        {
            if ((year.HasValue || month.HasValue || day.HasValue) == false)
                throw new ArgumentNullException("Date cannot be empty.");

            setting = true;

            Year = year;
            Month = month;
            Day = day;

            int y;
            if (year.HasValue)
            {
                y = year.Value;
            }
            else
            {
                y = Now.Year;
            }

            if (month.HasValue)
            {
                if (day.HasValue)
                {
                    Offset = (Now - new DateTime(y, month.Value, day.Value)).Days;
                    Unit = DateUnits.Day;
                }
                else
                {
                    Offset = (Now.Year - y) * 12 + Now.Month - month.Value;
                    Unit = DateUnits.Month;
                }

                if (!year.HasValue)
                {
                    Year = Now.Year;
                }
            }
            else
            {
                if (year.HasValue)
                {
                    Offset = Now.Year - y;
                    Unit = DateUnits.Year;
                }
                else
                {
                    if (day.HasValue)
                    {
                        Offset = 0;
                        Unit = DateUnits.Day;

                        Month = Now.Month;
                        Year = Now.Year;
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

        public DateOffset(int offset, DateUnits unit)
        {
            SetOffset(offset, unit);
        }
        public DateOffset(int offset, DateUnits unit, Func<DateTime> now)
        {
            Contract.Requires(now != null);
            NowDate = now;
            SetOffset(offset, unit);
        }
    }
}