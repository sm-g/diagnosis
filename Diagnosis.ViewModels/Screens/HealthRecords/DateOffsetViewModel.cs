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
        internal readonly DateOffset to;
        private readonly HealthRecord hr;
        private readonly Patient patient;
        private DateUnit _roundUnit;
        private ShowAs? _firstSet;
        private int? _roundOffset;
        private DatePickerViewModel _toDpVm;
        private bool _inEdit;
        private DatePickerViewModel _fromDpVm;

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

            if (from.Year != null) // есть дата у записи
            {
                this._firstSet = ShowAs.Date; // не новая запись — не меняем showas при вводе

                RoundOffsetUnitByDate();
            }

            From = new DatePickerViewModel(from);
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

        public bool OpenedInIntervalEditor
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
                    if (value)
                        To = new DateOffsetViewModel.DatePickerViewModel(hr.ToDate);
                    else
                        To = null; // unbind ComboboxDatePicker DataContext;

                    OnPropertyChanged(() => OpenedInIntervalEditor);
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
                FirstSet = ShowAs.Offset;

                if (IsClosedInterval)
                {
                    var rel = Relative;
                    rel.Offset = value;

                    from.FillDateDownTo(rel.GetSortingDate(), rel.Unit);
                }
                else
                {
                    from.Offset = value;
                }

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
                if (IsClosedInterval)
                {
                    var rel = Relative;
                    rel.Unit = value;

                    // но не cuts
                    to.FillDateDownTo(to.GetSortingDate(), value);
                    from.FillDateFrom(rel);
                }
                else
                {
                    from.Unit = value;
                }

                OnPropertyChanged(() => Unit);
            }
        }

        public DatePickerViewModel From
        {
            get
            {
                return _fromDpVm;
            }
            set
            {
                if (_fromDpVm != value)
                {
                    _fromDpVm = value;
                    OnPropertyChanged(() => From);
                }
            }
        }

        /// <summary>
        /// Null when interval editor is closed.
        /// Set to null fixes combobox binding.
        /// </summary>
        public DatePickerViewModel To
        {
            get
            {
                Contract.Ensures(Contract.Result<DatePickerViewModel>() != null || !OpenedInIntervalEditor);
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
                        RoundedOffset = Relative.RoundOffsetFor(value);
                    else
                        RoundedOffset = from.RoundOffsetFor(value);
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
                return CanShowAsAge && from.Year.HasValue
                    ? DateHelper.GetAge(patient.BirthYear, patient.BirthMonth, patient.BirthDay, from.GetSortingDate())
                    : (int?)null;
            }
            set
            {
                Contract.Ensures(patient.BirthYear == null || value == null ||
                    value == DateHelper.GetAge(patient.BirthYear, patient.BirthMonth, patient.BirthDay, from.GetSortingDate()));

                if (patient.BirthYear == null)
                    return;

                FirstSet = DateOffsetViewModel.ShowAs.AtAge;

                // установка возраста меняет только год
                if (value.HasValue)
                    from.Year = DateHelper.GetYearForAge(value.Value, patient.BirthYear.Value, patient.BirthMonth, patient.BirthDay, from.GetSortingDate());
                else
                    from.Year = null;

                OnPropertyChanged(() => AtAge);
            }
        }

        public int? ToAtAge
        {
            get
            {
                return CanShowAsAge && to.Year.HasValue
                    ? DateHelper.GetAge(patient.BirthYear, patient.BirthMonth, patient.BirthDay, to.GetSortingDate())
                    : (int?)null;
            }
            set
            {
                Contract.Ensures(patient.BirthYear == null || value == null ||
                    value == DateHelper.GetAge(patient.BirthYear, patient.BirthMonth, patient.BirthDay, to.GetSortingDate()));

                if (patient.BirthYear == null)
                    return;

                FirstSet = DateOffsetViewModel.ShowAs.AtAge;

                if (value.HasValue)
                    to.Year = DateHelper.GetYearForAge(value.Value, patient.BirthYear.Value, patient.BirthMonth, patient.BirthDay, to.GetSortingDate());
                else
                    to.Year = null;

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
        /// В какое поле был первый ввод (для записи без даты, открытой в этой сессии).
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

        internal static void ClearDict()
        {
            dict.Values.ForEach(x => x.Dispose());
            Contract.Assert(dict.Count == 0);
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

        private void RoundOffsetUnitByDate()
        {
            DateOffset rounding = null;
            if (IsClosedInterval && !Relative.IsEmpty)
                rounding = Relative.RoundOffsetUnitByDate(hr.DescribedAt);
            else if (from.Year != null)
                rounding = from.RoundOffsetUnitByDate(hr.DescribedAt);

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
                case "Offset":
                    OnPropertyChanged(() => Offset);
                    break;

                case "Unit":
                    OnPropertyChanged(() => Unit);
                    break;

                case "Year":
                    OnPropertyChanged(() => IsEmpty);
                    break;
            }
        }

        private void to_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Common_date_PropertyChanged(e);
        }

        private void Common_date_PropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Day":
                case "Month":
                case "Year":
                    FirstSet = ShowAs.Date;
                    Relative = from.RelativeTo(to);

                    RoundOffsetUnitByDate();
                    OnPropertyChanged(() => IsClosedInterval);
                    OnPropertyChanged(() => IsOpenedInterval);

                    OnPropertyChanged(() => OffsetFrom);

                    OnPropertyChanged(() => PartialDateString);
                    OnPropertyChanged(() => AtAgeString);
                    OnPropertyChanged(() => AtAge);
                    OnPropertyChanged(() => ToAtAge);

                    if (IsClosedInterval)
                    {
                        OnPropertyChanged(() => Offset);
                        OnPropertyChanged(() => Unit);
                    }
                    break;
            }

            if (IsClosedInterval)
                RoundedOffset = Relative.RoundOffsetFor(RoundedUnit);
            else
                RoundedOffset = from.RoundOffsetFor(RoundedUnit);

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
            private DateOffset d;

            public DatePickerViewModel(DateOffset d)
            {
                this.d = d;
                d.PropertyChanged += d_PropertyChanged;
            }

            public int? Year
            {
                get { return d.Year; }
                set { d.Year = value; }
            }

            public int? Month
            {
                get { return d.Month; }
                set { d.Month = value; }
            }

            public int? Day
            {
                get { return d.Day; }
                set { d.Day = value; }
            }

            protected override void Dispose(bool disposing)
            {
                try
                {
                    if (disposing)
                    {
                        d.PropertyChanged -= d_PropertyChanged;
                    }
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }

            private void d_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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