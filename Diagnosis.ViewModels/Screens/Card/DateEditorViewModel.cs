using Diagnosis.Common;
using Diagnosis.Models;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class DateEditorViewModel : ViewModelBase
    {
        internal readonly HealthRecord healthRecord;
        private bool _interaval;
        private DateOffsetViewModel _doVm;

        private DateOffset lastToDate;
        private bool _isExpanded;

        public DateEditorViewModel(HealthRecord hr)
        {
            Contract.Requires(hr != null);
            this.healthRecord = hr;
            hr.FromDate.PropertyChanged += FromDate_PropertyChanged;

            healthRecord.PropertyChanged += healthRecord_PropertyChanged;

            EventDate = DateOffsetViewModel.FromHr(healthRecord);
            EventDate.PropertyChanged += EventDateVm_PropertyChanged;
            DateSuggestions = new ObservableCollection<DateSuggestion>();

            IsIntervalEditorOpened = hr.FromDate != hr.ToDate;

            // для даты-точки нет конца интервала
            if (hr.FromDate == hr.ToDate)
                hr.ToDate.Clear();

            SetupDateSuggsetions();
        }

        public DateOffsetViewModel EventDate
        {
            get
            {
                return _doVm;
            }
            set
            {
                if (_doVm != value)
                {
                    _doVm = value;
                    OnPropertyChanged(() => EventDate);
                }
            }
        }

        public bool ShowAsDate
        {
            get
            {
                return healthRecord.Unit == HealthRecordUnit.NotSet;
            }
            set
            {
                if (value)
                    healthRecord.Unit = HealthRecordUnit.NotSet;
            }
        }

        public bool ShowAsAge
        {
            get
            {
                return healthRecord.Unit == HealthRecordUnit.ByAge;
            }
            set
            {
                if (value)
                    healthRecord.Unit = HealthRecordUnit.ByAge;
            }
        }

        public bool ShowAsOffset
        {
            get
            {
                return healthRecord.Unit != HealthRecordUnit.ByAge
                    && healthRecord.Unit != HealthRecordUnit.NotSet;
            }
            set
            {
                if (value)
                    healthRecord.Unit = EventDate.RoundedUnit.ToHealthRecordUnit();
            }
        }

        public bool IsDateEditorExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(() => IsDateEditorExpanded);
                }
            }
        }

        /// <summary>
        /// Показывать редактор второй даты.
        /// Пока запись редактируется, оcтается введенная дата.
        /// </summary>
        public bool IsIntervalEditorOpened
        {
            get
            {
                return _interaval;
            }
            set
            {
                if (_interaval != value)
                {
                    _interaval = value;
                    if (value)
                    {
                        // открыли редактор второй даты - в нем дата, введенная до закрытия
                        if (lastToDate != null)
                            healthRecord.ToDate.FillDateFrom(lastToDate);
                        else
                            healthRecord.ToDate.Clear();

                        EventDate.OpenedInIntervalEditor = true;
                    }
                    else
                    {
                        lastToDate = new DateOffset(healthRecord.ToDate);
                        EventDate.OpenedInIntervalEditor = false;

                        healthRecord.ToDate.FillDateFrom(healthRecord.FromDate);
                    }
                    OnPropertyChanged(() => IsIntervalEditorOpened);
                }
            }
        }

        public ObservableCollection<DateSuggestion> DateSuggestions { get; private set; }

        /// <summary>
        /// Для события-точки обе даты меняем вместе при закрытом редакторе интервала.
        /// </summary>
        private bool SetToWithFrom { get { return !IsIntervalEditorOpened && healthRecord.IsPoint; } }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                healthRecord.PropertyChanged -= healthRecord_PropertyChanged;
                healthRecord.FromDate.PropertyChanged -= FromDate_PropertyChanged;
                EventDate.PropertyChanged -= EventDateVm_PropertyChanged;

                EventDate = null; // unbind ComboboxDatePicker DataContext;

                // редактор интервала закрыт - дата-точка
                if (!IsIntervalEditorOpened)
                    healthRecord.ToDate.FillDateFrom(healthRecord.FromDate);
            }
            base.Dispose(disposing);
        }

        private void FromDate_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (SetToWithFrom)
                switch (e.PropertyName)
                {
                    case "Day":
                        healthRecord.ToDate.Day = healthRecord.FromDate.Day;
                        break;

                    case "Month":

                        healthRecord.ToDate.Month = healthRecord.FromDate.Month;
                        break;

                    case "Year":
                        healthRecord.ToDate.Year = healthRecord.FromDate.Year;
                        break;
                }
        }

        /// <summary>
        /// Показываем даты из записей этого же списка для быстрой вставки
        /// </summary>
        private void SetupDateSuggsetions()
        {
            if (EventDate.FirstSet == null)
            {
                var holderHrsWithDates = healthRecord.Holder.HealthRecords
                    .Where(x => x.Unit == HealthRecordUnit.NotSet && !x.IsDeleted);
                var ds = holderHrsWithDates.Select(x => new DateSuggestion(x, healthRecord))
                    .Where(x => !x.Do.IsEmpty)
                    .DistinctBy(x => x.Do)
                    .OrderBy(x => x.Do)
                    .Take(5);
                DateSuggestions.SyncWith(ds);
            }
        }

        private void EventDateVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RoundedUnit" && ShowAsOffset)
            {
                healthRecord.Unit = EventDate.RoundedUnit.ToHealthRecordUnit();
            }

            if (e.PropertyName == "FirstSet")
                // первая установка даты также ставит ShowAs
                switch (EventDate.FirstSet)
                {
                    default:
                    case DateOffsetViewModel.ShowAs.Date:
                        ShowAsDate = true;
                        break;

                    case DateOffsetViewModel.ShowAs.Offset:
                        ShowAsOffset = true;
                        break;

                    case DateOffsetViewModel.ShowAs.AtAge:
                        ShowAsAge = true;
                        break;
                }
        }

        private void healthRecord_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);

            switch (e.PropertyName)
            {
                case "Unit":
                    OnPropertyChanged(() => ShowAsAge);
                    OnPropertyChanged(() => ShowAsOffset);
                    OnPropertyChanged(() => ShowAsDate);
                    break;
            }
        }

        public class DateSuggestion : ViewModelBase
        {
            private HealthRecord target;
            private HealthRecord source;
            private DateOffset sourceDo;

            public DateSuggestion(HealthRecord source, HealthRecord target)
            {
                this.source = source;
                this.target = target;
                sourceDo = source.FromDate;
            }

            public DateOffset Do { get { return sourceDo; } }

            public RelayCommand SetDateCommand
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                        target.FromDate.Year = source.FromDate.Year;
                        target.FromDate.Month = source.FromDate.Month;
                        target.FromDate.Day = source.FromDate.Day;
                    });
                }
            }

            public override string ToString()
            {
                return DateOffsetFormatter.GetPartialDateString(sourceDo);
            }
        }
    }
}