using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class ShortHealthRecordViewModel : CheckableBase
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(ShortHealthRecordViewModel));
        internal readonly HealthRecord healthRecord;
        private readonly Patient patient;
        private string _extra;

        public ShortHealthRecordViewModel(HealthRecord hr)
        {
            Contract.Requires(hr != null);
            this.healthRecord = hr;

            patient = hr.GetPatient();
            patient.PropertyChanged += patient_PropertyChanged;

            healthRecord.PropertyChanged += healthRecord_PropertyChanged;
            healthRecord.ItemsChanged += healthRecord_ItemsChanged;
        }

        /// <summary>
        /// For XAML designer
        /// </summary>
        [Obsolete]
        public ShortHealthRecordViewModel()
        {
            if (!IsInDesignMode) return;

            var pat = new Patient();
            var doc = new Doctor("Ivanov");
            var holder = new Course(pat, doc);
            var hr = new HealthRecord(holder, doc)
            {
                Category = new HrCategory() { Name = "жалоба" },
                FromMonth = 5,
            };
            hr.AddItems(new IHrItemObject[] { new Word("анемия"), new Word("впервые"), new Comment("без осложнений") });

            healthRecord = hr;
            SortingExtraInfo = hr.Category.Name;
        }

        public string Name
        {
            get
            {
                return string.Join(" ", healthRecord.GetOrderedEntities());// сущности давления надо форматировать
            }
        }

        #region CheckableBase

        private bool checkedBySelection;

        protected override void OnSelectedChanged()
        {
            base.OnSelectedChanged();

            // check hr when select it and uncheck when selection goes away
            // except hr was checked by checkbox before
            if (!IsChecked || checkedBySelection)
            {
                checkedBySelection = IsSelected;
                IsChecked = IsSelected;
            }
        }

        protected override void OnCheckedChanged()
        {
            base.OnCheckedChanged();

            // убираем выделение при снятии флажка
            IsSelected = IsChecked;
        }

        #endregion CheckableBase

        #region Model

        public HrCategory Category
        {
            get
            {
                return healthRecord.Category;
            }
        }

        public int? FromYear
        {
            get
            {
                return healthRecord.FromYear;
            }
        }

        public int? FromMonth
        {
            get
            {
                return healthRecord.FromMonth;
            }
        }

        public int? FromDay
        {
            get
            {
                return healthRecord.FromDay;
            }
        }

        public DateOffset DateOffset
        {
            get
            {
                return healthRecord.DateOffset;
            }
        }

        public Doctor Doctor
        {
            get { return healthRecord.Doctor; }
        }

        public DateTime CreatedAt
        {
            get { return healthRecord.CreatedAt; }
        }

        public int Ord
        {
            get { return healthRecord.Ord; }
        }

        public bool IsDeleted
        {
            get { return healthRecord.IsDeleted; }
        }

        #endregion Model

        public DateTime SortingDate
        {
            get { return DateOffset.GetSortingDate(); }
        }

        public string GroupingDate
        {
            get
            {
                string res = "";
                return res;
            }
        }
        public string GroupingCreatedAt
        {
            get
            {
                string res;
                var span = (DateTime.Today - CreatedAt).Days;
                if (span < 1)
                    res = "сегодня";
                else if (span < 2)
                    res = "вчера";
                else if (span < 7)
                    res = "за неделю";
                else if (span < 30)
                    res = "за последний месяц";
                else
                    res = "давно";
                return res;
            }
        }
        public string SortingExtraInfo
        {
            get
            {
                return _extra;
            }
            set
            {
                if (_extra != value)
                {
                    _extra = value;
                    OnPropertyChanged(() => SortingExtraInfo);
                }
            }
        }

        public string DateOffsetString
        {
            get
            {
                switch (healthRecord.Unit)
                {
                    case HealthRecordUnit.NotSet:
                        return DateOffsetFormatter.GetPartialDateString(DateOffset);

                    case HealthRecordUnit.ByAge:
                        var pat = healthRecord.GetPatient();
                        var age = DateHelper.GetAge(pat.BirthYear, pat.BirthMonth, pat.BirthDay, DateOffset.GetSortingDate());
                        var index = Plurals.GetPluralEnding(age.Value);
                        return string.Format("в {0} {1}", age, Plurals.years[index]);

                    default:
                        return DateOffsetFormatter.GetOffsetUnitString(DateOffset);
                }
            }
        }

        #region Commands

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

        public RelayCommand OpenCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Event.EditHealthRecord, new object[] { healthRecord, false }.AsParams(MessageKeys.HealthRecord, MessageKeys.Boolean));
                });
            }
        }

        public RelayCommand DeleteCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    healthRecord.IsDeleted = true;
                });
            }
        }

        #endregion Commands

        private void healthRecord_ItemsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Name");
        }

        private void healthRecord_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);

            switch (e.PropertyName)
            {
                case "FromDay":
                case "FromMonth":
                case "FromYear":
                case "Unit":
                    OnPropertyChanged("SortingDate");
                    OnPropertyChanged("DateOffsetString");
                    break;

                case "HrItems":
                    OnPropertyChanged("Name");
                    break;
            }
        }

        private void patient_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "BirthDay":
                case "BirthMonth":
                case "BirthYear":
                    OnPropertyChanged("DateOffsetString");
                    break;
            }
        }

        public override string ToString()
        {
            return string.Format("short {0}", healthRecord);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                healthRecord.PropertyChanged -= healthRecord_PropertyChanged;
                healthRecord.ItemsChanged -= healthRecord_ItemsChanged;
                patient.PropertyChanged -= patient_PropertyChanged;
            }
            base.Dispose(disposing);
        }
    }
}