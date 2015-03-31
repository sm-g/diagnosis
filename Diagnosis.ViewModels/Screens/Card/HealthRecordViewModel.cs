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
        private DateOffsetViewModel _do;

        public HealthRecordViewModel(HealthRecord hr)
        {
            Contract.Requires(hr != null);
            this.healthRecord = hr;
            this.patient = hr.GetPatient();

            patient.PropertyChanged += patient_PropertyChanged;
            healthRecord.PropertyChanged += healthRecord_PropertyChanged;

            DateOffset = DateOffsetViewModel.FromHr(healthRecord);
            DateOffset.PropertyChanged += DateOffset_PropertyChanged;

            DateSuggestions = new ObservableCollection<DateSuggestion>();

            // показываем даты из записей этого же списка для быстрой вставки
            if (DateOffset.FirstSet == null)
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

        public int? FromYear
        {
            get
            {
                return healthRecord.FromYear;
            }
            set
            {
                healthRecord.FromYear = value;
            }
        }

        public int? FromMonth
        {
            get
            {
                return healthRecord.FromMonth;
            }
            set
            {
                healthRecord.FromMonth = value;
            }
        }

        public int? FromDay
        {
            get
            {
                return healthRecord.FromDay;
            }
            set
            {
                healthRecord.FromDay = value;
            }
        }

        public HealthRecordUnit Unit { get; set; }

        #endregion Model

        /// <summary>
        /// Откруглять давность.
        /// </summary>
        //public bool DoRound
        //{
        //    get
        //    {
        //        return _doRound;
        //    }
        //    set
        //    {
        //        if (_doRound != value)
        //        {
        //            _doRound = value;
        //            if (value)
        //            {
        //                DateOffset.Settings = DateOffset.DateOffsetSettings.Rounding();
        //            }
        //            else
        //            {
        //                DateOffset.Settings = DateOffset.DateOffsetSettings.ExactSetting();
        //            }
        //            OnPropertyChanged(() => DoRound);
        //        }
        //    }
        //}

        #region DateEditor

        public DateOffsetViewModel DateOffset
        {
            get
            {
                return _do;
            }
            set
            {
                if (_do != value)
                {
                    _do = value;
                    OnPropertyChanged(() => DateOffset);
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
                    healthRecord.Unit = DateOffset.RoundedUnit.ToHealthRecordUnit();
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
                var age = DateHelper.GetAge(patient.BirthYear, patient.BirthMonth, patient.BirthDay, DateOffset.GetSortingDate());
                if (age == null)
                    return null;
                var index = Plurals.GetPluralEnding(age.Value);
                return string.Format("в {0} {1}", age, Plurals.years[index]);
            }
        }

        public int? AtAge
        {
            get
            {
                return CanShowAsAge && DateOffset.Year.HasValue
                    ? DateOffset.Year.Value - patient.BirthYear.Value
                    : (int?)null;
            }
            set
            {
                DateOffset.FirstSet = DateOffsetViewModel.ShowAs.AtAge;

                // установка возраста меняет только год
                DateOffset.Year = patient.BirthYear.Value + value;

                OnPropertyChanged(() => AtAge);
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

        public ObservableCollection<DateSuggestion> DateSuggestions { get; private set; }

        private void DateOffset_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RoundedUnit" && ShowAsOffset)
            {
                healthRecord.Unit = DateOffset.RoundedUnit.ToHealthRecordUnit();
            }

            if (e.PropertyName == "FirstSet")
                // первая установка даты также ставит ShowAs
                switch (DateOffset.FirstSet)
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
                sourceDo = DateOffsetViewModel.FromHr(source).Do;
            }

            public DateOffset Do { get { return sourceDo; } }

            public RelayCommand SetDateCommand
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                        target.FromYear = source.FromYear;
                        target.FromMonth = source.FromMonth;
                        target.FromDay = source.FromDay;
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
                patient.PropertyChanged -= patient_PropertyChanged;
                DateOffset.PropertyChanged -= DateOffset_PropertyChanged;
                DateOffset = null; // unbind DataContext;
            }
            base.Dispose(disposing);
        }

        private void healthRecord_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);

            switch (e.PropertyName)
            {
                //case "FromDay":
                //case "FromMonth":

                case "Unit":
                    OnPropertyChanged(() => ShowAsAge);
                    OnPropertyChanged(() => ShowAsOffset);
                    OnPropertyChanged(() => ShowAsDate);
                    break;

                case "FromYear":
                    OnPropertyChanged(() => AtAgeString);
                    OnPropertyChanged(() => AtAge);
                    break;
            }
        }

        private void patient_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BirthYear")
            {
                OnPropertyChanged(() => CanShowAsAge);
                OnPropertyChanged(() => AtAgeString);
                OnPropertyChanged(() => AtAge);
            }
        }
    }
}