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
        private bool _doRound;

        public HealthRecordViewModel(HealthRecord hr)
        {
            Contract.Requires(hr != null);
            this.healthRecord = hr;
            patient = hr.GetPatient();
            patient.PropertyChanged += patient_PropertyChanged;
            healthRecord.PropertyChanged += healthRecord_PropertyChanged;
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

        public DateOffset DateOffset
        {
            get
            {
                return healthRecord.DateOffset;
            }
        }

        #endregion Model

        /// <summary>
        /// Откруглять давность.
        /// </summary>
        public bool DoRound
        {
            get
            {
                return _doRound;
            }
            set
            {
                if (_doRound != value)
                {
                    _doRound = value;
                    if (value)
                    {
                        DateOffset.Settings = DateOffset.DateOffsetSettings.Rounding();
                    }
                    else
                    {
                        DateOffset.Settings = DateOffset.DateOffsetSettings.ExactSetting();
                    }
                    OnPropertyChanged(() => DoRound);
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
                    healthRecord.Unit = DateOffset.Unit.ToHealthRecordUnit();
            }
        }

        public bool CanShowAsAge
        {
            get
            {
                return healthRecord.GetPatient().BirthYear.HasValue;
            }
        }

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
            }
            base.Dispose(disposing);
        }

        private void healthRecord_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            if (e.PropertyName == "Unit")
            {
                OnPropertyChanged(() => ShowAsAge);
                OnPropertyChanged(() => ShowAsOffset);
                OnPropertyChanged(() => ShowAsDate);
            }
        }

        private void patient_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BirthYear")
            {
                OnPropertyChanged(() => CanShowAsAge);
            }
        }
    }
}