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

        public string Comment
        {
            get
            {
                return healthRecord.Comment;
            }
        }

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

        #endregion Model

        public DateTime SortingDate
        {
            get
            {
                int year = FromYear ?? 1;
                int month = FromMonth ?? 1;
                int day = FromDay ?? 1;
                return new DateTime(year, month, day);
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

        public ShortHealthRecordViewModel(HealthRecord hr)
        {
            Contract.Requires(hr != null);
            this.healthRecord = hr;

            healthRecord.PropertyChanged += healthRecord_PropertyChanged;
            healthRecord.ItemsChanged += healthRecord_ItemsChanged;
            healthRecord.DateOffset.PropertyChanged += DateOffset_PropertyChanged;
        }

        private void healthRecord_ItemsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Name");
        }

        private void DateOffset_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Unit")
            {
                OnPropertyChanged("DateOffset"); // for converters binding
            }
        }

        private void healthRecord_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);

            switch (e.PropertyName)
            {
                case "FromDay":
                case "FromMonth":
                case "FromYear":
                    OnPropertyChanged("SortingDate");
                    break;
                case "HrItems":
                    OnPropertyChanged("Name");
                    break;

                default:
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
                healthRecord.DateOffset.PropertyChanged -= DateOffset_PropertyChanged;
            }
            base.Dispose(disposing);
        }
    }
}