using Diagnosis.Common;
using Diagnosis.Models;
using System.Diagnostics.Contracts;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class HealthRecordViewModel : ViewModelBase
    {
        internal readonly HealthRecord healthRecord;
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HealthRecordViewModel));
        private readonly Patient patient;
        private static bool _isExpanded;
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

        void DateOffset_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RoundedUnit" && ShowAsOffset)
            {
                healthRecord.Unit = DateOffset.RoundedUnit.ToHealthRecordUnit();
            }
        }
        #endregion



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