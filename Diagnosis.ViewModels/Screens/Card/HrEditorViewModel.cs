﻿using Diagnosis.Core;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search.Autocomplete;
using EventAggregator;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using NHibernate.Linq;
using System.Diagnostics.Contracts;
using System.ComponentModel;

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
            _autocomplete.InputEnded += _autocomplete_InputEnded;

            OnPropertyChanged("AutoComplete");
        }

        void _autocomplete_InputEnded(object sender, EventArgs e)
        {
            // меняет симптом записи

            // TODO менять только при сохранении?
            // тогда не нужно вызывать InputEnded при потере фокуса автокомплитом
            // как тогда показать, что это поле поменялось?

            var words = _autocomplete.GetItems().Cast<Word>().ToList();
            if (words.Count > 0) // == 0 если исправляем единтсвенное слово
                HealthRecord.healthRecord.Symptom = new Symptom(words);
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

            // смена записи — сохранение редактируемой
            SaveCurrentHr();

            session.BeginTransaction();

            HealthRecord = new HealthRecordViewModel(hr);
            (hr as IEditableObject).BeginEdit();

            CreateAutoComplete();
            UpdateDiagnosisQueryCode();

        }


        /// <summary>
        /// Выгружает редактируемую запись.
        /// </summary>
        public void Unload()
        {
            SaveCurrentHr();
            HealthRecord = null;
        }

        private void SaveCurrentHr()
        {
            if (HealthRecord != null)
            {
                (HealthRecord.healthRecord as IEditableObject).EndEdit();
                if (session.Transaction.IsActive)
                {
                    session.Transaction.Commit();
                }
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
                }

            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}