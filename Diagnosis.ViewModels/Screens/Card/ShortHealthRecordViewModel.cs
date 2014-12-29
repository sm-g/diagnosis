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
        internal readonly HealthRecord healthRecord;
        private readonly Patient patient;

        public ShortHealthRecordViewModel(HealthRecord hr)
        {
            Contract.Requires(hr != null);
            this.healthRecord = hr;

            patient = hr.GetPatient();
            patient.PropertyChanged += patient_PropertyChanged;

            healthRecord.PropertyChanged += healthRecord_PropertyChanged;
            healthRecord.ItemsChanged += healthRecord_ItemsChanged;
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

        #endregion Model

        public DateTime SortingDate
        {
            get
            {
                return DateOffset.GetSortingDate();
            }
        }

        public string DateOffsetString
        {
            get
            {
                switch (healthRecord.Unit)
                {
                    case HealthRecordUnits.NotSet:
                        return DateOffsetFormatter.GetPartialDateString(DateOffset);

                    case HealthRecordUnits.ByAge:
                        var pat = healthRecord.GetPatient();
                        var age = DateHelper.GetAge(pat.BirthYear, pat.BirthMonth, pat.BirthDay, DateOffset.GetSortingDate());
                        var index = Plurals.GetPluralEnding(age.Value);
                        return string.Format("{0} {1}", age, Plurals.years[index]);
                    default:
                        return DateOffsetFormatter.GetOffsetUnitString(DateOffset);
                }
            }
        }

        public ICommand SendToSearchCommand
        {
            get
            {
                return new RelayCommand(() =>
                        {
                            this.Send(Events.SendToSearch, healthRecord.ToEnumerable().AsParams(MessageKeys.HealthRecords));
                        });
            }
        }

        public RelayCommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                       {
                           this.Send(Events.EditHealthRecord, healthRecord.AsParams(MessageKeys.HealthRecord));
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
            return string.Format("{0} {1}", GetType().Name, healthRecord);
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