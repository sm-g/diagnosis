using Diagnosis.App.Messaging;
using Diagnosis.Core;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class HealthRecordViewModel : CheckableBase, IEditableNesting
    {
        internal HealthRecord healthRecord;
        private DateOffset _dateOffset;
        private List<EventMessageHandler> msgHandlers;

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
                    return string.Empty;
            }
        }

        #region CheckableBase

        private bool checkedBySelection;

        public override void OnSelectedChanged()
        {
            // check hr when select it and uncheck when selection goes away
            // except hr was checked by user before
            if (!IsChecked || checkedBySelection)
            {
                checkedBySelection = IsSelected;
                IsChecked = IsSelected;
            }
        }

        #endregion CheckableBase

        #region Model

        private SymptomViewModel _symptom;
        private CategoryViewModel _category;
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
                    OnPropertyChanged("Comment");
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

                    Editable.MarkDirty();
                    OnPropertyChanged("Symptom");
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

                    Editable.MarkDirty();
                    OnPropertyChanged("Diagnosis");
                    OnPropertyChanged("HasDiagnosis");
                }
            }
        }

        public CategoryViewModel Category
        {
            get
            {
                return _category;
            }
            set
            {
                if (_category != value)
                {
                    if (value != null && value != CategoryManager.NoCategory)
                    {
                        healthRecord.Category = value.category;
                        _category = value;
                    }
                    else
                    {
                        healthRecord.Category = null;
                        _category = CategoryManager.NoCategory;
                    }

                    Editable.MarkDirty();
                    OnPropertyChanged("Category");
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
                    OnPropertyChanged("NumValue");
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

                    OnPropertyChanged("FromYear");
                    OnPropertyChanged("SortingDate");
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

                    OnPropertyChanged("FromMonth");
                    OnPropertyChanged("SortingDate");
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

                    OnPropertyChanged("FromDay");
                    OnPropertyChanged("SortingDate");
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

        public ObservableCollection<CategoryViewModel> Categories
        {
            get
            {
                return EntityManagers.CategoryManager.Categories;
            }
        }

        public bool HasDiagnosis
        {
            get
            {
                return Diagnosis != null;
            }
        }

        private static bool makingCurrent;
        private static HealthRecordViewModel currentHr;

        private void MakeCurrent()
        {
            if (currentHr != null && currentHr != this)
            {
                // новая выбранная запись в том же приеме
                if (currentHr.healthRecord.Appointment == this.healthRecord.Appointment)
                {
                    // оставляем редактор открытым при смене выбранной записи
                    this.Editable.IsEditorActive = currentHr.Editable.IsEditorActive;

                    if (!currentHr.Editable.IsDirty)
                    {
                        currentHr.Editable.IsEditorActive = false;
                    }

                    // сохраняем запись
                    currentHr.Editable.Commit();
                }
                currentHr.UnsubscribeCheckedChanges();
            }
            if (currentHr != this)
            {
                this.SubscribeToCheckedChanges();
            }

            currentHr = this;

            CheckInCurrent();
        }

        private void CheckInCurrent()
        {
            makingCurrent = true;
            EntityManagers.DiagnosisManager.Check(Diagnosis);
            makingCurrent = false;
        }

        public HealthRecordViewModel(HealthRecord hr)
        {
            Contract.Requires(hr != null);

            this.healthRecord = hr;

            Editable = new Editable(this, dirtImmunity: true, switchedOn: true);

            Category = EntityManagers.CategoryManager.GetByModel(hr.Category) ?? EntityManagers.CategoryManager.Default;
            Symptom = EntityManagers.SymptomsManager.Symptoms.FirstOrDefault(s => s.symptom == hr.Symptom);
            Diagnosis = EntityManagers.DiagnosisManager.GetHealthRecordDiagnosis(healthRecord);

            Editable.CanBeDirty = true;

            Subscribe();
        }

        public void UnsubscribeCheckedChanges()
        {
            foreach (var h in msgHandlers)
            {
                h.Dispose();
            }
        }

        #region Event handlers

        private void Subscribe()
        {
            this.PropertyChanged += HealthRecordViewModel_PropertyChanged;
        }

        private void SubscribeToCheckedChanges()
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

        private void HealthRecordViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                if (IsSelected)
                    MakeCurrent();
            }
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

            OnPropertyChanged("Name");
        }

        #endregion Event handlers

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} {4} {5}", Category, Symptom, NumValue != null ? NumValue.ToString() : "", Diagnosis, DateOffset, Comment);
        }
    }
}