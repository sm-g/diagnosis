using Diagnosis.Core;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search.Autocomplete;
using EventAggregator;
using NHibernate;
using NHibernate.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class HrEditorViewModel : ViewModelBase
    {
        private Autocomplete _autocomplete;
        private PopupSearch<DiagnosisViewModel> _diagnosisSearch;

        private HealthRecordViewModel _hr;
        private ISession session;
        private IEnumerable<Category> _categories;
        private EventMessageHandler handler;

        public HrEditorViewModel(ISession session)
        {
            this.session = session;

            handler = this.Subscribe(Events.SettingsSaved, (e) =>
             {
                 OnPropertyChanged(() => ShowIcdDiseaseSearch);
                 // после смены настроек доктора может понадобиться поиск по диагнозам
                 CreateDiagnosisSearch();
             });

            if (ShowIcdDiseaseSearch)
                CreateDiagnosisSearch();
        }

        #region HealthRecord

        public HealthRecordViewModel HealthRecord
        {
            get
            {
                return _hr;
            }
            private set
            {
                if (_hr != value)
                {
                    _hr = value;
                    OnPropertyChanged("HealthRecord");
                    OnPropertyChanged("Category");
                    OnPropertyChanged("IsActive");
                }
            }
        }

        public IEnumerable<Category> Categories
        {
            get
            {
                if (_categories == null)
                {
                    _categories = new List<Category>(session.Query<Category>()
                        .OrderBy(cat => cat.Ord).ToList());
                }
                return _categories;
            }
        }

        public Category Category
        {
            get
            {
                return HealthRecord != null ? HealthRecord.Category : null;
            }
            set
            {
                HealthRecord.Category = value;
            }
        }

        public bool IsActive
        {
            get
            {
                return HealthRecord != null;
            }
        }

        #endregion HealthRecord

        public RelayCommand RevertCommand
        {
            get
            {
                return new RelayCommand(() =>
                       {
                           (HealthRecord.healthRecord as IEditableObject).CancelEdit();
                           (HealthRecord.healthRecord as IEditableObject).BeginEdit();
                       }, () => IsActive && HealthRecord.healthRecord.IsDirty);
            }
        }

        public RelayCommand DeleteCommand
        {
            get
            {
                return new RelayCommand(() =>
                       {
                           HealthRecord.healthRecord.IsDeleted = true;
                       });
            }
        }

        public RelayCommand CloseCommand
        {
            get
            {
                return new RelayCommand(Unload);
            }
        }

        #region AutoComplete

        public Autocomplete Autocomplete { get { return _autocomplete; } }

        private void CreateAutoComplete()
        {
            List<Word> initialWords = new List<Word>();
            if (HealthRecord.Symptom != null)
                foreach (var item in HealthRecord.healthRecord.Symptom.Words)
                {
                    initialWords.Add(item);
                }

            _autocomplete = new Autocomplete(new Recognizer(session, true) { AllowNewFromQuery = true }, true, initialWords.ToArray());

            OnPropertyChanged("AutoComplete");
        }

        #endregion AutoComplete

        #region Diagnosis search

        public PopupSearch<DiagnosisViewModel> DiagnosisSearch
        {
            get
            {
                return _diagnosisSearch;
            }
            private set
            {
                if (_diagnosisSearch != value)
                {
                    _diagnosisSearch = value;
                    OnPropertyChanged("DiagnosisSearch");
                }
            }
        }

        private void CreateDiagnosisSearch()
        {
            DiagnosisSearch = new PopupSearch<DiagnosisViewModel>(
                   EntityProducers.DiagnosisProducer.RootFiltratingSearcher
                   );

            DiagnosisSearch.Filter.Cleared += (s, e) =>
            {
                HealthRecord.Diagnosis = null;
            };
            DiagnosisSearch.ResultItemSelected += (s, e) =>
            {
                if (HealthRecord != null)
                {
                    HealthRecord.Diagnosis = e.vm as DiagnosisViewModel;
                    UpdateDiagnosisQueryCode();
                }
            };
            DiagnosisSearch.Filter.Results.CollectionChanged += (s, e) =>
            {
                // VM.IsFiltered
            };
            UpdateDiagnosisQueryCode();
        }

        #endregion Diagnosis search

        public bool ShowIcdDiseaseSearch
        {
            get
            {
                return AuthorityController.CurrentDoctor.DoctorSettings.HasFlag(DoctorSettings.ShowIcdDisease);
            }
        }

        /// <summary>
        /// Загружает запись в редактор.
        /// </summary>
        /// <param name="hr"></param>
        public void Load(HealthRecord hr)
        {
            Contract.Requires(hr != null);

            CloseCurrentHr();

            HealthRecord = new HealthRecordViewModel(hr);
            hr.PropertyChanged += hr_PropertyChanged;

            (hr as IEditableObject).BeginEdit();

            CreateAutoComplete();
            UpdateDiagnosisQueryCode();
        }

        /// <summary>
        /// Выгружает редактируемую запись.
        /// </summary>
        public void Unload()
        {
            CloseCurrentHr();
            HealthRecord = null;
        }

        private void CloseCurrentHr()
        {
            if (HealthRecord != null)
            {
                HealthRecord.healthRecord.PropertyChanged -= hr_PropertyChanged;
                SaveCurrentHr();
                (HealthRecord.healthRecord as IEditableObject).EndEdit();
            }
        }

        private void SaveCurrentHr()
        {
            var words = _autocomplete.GetItems().Cast<Word>().ToList();
            if (words.Count > 0) // == 0 если исправляем единственное слово
            {
                // симптом со всеми словами
                var existing = SymptomQuery.ByWords(session)(words);

                if (existing == null)
                    HealthRecord.healthRecord.Symptom = new Symptom(words);
                else
                    HealthRecord.healthRecord.Symptom = existing;
            }
        }

        private void UpdateDiagnosisQueryCode()
        {
            if (DiagnosisSearch != null && HealthRecord != null)
            {
                DiagnosisSearch.Filter.UpdateResultsOnQueryChanges = false;

                if (HealthRecord.Diagnosis != null)
                    DiagnosisSearch.Filter.Query = HealthRecord.Diagnosis.Code;
                else
                    DiagnosisSearch.Filter.Query = "";

                DiagnosisSearch.Filter.UpdateResultsOnQueryChanges = true;
            }
        }

        private void hr_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Category")
            {
                OnPropertyChanged(e.PropertyName);
            }
        }

        public override string ToString()
        {
            return string.Format("editor {0}", HealthRecord);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    handler.Dispose();
                    Unload();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}