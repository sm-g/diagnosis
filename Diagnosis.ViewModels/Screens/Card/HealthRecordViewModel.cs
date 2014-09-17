using Diagnosis.Core;
using Diagnosis.Models;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;
using EventAggregator;

namespace Diagnosis.ViewModels
{
    public class HealthRecordViewModel : ViewModelBase
    {
        internal readonly HealthRecord healthRecord;
        EventMessageHandler handler;

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

        #region Model

        private DiagnosisViewModel _diagnosis;

        public string Comment
        {
            get
            {
                return healthRecord.Comment;
            }
            set
            {
                healthRecord.Comment = value;
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
            set
            {
                healthRecord.Category = value;
            }
        }

        public decimal? NumValue
        {
            get
            {
                return healthRecord.NumValue;
            }
            set
            {
                healthRecord.NumValue = value;
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
                healthRecord.FromMonth = value.ConvertTo<int, byte>();
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
                healthRecord.FromDay = value.ConvertTo<int, byte>();
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

        public HealthRecordViewModel(HealthRecord hr)
        {
            Contract.Requires(hr != null);
            this.healthRecord = hr;

            healthRecord.PropertyChanged += healthRecord_PropertyChanged;

            SetDiagnosis();

            handler = this.Subscribe(Events.SettingsSaved, (e) =>
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
                case "Symptom":
                    OnPropertyChanged("Name");
                    break;

                case "Disease":
                    SetDiagnosis();
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
                handler.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}