﻿using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.Models.Enums;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public partial class ShortHealthRecordViewModel : CheckableBase
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
            DateOffset = DateOffsetViewModel.FromHr(healthRecord);
            DateOffset.PropertyChanged += (s, e) =>
            {
                OnPropertyChanged(() => DateOffsetString);
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

        public DateOffsetViewModel DateOffset
        {
            get;
            private set;
        }

        public Doctor Doctor
        {
            get { return healthRecord.Doctor; }
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
                    logger.DebugFormat("focused {0} {1}", this, value);
                    _focused = value;
                    OnPropertyChanged(() => IsFocused);
                }
            }
        }

        #endregion Model

        public DateTime SortingDate
        {
            get { return DateOffset.GetSortingDate(); }
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

        public string DateOffsetString
        {
            get
            {
                switch (healthRecord.Unit)
                {
                    case HealthRecordUnit.NotSet:
                        return DateOffsetFormatter.GetPartialDateString(DateOffset.Do);

                    case HealthRecordUnit.ByAge:
                        var age = DateHelper.GetAge(patient.BirthYear, patient.BirthMonth, patient.BirthDay, DateOffset.GetSortingDate());
                        if (age == null)
                            return null;
                        var index = Plurals.GetPluralEnding(age.Value);
                        return string.Format("в {0} {1}", age, Plurals.years[index]);

                    default:
                        return string.Format("{0} {1}",
                            DateOffset.RoundedOffset,
                            DateOffsetFormatter.GetUnitString(DateOffset.RoundedOffset, DateOffset.RoundedUnit));
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
            OnPropertyChanged(() => Name);
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
                    OnPropertyChanged(() => SortingDate);
                    OnPropertyChanged(() => DateOffsetString);
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
                    OnPropertyChanged(() => DateOffsetString);
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