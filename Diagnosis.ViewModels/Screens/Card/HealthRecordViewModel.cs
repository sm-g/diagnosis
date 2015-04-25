using Diagnosis.Common;
using Diagnosis.Models;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class HealthRecordViewModel : ViewModelBase
    {
        internal readonly HealthRecord healthRecord;
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HealthRecordViewModel));
        private static bool _isExpanded;
        private readonly Patient patient;
        private DateOffsetViewModel _doVm;

        public HealthRecordViewModel(HealthRecord hr)
        {
            Contract.Requires(hr != null);
            this.healthRecord = hr;
            this.patient = hr.GetPatient();

            healthRecord.PropertyChanged += healthRecord_PropertyChanged;

            EventDate = DateOffsetViewModel.FromHr(healthRecord);
            EventDate.PropertyChanged += DateOffset_PropertyChanged;
            DateSuggestions = new ObservableCollection<DateSuggestion>();
            IsIntervalEditorOpened = hr.FromDate != hr.ToDate;

            // в редакторе для даты-точки нет конца интервала
            if (hr.FromDate == hr.ToDate)
                hr.ToDate.Clear();

            // показываем даты из записей этого же списка для быстрой вставки
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

        #region Model

        public HrCategory Category
        {
            get
            {
                return healthRecord.Category;
            }
            set
            {
                healthRecord.Category = value;
            }
        }

        public HealthRecordUnit Unit { get; set; }

        #endregion Model


        #region DateEditor

        private bool _interaval;

        private DateOffset lastToDate;

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

                        // bind after
                        EventDate.To = new DateOffsetViewModel.DatePickerViewModel(EventDate);
                        EventDate.OpenedInEditor = true;
                    }
                    else
                    {
                        lastToDate = new DateOffset(healthRecord.ToDate);
                        EventDate.To = null; // unbind ComboboxDatePicker DataContext;
                        EventDate.OpenedInEditor = false;

                        healthRecord.ToDate.FillDateFrom(healthRecord.FromDate);
                    }
                    OnPropertyChanged(() => IsIntervalEditorOpened);
                }
            }
        }

        public ObservableCollection<DateSuggestion> DateSuggestions { get; private set; }

        private void DateOffset_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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

        #endregion DateEditor

        public ICommand SendToSearchCommand
        {
            get
            {
                return new RelayCommand(() =>
                        {
                            this.Send(Event.SendToSearch, healthRecord.ToEnumerable().AsParams(MessageKeys.HealthRecords));
                        });
            }
        }

        public RelayCommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                       {
                           this.Send(Event.EditHealthRecord, new object[] { healthRecord, true }.AsParams(MessageKeys.HealthRecord, MessageKeys.Boolean));
                       });
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", GetType().Name, healthRecord);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                healthRecord.PropertyChanged -= healthRecord_PropertyChanged;
                EventDate.PropertyChanged -= DateOffset_PropertyChanged;
                EventDate = null; // unbind ComboboxDatePicker DataContext;

                // редактор интервала закрыт - дата-точка
                if (!IsIntervalEditorOpened)
                    healthRecord.ToDate.FillDateFrom(healthRecord.FromDate);

            }
            base.Dispose(disposing);
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
    }
}