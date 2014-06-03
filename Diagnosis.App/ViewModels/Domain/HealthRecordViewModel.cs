﻿using Diagnosis.App.Messaging;
using Diagnosis.Core;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class HealthRecordViewModel : CheckableBase, IEditableNesting
    {
        internal readonly HealthRecord healthRecord;
        private AutoCompleteBoxViewModel _autoComplete;
        private AutoCompleteBase<WordViewModel> _autoComplete2;
        private PopupSearch<DiagnosisViewModel> _diagnosisSearch;
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
                    OnPropertyChanged(() => Comment);
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
                    OnPropertyChanged(() => Symptom);
                    OnPropertyChanged(() => Name);
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
                    OnPropertyChanged(() => Diagnosis);
                    OnPropertyChanged(() => HasDiagnosis);
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
                    OnPropertyChanged(() => Category);
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
                    OnPropertyChanged(() => NumValue);
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

                    OnPropertyChanged(() => FromYear);
                    OnPropertyChanged(() => SortingDate);
                    OnPropertyChanged(() => Date);
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

                    OnPropertyChanged(() => FromMonth);
                    OnPropertyChanged(() => SortingDate);
                    OnPropertyChanged(() => Date);
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

                    OnPropertyChanged(() => FromDay);
                    OnPropertyChanged(() => SortingDate);
                    OnPropertyChanged(() => Date);
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

        public string Date
        {
            get
            {
                string d = FromDay != null ? FromDay.ToString() + "." : "";
                d += FromMonth != null ? FromMonth.ToString() + "." : "";
                d += FromYear != null ? FromYear.ToString() : "";
                return d;
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

        public AutoCompleteBoxViewModel AutoComplete
        {
            get
            {
                return _autoComplete ?? (_autoComplete = new AutoCompleteBoxViewModel(
                    QuerySeparator.Default));
            }
        }

        public AutoCompleteBase<WordViewModel> AutoComplete2
        {
            get
            {
                return _autoComplete2 ?? (_autoComplete2 = new WordCompositeAutoComplete(
                    QuerySeparator.Default,
                    new SimpleSearcherSettings() { AllChildren = true }));
            }
        }

        public PopupSearch<DiagnosisViewModel> DiagnosisSearch
        {
            get
            {
                return _diagnosisSearch;
            }
            set
            {
                if (_diagnosisSearch != value)
                {
                    _diagnosisSearch = value;
                    OnPropertyChanged(() => DiagnosisSearch);
                }
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

                    // сохраняем запись
                    currentHr.Editable.CommitCommand.Execute(null);
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
            if (Symptom != null)
                EntityManagers.WordsManager.CheckThese(Symptom.Words);
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
            CreateDiagnosisSearch();
        }

        public void UnsubscribeCheckedChanges()
        {
            foreach (var h in msgHandlers)
            {
                h.Dispose();
            }
        }

        private void CreateDiagnosisSearch()
        {
            if (DiagnosisSearch != null)
            {
                DiagnosisSearch.Cleared -= DiagnosisSearch_Cleared;
            }
            DiagnosisSearch = new PopupSearch<DiagnosisViewModel>(
                   EntityManagers.DiagnosisManager.RootFiltratingSearcher,
                   onSelected: (dia) => { dia.IsChecked = true; });

            DiagnosisSearch.Cleared += DiagnosisSearch_Cleared;

            UpdateDiagnosisQueryCode();
        }

        private void DiagnosisSearch_Cleared(object sender, EventArgs e)
        {
            Diagnosis = null;
            // DiagnosisSearch.Query already empty
        }

        private void UpdateDiagnosisQueryCode()
        {
            if (DiagnosisSearch != null)
            {
                DiagnosisSearch.UpdateResultsOnQueryChanges = false;

                if (Diagnosis != null)
                    DiagnosisSearch.Query = Diagnosis.Code;
                else
                    DiagnosisSearch.Query = "";

                DiagnosisSearch.UpdateResultsOnQueryChanges = true;
            }
        }

        #region Event handlers

        private void Subscribe()
        {
            this.PropertyChanged += HealthRecordViewModel_PropertyChanged;
            EntityManagers.DiagnosisManager.RootChanged += (s, e) =>
            {
                CreateDiagnosisSearch();
            };
        }

        private void SubscribeToCheckedChanges()
        {
            msgHandlers = new List<EventMessageHandler>()
            {
                this.Subscribe((int)EventID.WordCheckedChanged, (e) =>
                {
                    if (this.IsSelected && !makingCurrent)
                    {
                        var word = e.GetValue<WordViewModel>(Messages.Word);
                        var isChecked = e.GetValue<bool>(Messages.CheckedState);

                        OnWordCheckedChanged(word, isChecked);
                    }
                }),
                this.Subscribe((int)EventID.DiagnosisCheckedChanged, (e) =>
                {
                    if (this.IsSelected && !makingCurrent)
                    {
                        var diagnosis = e.GetValue<DiagnosisViewModel>(Messages.Diagnosis);
                        var isChecked = e.GetValue<bool>(Messages.CheckedState);

                        OnDiagnosisCheckedChanged(diagnosis, isChecked);
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

        private void OnWordCheckedChanged(WordViewModel word, bool isChecked)
        {
            // меняем симптом у открытой записи

            HashSet<WordViewModel> words;

            if (Symptom != null)
                words = new HashSet<WordViewModel>(Symptom.Words);
            else
                words = new HashSet<WordViewModel>();

            if (isChecked)
            {
                words.Add(word);
            }
            else
            {
                words.Remove(word);
            }

            this.Symptom = EntityManagers.SymptomsManager.Create(words);
            healthRecord.Symptom = Symptom.symptom;
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

            UpdateDiagnosisQueryCode();
            OnPropertyChanged(() => Name);
        }

        #endregion Event handlers

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} {4} {5}", Category, Symptom, NumValue != null ? NumValue.ToString() : "", Diagnosis, DateOffset, Comment);
        }
    }
}