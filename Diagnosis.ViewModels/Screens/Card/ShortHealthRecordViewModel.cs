using Diagnosis.Core;
using Diagnosis.Models;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class ShortHealthRecordViewModel : CheckableBase
    {
        internal readonly HealthRecord healthRecord;

        public string Name
        {
            get
            {
                if (Symptom != null)
                    return (Symptom.Disease != null ? Symptom.Disease.Code + ". " : "") +
                    string.Join(" ", Symptom.Words.OrderBy(w => w.Priority).Select(w => w.Title));
                else
                    return "";
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

        private DiagnosisViewModel _diagnosis;

        public string Comment
        {
            get
            {
                return healthRecord.Comment;
            }
        }

        public Symptom Symptom
        {
            get
            {
                return healthRecord.Symptom;
            }
        }

        public DiagnosisViewModel Diagnosis
        {
            get
            {
                return _diagnosis;
            }
            set
            {
                if (_diagnosis != value)
                {
                    _diagnosis = value;
                    if (value != null)
                    {
                        healthRecord.Disease = value.diagnosis.Disease;
                    }
                    else
                    {
                        healthRecord.Disease = null;
                    }

                    OnPropertyChanged("Diagnosis");
                    OnPropertyChanged("ShowDiagnosis");
                    OnPropertyChanged("Name");
                }
            }
        }

        public Category Category
        {
            get
            {
                return healthRecord.Category;
            }
        }

        public decimal? NumValue
        {
            get
            {
                return healthRecord.NumValue;
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

        public bool ShowDiagnosis
        {
            get
            {
                return Diagnosis != null && AuthorityController.CurrentDoctor.DoctorSettings.HasFlag(DoctorSettings.ShowIcdDisease);
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

        public ShortHealthRecordViewModel(HealthRecord hr)
        {
            Contract.Requires(hr != null);
            this.healthRecord = hr;

            healthRecord.PropertyChanged += healthRecord_PropertyChanged;

            SetDiagnosis();

            this.Subscribe(Events.SettingsSaved, (e) =>
            {
                OnPropertyChanged("ShowDiagnosis");
            });
        }

        private void SetDiagnosis()
        {
            Diagnosis = EntityProducers.DiagnosisProducer.GetByDisease(healthRecord.Disease);
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

                case "Symptom":
                    OnPropertyChanged("Name");
                    break;

                case "Disease":
                    SetDiagnosis();
                    break;

                default:
                    break;
            }
        }

        public override string ToString()
        {
            return healthRecord.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                healthRecord.PropertyChanged -= healthRecord_PropertyChanged;
            }
            base.Dispose(disposing);
        }
    }
}