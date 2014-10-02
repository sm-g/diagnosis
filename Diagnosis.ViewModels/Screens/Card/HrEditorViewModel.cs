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

        /// <summary>
        /// Создает автокомплит с начальными словами и измерениями из редактируемой записи.
        /// </summary>
        private void CreateAutoComplete()
        {
            var initials = from item in HealthRecord.healthRecord.HrItems
                           orderby item.Order
                           select item.Entity;

            _autocomplete = new Autocomplete(
                new Recognizer(session, true) { AllowNewFromQuery = true },
                true,
                initials);

            _autocomplete.EntitiesChanged += (s, e) =>
            {
                var entities = _autocomplete.GetEntities().ToList();

                // меняем элементы записи при завершении или удалении тега
                // TODO save order
                SetWords(HealthRecord.healthRecord, entities);
                SetMeasures(HealthRecord.healthRecord, entities);
            };

            OnPropertyChanged("Autocomplete");
        }

        private void SetMeasures(HealthRecord hr, IEnumerable<IDomainEntity> entities)
        {
            var measures = entities.Where(x => x is Measure).Cast<Measure>().ToList();
            // сохраняем в измерениях их порядок в автокомплите
            measures.ForEach(m => m.Order = (byte)measures.IndexOf(m));

            var toAdd = measures.Except(hr.Measures).ToList();
            var toRemove = hr.Measures.Except(measures).ToList();
            toAdd.ForEach(m =>
            {
                new HrItem(hr, m);
            });
            toRemove.ForEach(m => hr.RemoveItem(hr.HrItems.First(i => i.Measure == m)));
        }
        private void SetWords(HealthRecord hr, IEnumerable<IDomainEntity> entities)
        {
            var words = entities.Where(x => x is Word).Cast<Word>().ToList();

            var toAdd = words.Except(hr.Words).ToList();
            var toRemove = hr.Words.Except(words).ToList();
            toAdd.ForEach(m =>
            {
                var item = new HrItem(hr, m);

            });
            toRemove.ForEach(m => hr.RemoveItem(hr.HrItems.First(i => i.Word == m)));
        }

        //private void SetSymptomAndWordsOrder(HealthRecord hr, IEnumerable<IDomainEntity> entities)
        //{
        //    inSetSymptomOnTagCompleted = true;
        //    var words = entities.Where(x => x is Word).Cast<Word>().ToList();
        //    if (words.Count > 0)
        //    {
        //        // симптом со всеми словами
        //        Symptom existing = SymptomQuery.ByWords(session)(words);

        //        if (existing == null)
        //        {
        //            hr.WordsOrder = string.Join("", Enumerable.Range(0, words.Count));
        //            hr.Symptom = new Symptom(words);
        //        }
        //        else
        //        {
        //            var wordsInExisting = existing.Words.ToList();
        //            hr.WordsOrder = string.Join("", words.Select(w => wordsInExisting.IndexOf(w)));
        //            // hr.Symptom = existing;
        //        }
        //    }
        //    else
        //    {
        //        hr.WordsOrder = "";
        //        hr.Symptom = null;
        //    }
        //    inSetSymptomOnTagCompleted = false;
        //}


        /// <summary>
        /// w w m m w => 221
        /// m w => 011
        /// </summary>
        /// <param name="entities"></param>
        //private static string GetAutocompleteEnititiesSequence(IEnumerable<IDomainEntity> entities)
        //{
        //    string seq = "";
        //    var ent = entities.ToList();
        //    if (ent.Count == 0)
        //    {
        //        return "";
        //    }
        //    if (ent.First() is Measure)
        //        seq = "0";

        //    var counter = 1;
        //    for (int i = 1; i < ent.Count; i++)
        //    {
        //        if (counter > 9)
        //        {
        //            throw new Exception("Больше 9 слов/измерений подряд");
        //        }
        //        if (ent[i].GetType() == ent[i - 1].GetType())
        //        {
        //            counter++;
        //        }
        //        else
        //        {
        //            seq += counter.ToString();
        //            counter = 1;
        //        }
        //    }
        //    seq += counter.ToString();
        //    return seq;
        //}

        /// <summary>
        /// Слова и измерения в порядке их сохранения.
        /// </summary>
        /// <param name="hr"></param>
        /// <returns></returns>
        //public static List<IDomainEntity> GetOrderedWordsMeasures(HealthRecord hr)
        //{
        //    var initials = new List<IDomainEntity>();

        //    var words = hr.Words.ToList();

        //    var wordsOrder = hr.WordsOrder != null ?
        //        hr.WordsOrder.ToDigits().ToList() :
        //        Enumerable.Range(0, words.Count).ToList();
        //    var wmSequence = hr.WordsMeasuresSequence != null ?
        //        hr.WordsMeasuresSequence.ToDigits() :
        //        new[] { words.Count, hr.Measures.Count() };
        //    bool takingWords = true;
        //    int wordsTaken = 0, measuresTaken = 0;
        //    foreach (var n in wmSequence)
        //    {
        //        if (takingWords)
        //        {
        //            initials.AddRange(wordsOrder.Skip(wordsTaken).Take(n).Select(i => words[i]));
        //            wordsTaken += n;
        //        }
        //        else
        //        {
        //            initials.AddRange(hr.Measures.OrderBy(m => m.Order).Skip(measuresTaken).Take(n));
        //            measuresTaken += n;
        //        }

        //        takingWords = !takingWords;
        //    }
        //    return initials;
        //}

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