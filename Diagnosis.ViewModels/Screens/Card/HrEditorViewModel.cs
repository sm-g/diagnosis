using Diagnosis.Core;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search.Autocomplete;
using EventAggregator;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        private IEnumerable<HrCategory> _categories;
        private EventMessageHandler handler;
        private bool inSetSymptomOnTagCompleted;

        public event EventHandler<DomainEntityEventArgs> Unloaded;

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

        public IEnumerable<HrCategory> Categories
        {
            get
            {
                if (_categories == null)
                {
                    _categories = new List<HrCategory>(session.Query<HrCategory>()
                        .OrderBy(cat => cat.Ord).ToList());
                }
                return _categories;
            }
        }

        public HrCategory Category
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

        #endregion HealthRecord
        public bool IsActive
        {
            get
            {
                return HealthRecord != null;
            }
        }


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

        /// <summary>
        /// Создает автокомплит с начальными словами и измерениями из редактируемой записи.
        /// </summary>
        private void CreateAutoComplete()
        {
            var initials = from item in HealthRecord.healthRecord.HrItems // ordered by mapping
                           select item.Entity;

            _autocomplete = new Autocomplete(
                new Recognizer(session, true) { AllowNewFromQuery = true },
                true,
                initials);

            _autocomplete.EntitiesChanged += (s, e) =>
            {
                // меняем элементы записи при завершении или удалении тега
                var entities = _autocomplete.GetEntities().ToList();
                SetOrderedHrItems(HealthRecord.healthRecord, entities);
            };

            OnPropertyChanged("Autocomplete");
        }

        private static void SetOrderedHrItems(HealthRecord hr, List<IHrItemObject> entities)
        {
            var hrEntities = hr.HrItems.Select(x => x.Entity).ToList();
            var toAdd = entities.Except(hrEntities).ToList();
            var toRemove = hrEntities.Except(entities).ToList();

            toAdd.ForEach(m =>
            {
                new HrItem(hr, m);
            });
            toRemove.ForEach(m =>
            {
                hr.RemoveItem(hr.HrItems.First(i => i.Entity == m));
            });

            var ordered = hr.HrItems.OrderBy(i => entities.IndexOf(i.Entity)).ToList();
            for (int i = 0; i < ordered.Count; i++)
            {
                ordered[i].Ord = i;
            }
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

            if (HealthRecord != null && HealthRecord.healthRecord == hr)
                return;

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
                (HealthRecord.healthRecord as IEditableObject).EndEdit();
                OnUnloaded(HealthRecord.healthRecord);
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
            else if (e.PropertyName == "Symptom")
            {
                Debug.Assert(inSetSymptomOnTagCompleted);
            }
        }
        protected virtual void OnUnloaded(HealthRecord hr)
        {
            var h = Unloaded;
            if (h != null)
            {
                h(this, new DomainEntityEventArgs(hr));
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