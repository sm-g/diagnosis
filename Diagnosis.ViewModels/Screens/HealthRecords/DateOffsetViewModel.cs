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

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(DateOffsetViewModel));
        private readonly DateOffset from;
        private readonly DateOffset to;
        private readonly HealthRecord hr;
        private DateUnit _roundUnit;
        private ShowAs? _firstSet;
        private int? _roundOffset;
        private Patient patient;
        private DatePickerViewModel _toDpVm;
        private bool _inEdit;

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

        protected DateOffsetViewModel(HealthRecord hr)
        {
            this.from = hr.FromDate;
            this.to = hr.ToDate;
            this.patient = hr.GetPatient();
            this.hr = hr;
            to.PropertyChanged += to_PropertyChanged;
            from.PropertyChanged += from_PropertyChanged;
            hr.PropertyChanged += healthRecord_PropertyChanged;
            patient.PropertyChanged += patient_PropertyChanged;

            Relative = from.RelativeTo(to);

            if (Year != null) // есть дата у записи
            {
                this._firstSet = ShowAs.Date; // не новая запись — не меняем showas при вводе

                RoundOffsetUnitByDate();
            }
        }

        public enum ShowAs
        {
            Date,
            Offset,
            AtAge
        }

        public bool IsClosedInterval { get { return hr.IsClosedInterval; } }

        public bool IsOpenedInterval { get { return hr.IsOpenedInterval; } }

        public bool IsPoint { get { return hr.IsPoint; } }

        public bool SetToWithFrom { get { return !OpenedInEditor && hr.IsPoint; } }

        public bool OpenedInEditor
        {
            get
            {
                return _inEdit;
            }
            set
            {
                if (_inEdit != value)
                {
                    _inEdit = value;
                    OnPropertyChanged(() => OpenedInEditor);
                }
            }
        }

        public int? Offset
        {
            get
            {
                return IsClosedInterval ? Relative.Offset : from.Offset;
            }
            set
            {
                if (IsClosedInterval)
                {
                    var rel = Relative;
                    rel.Offset = value;

                    from.FillDateDownTo(rel.GetSortingDate(), rel.Unit);
                }
                else
                {
                    if (SetToWithFrom)
                        to.Offset = value;
                    from.Offset = value;
                }

                FirstSet = ShowAs.Offset;
                OnPropertyChanged(() => Offset);
            }
        }

        public DateUnit Unit
        {
            get
            {
                return IsClosedInterval ? Relative.Unit : from.Unit;
            }
            set
            {
                FirstSet = ShowAs.Offset;
                if (IsClosedInterval)
                {
                    var rel = Relative;
                    rel.Unit = value;

                    // но не cuts
                    FillFixedSide(to, to.GetSortingDate(), value);

                    from.Year = rel.Year;
                    from.Month = rel.Month;
                    from.Day = rel.Day;
                }
                else
                {
                    if (SetToWithFrom)
                        to.Unit = value;
                    from.Unit = value;
                }

                OnPropertyChanged(() => Unit);
            }
        }

        public int? Year
        {
            get
            {
                return from.Year;
            }
            set
            {
                if (from.Year != value)
                {
                    FirstSet = ShowAs.Date;
                    if (SetToWithFrom)
                        to.Year = value;
                    from.Year = value;
                    OnPropertyChanged(() => Year);
                }
            }
        }

        public int? Month
        {
            get
            {
                return from.Month;
            }
            set
            {
                if (from.Month != value)
                {
                    FirstSet = ShowAs.Date;
                    if (SetToWithFrom)
                        to.Month = value;
                    from.Month = value;
                    OnPropertyChanged(() => Month);
                }
            }
        }

        public int? Day
        {
            get
            {
                return from.Day;
            }
            set
            {
                if (from.Day != value)
                {
                    FirstSet = ShowAs.Date;
                    if (SetToWithFrom)
                        to.Day = value;
                    from.Day = value;
                    OnPropertyChanged(() => Day);
                }
            }
        }
        /// <summary>
        /// To fix combobox binding set to null.
        /// </summary>
        public DatePickerViewModel To
        {
            get
            {
                return _toDpVm;
            }
            set
            {
                if (_toDpVm != value)
                {
                    _toDpVm = value;
                    OnPropertyChanged(() => To);
                }
            }
        }

        public int? ToYear
        {
            get
            {
                return to.Year;
            }
            set
            {
                if (to.Year != value)
                {
                    FirstSet = ShowAs.Date;
                    to.Year = value;
                    OnPropertyChanged(() => ToYear);
                }
            }
        }

        public int? ToMonth
        {
            get
            {
                return to.Month;
            }
            set
            {
                if (to.Month != value)
                {
                    FirstSet = ShowAs.Date;
                    to.Month = value;
                    OnPropertyChanged(() => ToMonth);
                }
            }
        }

        public int? ToDay
        {
            get
            {
                return to.Day;
            }
            set
            {
                if (to.Day != value)
                {
                    FirstSet = ShowAs.Date;
                    to.Day = value;

                    OnPropertyChanged(() => ToDay);
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
                    if (IsClosedInterval)
                        RoundedOffset = RoundOffsetFor(Relative, value);
                    else
                        RoundedOffset = RoundOffsetFor(from, value);
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
            private set
            {
                if (_roundOffset != value)
                {
                    _roundOffset = value;
                    OnPropertyChanged(() => RoundedOffset);
                }
            }
        }

        public bool CanShowAsAge
        {
            get
            {
                return patient.BirthYear.HasValue;
            }
        }

        public string AtAgeString
        {
            get
            {
                var fromAge = DateFormatter.GetAgeString(patient.BirthYear, patient.BirthMonth, patient.BirthDay, from.GetSortingDate());
                var toAge = DateFormatter.GetAgeString(patient.BirthYear, patient.BirthMonth, patient.BirthDay, to.GetSortingDate());

                if (IsClosedInterval)
                    return string.Format("c {0} до {1}", fromAge, toAge);
                if (IsOpenedInterval)
                    return string.Format("c {0}", fromAge);
                return string.Format("в {0}", fromAge);
            }
        }

        public int? AtAge
        {
            get
            {
                return CanShowAsAge && Year.HasValue
                    ? Year.Value - patient.BirthYear.Value
                    : (int?)null;
            }
            set
            {
                FirstSet = DateOffsetViewModel.ShowAs.AtAge;

                // установка возраста меняет только год
                Year = patient.BirthYear.Value + value;

                OnPropertyChanged(() => AtAge);
            }
        }

        public int? ToAtAge
        {
            get
            {
                return CanShowAsAge && ToYear.HasValue
                    ? ToYear.Value - patient.BirthYear.Value
                    : (int?)null;
            }
            set
            {
                FirstSet = DateOffsetViewModel.ShowAs.AtAge;

                // установка возраста меняет только год
                ToYear = patient.BirthYear.Value + value;

                OnPropertyChanged(() => ToAtAge);
            }
        }

        public string PartialDateString
        {
            get
            {
                if (IsClosedInterval)
                    return DateOffsetFormatter.GetPartialDateString(from) + " — " +
                           DateOffsetFormatter.GetPartialDateString(to);
                if (IsOpenedInterval)
                    return "с " + DateOffsetFormatter.GetPartialDateString(from);
                return DateOffsetFormatter.GetPartialDateString(from);
            }
        }

        public DateTime OffsetFrom
        {
            get { return IsClosedInterval ? to.GetSortingDate() : hr.DescribedAt; }
            set
            {
                if (!IsClosedInterval)
                {
                    hr.DescribedAt = value;
                    OnPropertyChanged(() => OffsetFrom);
                }
            }
        }

        /// <summary>
        /// Пустая дата.
        /// </summary>
        public bool IsEmpty
        {
            get { return from.IsEmpty; }
        }

        //public DateTime Now
        //{
        //    get { return IsClosedInterval ? Relative.Now : from.Now; }
        //    set
        //    {
        //        //if (!IsClosedInterval)
        //        {
        //            from.Now = value;
        //            to.Now = value;
        //        }
        //    }
        //}
        public bool ToIsEmpty
        {
            get { return to.IsEmpty; }
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

        /// <summary>
        /// В какое поле был первый ввод (для новой записи).
        /// </summary>
        public ShowAs? FirstSet
        {
            get
            {
                return _firstSet;
            }
            set
            {
                if (_firstSet == null) // только один раз
                {
                    _firstSet = value;
                    OnPropertyChanged(() => FirstSet);
                }
            }
        }

        private DateOffset Relative { get; set; }

        public static DateOffsetViewModel FromHr(HealthRecord healthRecord)
        {
            DateOffsetViewModel res;
            if (!dict.TryGetValue(healthRecord, out res))
            {
                // один раз подписываемся на удаление записи у держателя
                if (!dict.Keys.Any(hr => hr.Holder == healthRecord.Holder))
                    healthRecord.Holder.HealthRecordsChanged += Holder_HealthRecordsChanged;

                res = new DateOffsetViewModel(healthRecord);
                dict[healthRecord] = res;
            }
            return res;
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

        /// <summary>
        /// Округляет смещение.
        /// При укрупнении единицы смещение считается для полной даты с 1 вместо отсутствующих значений.
        /// </summary>
        private static int? RoundOffsetFor(DateOffset d, DateUnit unit)
        {
            if (!d.Year.HasValue)
            {
                return null;
            }
            int? RoundedOffset;
            switch (unit)
            {
                case DateUnit.Day:
                    RoundedOffset = (d.Now - d.GetSortingDate()).Days;
                    break;

                case DateUnit.Week:
                    RoundedOffset = (d.Now - d.GetSortingDate()).Days / 7;
                    break;

                case DateUnit.Month:
                    if (d.Month.HasValue)
                    {
                        RoundedOffset = DateHelper.GetTotalMonthsBetween(d.Now, d.Year.Value, d.Month.Value);
                    }
                    else
                    {
                        RoundedOffset = DateHelper.GetTotalMonthsBetween(d.Now, d.Year.Value, 1);
                    }
                    break;

                case DateUnit.Year:
                    RoundedOffset = d.Now.Year - d.Year.Value;
                    break;

                default:
                    throw new NotImplementedException();
            }
            return RoundedOffset;
        }

        private static void FillFixedSide(DateOffset d, DateTime dt, DateUnit value)
        {
            d.FillDateDownTo(dt, value);
        }

        /// <summary>
        /// Установка даты меняет единицу измерения и смещение на наиболее подходящие.
        /// </summary>
        private static DateOffset RoundOffsetUnitByDate(DateOffset d, DateTime described)
        {
            Contract.Requires(d.Year != null);

            int? offset = null;
            DateUnit unit = 0;

            Action setRoundedOffsetUnitMonthOrYear = () =>
            {
                var months = DateHelper.GetTotalMonthsBetween(described, d.Year.Value, d.Month.Value);
                if (months < 12) // меньше года - месяцы
                {
                    offset = months;
                    unit = DateUnit.Month;
                }
                else
                {
                    offset = described.Year - d.Year.Value;
                    unit = DateUnit.Year;
                }
            };

            if (d.Month == null) // _ _ y (или d _ y без автообрезания)
            {
                offset = described.Year - d.Year.Value;
                unit = DateUnit.Year;
            }
            else if (d.Day == null) // _ m y
            {
                setRoundedOffsetUnitMonthOrYear();
            }
            else // d m y
            {
                var days = (described - (DateTime)d).Days;
                if (days < 7) // меньше недели - дни
                {
                    offset = days;
                    unit = DateUnit.Day;
                }
                else if (days < 4 * 7) // меньше месяца - недели
                {
                    offset = days / 7;
                    unit = DateUnit.Week;
                }
                else
                {
                    setRoundedOffsetUnitMonthOrYear();
                }
            }

            return new DateOffset(offset, unit, () => d.Now);
        }

        private void RoundOffsetUnitByDate()
        {
            DateOffset rounding = null;
            if (IsClosedInterval && !Relative.IsEmpty)
                rounding = RoundOffsetUnitByDate(Relative, hr.DescribedAt);
            else if (from.Year != null)
                rounding = RoundOffsetUnitByDate(from, hr.DescribedAt);

            if (rounding != null)
            {
                RoundedOffset = rounding.Offset;
                RoundedUnit = rounding.Unit;
            }
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

        private void patient_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BirthYear" ||
                e.PropertyName == "BirthMonth" ||
                e.PropertyName == "BirthDay")
            {
                OnPropertyChanged(() => CanShowAsAge);
                OnPropertyChanged(() => AtAgeString);
                OnPropertyChanged(() => AtAge);
                OnPropertyChanged(() => ToAtAge);
            }
        }

        private void from_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Common_date_PropertyChanged(e);

            switch (e.PropertyName)
            {
                case "Day":
                case "Month":
                case "Year":
                case "Offset":
                case "Unit":
                    OnPropertyChanged("Day",
                                      "Month",
                                      "Year",
                                      "Offset",
                                      "Unit",
                                      "IsEmpty");
                    break;
            }
        }

        private void to_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Common_date_PropertyChanged(e);

            switch (e.PropertyName)
            {
                case "Day":
                case "Month":
                case "Year":
                    OnPropertyChanged("To" + e.PropertyName);
                    break;
            }
        }

        private void Common_date_PropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Year")
            {
                OnPropertyChanged(() => AtAge);
                OnPropertyChanged(() => ToAtAge);
            }
            switch (e.PropertyName)
            {
                case "Day":
                case "Month":
                case "Year":
                    Relative = from.RelativeTo(to);

                    RoundOffsetUnitByDate();
                    OnPropertyChanged("IsClosedInterval");
                    OnPropertyChanged("IsOpenedInterval");

                    OnPropertyChanged("OffsetFrom");

                    OnPropertyChanged("PartialDateString");
                    OnPropertyChanged("AtAgeString");

                    OnPropertyChanged("Offset");
                    OnPropertyChanged("Unit");
                    break;
            }

            if (IsClosedInterval)
                RoundedOffset = RoundOffsetFor(Relative, RoundedUnit);
            else
                RoundedOffset = RoundOffsetFor(from, RoundedUnit);

            logger.DebugFormat("changed {2}\nfrom {0}\nto {1}", hr.FromDate, hr.ToDate, e.PropertyName);
        }

        private void healthRecord_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var hr = sender as HealthRecord;
            switch (e.PropertyName)
            {
                case "Unit":
                    var doUnit = hr.Unit.ToDateOffsetUnit();
                    RoundedUnit = doUnit ?? RoundedUnit;
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                hr.PropertyChanged -= healthRecord_PropertyChanged;
                to.PropertyChanged -= to_PropertyChanged;
                from.PropertyChanged -= from_PropertyChanged;
                patient.PropertyChanged -= patient_PropertyChanged;
                dict.Remove(this.hr);
            }
            base.Dispose(disposing);
        }

        public class DatePickerViewModel : ViewModelBase
        {
            private DateOffsetViewModel dovm;

            public DatePickerViewModel(DateOffsetViewModel d)
            {
                dovm = d;
                d.to.PropertyChanged += to_PropertyChanged;
            }

            public int? Year
            {
                get
                {
                    return dovm.ToYear;
                }
                set
                {
                    dovm.ToYear = value;
                }
            }

            public int? Month
            {
                get
                {
                    return dovm.ToMonth;
                }
                set
                {
                    dovm.ToMonth = value;
                }
            }

            public int? Day
            {
                get
                {
                    return dovm.ToDay;
                }
                set
                {
                    dovm.ToDay = value;
                }
            }

            protected override void Dispose(bool disposing)
            {
                try
                {
                    if (disposing)
                    {
                        dovm.to.PropertyChanged -= to_PropertyChanged;
                    }
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }

            private void to_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case "Day":
                    case "Month":
                    case "Year":
                        OnPropertyChanged(e.PropertyName);
                        break;
                }
            }
        }
    }
}