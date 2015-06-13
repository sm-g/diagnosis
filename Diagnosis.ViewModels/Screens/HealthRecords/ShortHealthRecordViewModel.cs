using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.Models.Enums;
using Diagnosis.ViewModels.Controls;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public partial class ShortHealthRecordViewModel : CheckableBase, IFocusable
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(ShortHealthRecordViewModel));
        internal readonly HealthRecord healthRecord;
        private readonly Patient patient;
        private bool _focused;
        private string _extra;
        private bool _draggable;

        public ShortHealthRecordViewModel(HealthRecord hr)
        {
            Contract.Requires(hr != null);
            this.healthRecord = hr;

            patient = hr.GetPatient();
            patient.PropertyChanged += patient_PropertyChanged;

            healthRecord.PropertyChanged += healthRecord_PropertyChanged;
            healthRecord.ItemsChanged += healthRecord_ItemsChanged;

            SyncCheckedAndSelected = true;
            EventDate = EventDateViewModel.FromHr(healthRecord);
            EventDate.PropertyChanged += (s, e) =>
            {
                OnPropertyChanged(() => EventDateString);
            };
            DropHandler = new DropTargetHandler(this);
            IsDropTargetEnabled = true;
        }

        public string Name
        {
            get
            {
                return string.Join(", ", healthRecord.GetOrderedCHIOs());
            }
        }

        #region Model

        public HrCategory Category
        {
            get
            {
                return healthRecord.Category ?? HrCategory.Null;
            }
        }

        public EventDateViewModel EventDate
        {
            get;
            private set;
        }

        public Doctor Doctor
        {
            get { return healthRecord.Doctor; }
        }

        public DateTime DescribedAt
        {
            get { return healthRecord.DescribedAt; }
        }

        public DateTime CreatedAt
        {
            get { return healthRecord.CreatedAt; }
        }

        public DateTime UpdatedAt
        {
            get { return healthRecord.UpdatedAt; }
        }

        public int Ord
        {
            get { return healthRecord.Ord; }
            set
            {
#if DEBUG
                healthRecord.Ord = value;
#endif
            }
        }

        public bool IsDeleted
        {
            get { return healthRecord.IsDeleted; }
        }

        public bool IsFocused
        {
            get
            {
                return _focused;
            }
            set
            {
                if (_focused != value)
                {
                    //logger.DebugFormat("focused {0} {1}", this, value);
                    _focused = value;
                    OnPropertyChanged(() => IsFocused);
                }
            }
        }

        #endregion Model

        public DateTime SortingDate
        {
            get { return healthRecord.FromDate.GetSortingDate(); }
        }

        public HrCreatedAtOffset GroupingCreatedAt
        {
            get
            {
                return new HrCreatedAtOffset(CreatedAt);
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

        public string EventDateString
        {
            get
            {
                switch (healthRecord.Unit)
                {
                    case HealthRecordUnit.NotSet:
                        return EventDate.PartialDateString;

                    case HealthRecordUnit.ByAge:
                        return EventDate.AtAgeString;

                    default:
                        return string.Format("{0}{1} {2}{3}",
                            EventDate.IsOpenedInterval ? "уже " : string.Empty,
                            EventDate.RoundedOffset,
                            DateOffsetFormatter.GetUnitString(EventDate.RoundedOffset, EventDate.RoundedUnit),
                            EventDate.IsPoint ? " назад" : string.Empty);
                }
            }
        }

        public bool IsDraggable
        {
            get
            {
                return _draggable;
            }
            set
            {
                if (_draggable != value)
                {
                    _draggable = value;
                    OnPropertyChanged(() => IsDraggable);
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
                            this.Send(Event.SendToSearch, healthRecord.ToEnumerable().AsParams(MessageKeys.ToSearchPackage));
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
            OnPropertyChanged(() => Name);
        }

        private void healthRecord_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);

            switch (e.PropertyName)
            {
                case "FromDate":
                case "ToDate":
                case "Unit":
                    OnPropertyChanged(() => SortingDate);
                    OnPropertyChanged(() => EventDateString);
                    break;

                case "HrItems":
                    OnPropertyChanged(() => Name);
                    break;

                case "CreatedAt":
                    OnPropertyChanged(() => GroupingCreatedAt);
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
                    OnPropertyChanged(() => EventDateString);
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