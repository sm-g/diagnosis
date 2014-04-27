using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class HealthRecordViewModel : CheckableBase
    {
        private readonly HealthRecord healthRecord;
        private static HealthRecordViewModel current;
        private AutoComplete _autoComplete;
        private DiagnosisAutoComplete _diagnosisAutoComplete;
        private DateOffset _dateOffset;
        private List<EventMessageHandler> msgHandlers;

        public IEditable Editable { get; private set; }

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

        public override void OnCheckedChanged()
        {
            throw new NotImplementedException();
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
                    if (value != null)
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
                    _dateOffset = new DateOffset(FromYear, FromMonth, FromDay);
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

        public AutoComplete AutoComplete
        {
            get
            {
                return _autoComplete ?? (_autoComplete = new AutoComplete());
            }
        }

        public DiagnosisAutoComplete DiagnosisAutoComplete
        {
            get
            {
                return _diagnosisAutoComplete ?? (_diagnosisAutoComplete = new DiagnosisAutoComplete());
            }
        }

        public void MakeCurrent()
        {
            current = this;

            if (Symptom != null)
                EntityManagers.WordsManager.CheckThese(Symptom.Words);
            if (HasDiagnosis)
                EntityManagers.DiagnosisManager.Check(Diagnosis);
        }

        public HealthRecordViewModel(HealthRecord hr)
        {
            Contract.Requires(hr != null);

            this.healthRecord = hr;

            Editable = new EditableBase(this, dirtImmunity: true);

            Category = EntityManagers.CategoryManager.GetByModel(hr.Category);
            Symptom = EntityManagers.SymptomsManager.Symptoms.FirstOrDefault(s => s.symptom == hr.Symptom);
            Diagnosis = EntityManagers.DiagnosisManager.GetHealthRecordDiagnosis(healthRecord);


            Subscribe();

            Editable.CanBeDirty = true;
        }

        #region Event handlers

        public void Subscribe()
        {
            msgHandlers = new List<EventMessageHandler>()
            {
                this.Subscribe((int)EventID.WordCheckedChanged, (e) =>
                {
                    var symptom = e.GetValue<WordViewModel>(Messages.Word);
                    var isChecked = e.GetValue<bool>(Messages.CheckedState);

                    OnWordCheckedChanged(symptom, isChecked);
                }),
                this.Subscribe((int)EventID.DiagnosisCheckedChanged, (e) =>
                {
                    var diagnosis = e.GetValue<DiagnosisViewModel>(Messages.Diagnosis);
                    var isChecked = e.GetValue<bool>(Messages.CheckedState);

                    OnDiagnosisCheckedChanged(diagnosis, isChecked);
                })
            };
        }

        public void Unsubscribe()
        {
            foreach (var h in msgHandlers)
            {
                h.Dispose();
            }
        }

        private void OnWordCheckedChanged(WordViewModel word, bool isChecked)
        {
            if (this == current)
            {
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
                this.Symptom = EntityManagers.SymptomsManager.GetSymptomForWords(words);
                healthRecord.Symptom = Symptom.symptom;
            }
        }

        private void OnDiagnosisCheckedChanged(DiagnosisViewModel diagnosisVM, bool isChecked)
        {
            if (this == current)
            {
                if (isChecked)
                {
                    Diagnosis = diagnosisVM;
                    //  healthRecord.Diagnosis = diagnosisVM.diagnosis;
                }
                else
                {
                    Diagnosis = null;
                    //   healthRecord.Diagnosis = null;
                }
                OnPropertyChanged(() => Name);
            }
        }

        #endregion Event handlers
    }
}