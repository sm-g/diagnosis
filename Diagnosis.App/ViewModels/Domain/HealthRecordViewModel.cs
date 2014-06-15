using Diagnosis.App.Messaging;
using Diagnosis.Core;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class HealthRecordViewModel : CheckableBase, IEditableNesting
    {
        internal readonly HealthRecord healthRecord;
        private DateOffset _dateOffset;
        private IEnumerable<Category> _categories;
        private List<EventMessageHandler> msgHandlers;
        private static bool makingCurrent;
        private ICategoryRepository catRepo;

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

        public override void OnSelectedChanged()
        {
            // check hr when select it and uncheck when selection goes away
            // except hr was checked by checkbox before
            if (!IsChecked || checkedBySelection)
            {
                checkedBySelection = IsSelected;
                IsChecked = IsSelected;
            }
        }

        public override void OnCheckedChanged()
        {
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

                    Editable.MarkDirty();
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

                    Editable.MarkDirty();
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

                    Editable.MarkDirty();
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
                    Editable.MarkDirty();
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
                    Editable.MarkDirty();
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
                    Editable.MarkDirty();
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
                    Editable.MarkDirty();
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
                    Editable.MarkDirty();
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

        public bool HasDiagnosis
        {
            get
            {
                return Diagnosis != null;
            }
        }
        public HealthRecordViewModel(HealthRecord hr)
        {
            Contract.Requires(hr != null);

            this.healthRecord = hr;
            catRepo = new CategoryRepository();

            healthRecord.PropertyChanged += healthRecord_PropertyChanged;

            Editable = new Editable(healthRecord, dirtImmunity: true, switchedOn: true);

            SetSymptom();
            SetDiagnosis();

            Editable.CanBeDirty = true;
        }

        public void UnsubscribeCheckedChanges()
        {
            foreach (var h in msgHandlers)
            {
                h.Dispose();
            }
        }

        internal void CheckInCurrent()
        {
            makingCurrent = true;
            EntityManagers.DiagnosisManager.Check(Diagnosis);
            makingCurrent = false;
        }

        private void SetDiagnosis()
        {
            Diagnosis = EntityManagers.DiagnosisManager.GetHealthRecordDiagnosis(healthRecord);
        }

        private void SetSymptom()
        {
            Symptom = EntityManagers.SymptomsManager.GetByModel(healthRecord.Symptom);
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
                    OnPropertyChanged("Name");
                    break;

                case "Disease":
                    SetDiagnosis();
                    OnPropertyChanged("Diagnosis");
                    OnPropertyChanged("HasDiagnosis");
                    OnPropertyChanged("Name");
                    break;

                default:
                    break;
            }
        }
        #region Event handlers

        internal void SubscribeToCheckedChanges()
        {
            msgHandlers = new List<EventMessageHandler>()
            {
                this.Subscribe((int)EventID.DiagnosisCheckedChanged, (e) =>
                {
                    if (this.IsSelected && !makingCurrent)
                    {
                        var diagnosis = e.GetValue<DiagnosisViewModel>(Messages.Diagnosis);

                        OnDiagnosisCheckedChanged(diagnosis, diagnosis.IsChecked);
                    }
                })
            };
        }

        private void OnDiagnosisCheckedChanged(DiagnosisViewModel diagnosisVM, bool isChecked)
        {
            if (isChecked)
            {
                this.Diagnosis = diagnosisVM;
                healthRecord.Disease = diagnosisVM.diagnosis.Disease;
            }
            else if (diagnosisVM == this.Diagnosis)
            {
                this.Diagnosis = null;
                healthRecord.Disease = null;
            }
        }

        #endregion Event handlers

        public override string ToString()
        {
            return healthRecord.ToString();
        }
    }
}