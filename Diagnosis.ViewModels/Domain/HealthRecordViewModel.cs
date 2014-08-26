using Diagnosis.Core;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Diagnostics;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class HealthRecordViewModel : CheckableBase, IEditableNesting
    {
        internal readonly HealthRecord healthRecord;
        private DateOffset _dateOffset;
        private IEnumerable<Category> _categories;
        private ICommand _sendToSearch;
        private static ICategoryRepository catRepo = new CategoryRepository();

        #region IEditableNesting

        public Editable Editable { get; private set; }

        /// <summary>
        /// Запись пустая, если не задано ни одно свойство (кроме категории, которая всегда есть).
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return String.IsNullOrWhiteSpace(Comment)
                    && NumValue == null
                    && DateOffset.IsEmpty
                    && Diagnosis == null
                    && Symptom == null;
            }
        }

        #endregion IEditableNesting

        public string Name
        {
            get
            {
                if (Symptom != null)
                    return Symptom.Name;
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

        private SymptomViewModel _symptom;
        private DiagnosisViewModel _diagnosis;

        public string Comment
        {
            get
            {
                return healthRecord.Comment;
            }
            set
            {
                if (healthRecord.Comment != value)
                {
                    healthRecord.Comment = value;
                }
            }
        }

        public SymptomViewModel Symptom
        {
            get
            {
                return _symptom;
            }
            set
            {
                if (_symptom != value)
                {
                    _symptom = value;
                    if (value != null)
                        healthRecord.Symptom = value.symptom;
                    else
                        throw new ArgumentNullException("value", "Hr's symptom can not be set to null.");

                    OnPropertyChanged("Name");
                }
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
                if (healthRecord.Category != value)
                {
                    healthRecord.Category = value;
                }
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
                if (healthRecord.NumValue != value)
                {
                    healthRecord.NumValue = value;
                }
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
                if (healthRecord.FromYear != value)
                {
                    healthRecord.FromYear = value;
                    DateOffset.Year = value;
                }
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
                if (healthRecord.FromMonth != value)
                {
                    healthRecord.FromMonth = value.ConvertTo<int, byte>();
                    DateOffset.Month = value;
                }
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
                if (healthRecord.FromDay != value)
                {
                    healthRecord.FromDay = value.ConvertTo<int, byte>();
                    DateOffset.Day = value;
                }
            }
        }

        #endregion Model

        public DateOffset DateOffset
        {
            get
            {
                if (_dateOffset == null)
                {
                    _dateOffset = new DateOffset(FromYear, FromMonth, FromDay, () => healthRecord.Appointment.DateAndTime);
                    _dateOffset.PropertyChanged += (s, e) =>
                    {
                        switch (e.PropertyName)
                        {
                            case "Year":
                                FromYear = _dateOffset.Year;
                                break;

                            case "Month":
                                FromMonth = _dateOffset.Month;
                                break;

                            case "Day":
                                FromDay = _dateOffset.Day;
                                break;
                        }
                        OnPropertyChanged("DateOffset");
                    };
                }
                return _dateOffset;
            }
        }

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

        public IEnumerable<Category> Categories
        {
            get
            {
                if (_categories == null)
                {
                    _categories = new List<Category>(catRepo.GetAll().OrderBy(cat => cat.Order));
                }
                return _categories;
            }
        }

        public bool ShowDiagnosis
        {
            get
            {
                return Diagnosis != null && EntityProducers.DoctorsProducer.CurrentDoctor.doctor.DoctorSettings.HasFlag(DoctorSettings.ShowIcdDisease);
            }
        }
        public ICommand SendToSearchCommand
        {
            get
            {
                return _sendToSearch
                   ?? (_sendToSearch = new RelayCommand(() =>
                        {
                            this.Send(Events.SendToSearch, this.ToEnumerable().AsParams(MessageKeys.HealthRecords));
                        }));
            }
        }
        public HealthRecordViewModel(HealthRecord hr)
        {
            Contract.Requires(hr != null);
            this.healthRecord = hr;

            healthRecord.PropertyChanged += healthRecord_PropertyChanged;


            SetSymptom();
            SetDiagnosis();

            Editable = new Editable(healthRecord);
            this.Subscribe(Events.SettingsSaved, (e) =>
            {
                OnPropertyChanged("ShowDiagnosis");
            });
        }


        private void SetDiagnosis()
        {
            Diagnosis = EntityProducers.DiagnosisProducer.GetByDisease(healthRecord.Disease);
        }

        private void SetSymptom()
        {
            Symptom = EntityProducers.SymptomsProducer.GetByModel(healthRecord.Symptom);
        }

        private void healthRecord_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);

            switch (e.PropertyName)
            {
                case "FromDay":
                    DateOffset.Day = FromDay;
                    OnPropertyChanged("SortingDate");
                    break;

                case "FromMonth":
                    DateOffset.Month = FromMonth;
                    OnPropertyChanged("SortingDate");
                    break;

                case "FromYear":
                    DateOffset.Year = FromYear;
                    OnPropertyChanged("SortingDate");
                    break;

                case "Symptom":
                    SetSymptom();
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
    }
}