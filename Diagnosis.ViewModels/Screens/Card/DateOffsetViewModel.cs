using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
{
    [DebuggerDisplay("doVm {Do}")]
    public class DateOffsetViewModel : ViewModelBase
    {
        private static readonly Dictionary<HealthRecord, DateOffsetViewModel> dict = new Dictionary<HealthRecord, DateOffsetViewModel>();
        private readonly DateOffset d;
        private readonly HealthRecord hr;
        private DateUnit _roundUnit;

        private int? _roundOffset;

        static DateOffsetViewModel()
        {
            typeof(DateOffsetViewModel).Subscribe(Event.DeleteHolder, (e) =>
            {
                var holder = e.GetValue<IHrsHolder>(MessageKeys.Holder);
                holder.HealthRecordsChanged -= Holder_HealthRecordsChanged;
                foreach (var item in holder.HealthRecords)
                {
                    OnHrRemoved(item);
                }
            });
        }

        private DateOffsetViewModel(DateOffset d, HealthRecord hr)
        {
            this.d = d;
            this.hr = hr;
            d.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case "Year":
                        hr.FromYear = d.Year;
                        break;

                    case "Month":
                        hr.FromMonth = d.Month;
                        break;

                    case "Day":
                        hr.FromDay = d.Day;
                        break;
                }
                OnPropertyChanged(e.PropertyName);
                OnPropertyChanged("Do");
                RoundOffsetFor(RoundedUnit);
            };
            hr.PropertyChanged += healthRecord_PropertyChanged;

            if (Year != null)
                RoundOffsetUnitByDate();
        }

        public DateOffset Do
        {
            get { return d; }
        }

        public int? Offset
        {
            get
            {
                return d.Offset;
            }
            set
            {
                if (d.Offset != value)
                {
                    d.Offset = value;
                    OnPropertyChanged(() => Offset);
                }
            }
        }

        public DateUnit Unit
        {
            get
            {
                return d.Unit;
            }
            set
            {
                if (d.Unit != value)
                {
                    d.Unit = value;
                    OnPropertyChanged(() => Unit);
                }
            }
        }

        public int? Year
        {
            get
            {
                return d.Year;
            }
            set
            {
                if (d.Year != value)
                {
                    d.Year = value;
                    if (value != null)
                        RoundOffsetUnitByDate();
                    OnPropertyChanged(() => Year);
                }
            }
        }

        public int? Month
        {
            get
            {
                return d.Month;
            }
            set
            {
                if (d.Month != value)
                {
                    d.Month = value;
                    RoundOffsetUnitByDate();
                    OnPropertyChanged(() => Month);
                }
            }
        }

        public int? Day
        {
            get
            {
                return d.Day;
            }
            set
            {
                if (d.Day != value)
                {
                    d.Day = value;
                    RoundOffsetUnitByDate();
                    OnPropertyChanged(() => Day);
                }
            }
        }

        public DateUnit RoundedUnit
        {
            get
            {
                return _roundUnit;
            }
            set
            {
                if (_roundUnit != value)
                {
                    _roundUnit = value;
                    RoundOffsetFor(value);
                    OnPropertyChanged(() => RoundedUnit);
                }
            }
        }

        public int? RoundedOffset
        {
            get
            {
                return _roundOffset;
            }
            set
            {
                if (_roundOffset != value)
                {
                    _roundOffset = value;
                    OnPropertyChanged(() => RoundedOffset);
                }
            }
        }

        public DateTime Now
        {
            get { return d.Now; }
            set
            {
                d.Now = value;
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

        public RelayCommand SpinUnitCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RoundedUnit = RoundedUnit.GetNextDateUnit();
                });
            }
        }

        public static DateOffsetViewModel FromHr(HealthRecord healthRecord)
        {
            DateOffsetViewModel res;
            if (!dict.TryGetValue(healthRecord, out res))
            {
                Debug.Assert(healthRecord.CreatedAt != DateTime.MinValue);

                var d = new DateOffset(healthRecord.FromYear, healthRecord.FromMonth, healthRecord.FromDay, () => healthRecord.CreatedAt);
                d.UnitSettingStrategy = Common.DateOffset.UnitSetting.SetsDate;
                d.DateSettingStrategy = Common.DateOffset.DateSetting.SetsUnitSilly;

                // один раз подписываемся на удаление записи у держателя
                if (!dict.Keys.Any(hr => hr.Holder == healthRecord.Holder))
                    healthRecord.Holder.HealthRecordsChanged += Holder_HealthRecordsChanged;

                res = new DateOffsetViewModel(d, healthRecord);
                dict[healthRecord] = res;
            }
            return res;
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                hr.PropertyChanged -= healthRecord_PropertyChanged;
            }
            base.Dispose(disposing);
        }

        private static void Holder_HealthRecordsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (HealthRecord item in e.OldItems)
                {
                    OnHrRemoved(item);
                }
            }
        }

        private static void OnHrRemoved(HealthRecord item)
        {
            DateOffsetViewModel res;
            if (dict.TryGetValue(item, out res))
            {
                dict.Remove(item);
                res.Dispose();
            }
        }
        private void healthRecord_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var hr = sender as HealthRecord;
            switch (e.PropertyName)
            {
                case "FromDay":
                    Day = hr.FromDay;
                    break;

                case "FromMonth":
                    Month = hr.FromMonth;
                    break;

                case "FromYear":
                    Year = hr.FromYear;
                    break;

                case "Unit":
                    var doUnit = hr.Unit.ToDateOffsetUnit();
                    RoundedUnit = doUnit ?? RoundedUnit;
                    break;
            }
        }

        /// <summary>
        /// Установка даты меняет единицу измерения и смещение на наиболее подходящие.
        /// </summary>
        private void RoundOffsetUnitByDate()
        {
            Contract.Requires(Year != null);

            if (Month == null) // _ _ y (или d _ y без автообрезания)
            {
                RoundedOffset = Now.Year - Year.Value;
                RoundedUnit = DateUnit.Year;
            }
            else if (Day == null) // _ m y
            {
                SetRoundedOffsetUnitMonthOrYear();
            }
            else // d m y
            {
                var days = (Now - (DateTime)d).Days;
                if (days < 7) // меньше недели - дни
                {
                    RoundedOffset = days;
                    RoundedUnit = DateUnit.Day;
                }
                else if (days < 4 * 7) // меньше месяца - недели
                {
                    RoundedOffset = days / 7;
                    RoundedUnit = DateUnit.Week;
                }
                else
                {
                    SetRoundedOffsetUnitMonthOrYear();
                }
            }
        }
        private void SetRoundedOffsetUnitMonthOrYear()
        {
            var months = DateHelper.GetTotalMonthsBetween(Now, Year.Value, Month.Value);
            if (months < 12) // меньше года - месяцы
            {
                RoundedOffset = months;
                RoundedUnit = DateUnit.Month;
            }
            else
            {
                RoundedOffset = Now.Year - Year.Value;
                RoundedUnit = DateUnit.Year;
            }
        }

        /// <summary>
        /// Округляет смещение.
        /// При укрупнении единицы смещение считается для полной даты с 1 вместо отсутствующих значений.
        /// </summary>
        private void RoundOffsetFor(DateUnit unit)
        {
            if (!Year.HasValue)
            {
                RoundedOffset = null;
                return;
            }
            switch (unit)
            {
                case DateUnit.Day:
                    RoundedOffset = (Now - GetSortingDate()).Days;
                    break;

                case DateUnit.Week:
                    RoundedOffset = (Now - GetSortingDate()).Days / 7;
                    break;

                case DateUnit.Month:
                    if (Month.HasValue)
                    {
                        RoundedOffset = DateHelper.GetTotalMonthsBetween(Now, Year.Value, Month.Value);
                    }
                    else
                    {
                        RoundedOffset = DateHelper.GetTotalMonthsBetween(Now, Year.Value, 1);
                    }
                    break;

                case DateUnit.Year:
                    RoundedOffset = Now.Year - Year.Value;
                    break;
            }
        }
    }
}