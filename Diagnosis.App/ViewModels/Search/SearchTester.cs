using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using EventAggregator;
using Diagnosis.Core;
using Diagnosis.App.Messaging;
using System.Text;

namespace Diagnosis.App.ViewModels
{
    public class SearchTester : ViewModelBase
    {
        private AutoCompleteBase<WordViewModel> _wordAutoComplete;
        private AutoCompleteBase<WordViewModel> _wordAutoCompleteComposite;
        private AutoCompleteBase<DiagnosisViewModel> _diaAutoComplete;
        private PopupSearch<DiagnosisViewModel> _diaFilteringSearch;
        private AutoCompleteBoxViewModel _autoComplete;
        private PopupSearch<DiagnosisViewModel> _diaSearch;
        private PopupSearch<WordViewModel> _wordSearch;

        public PopupSearch<WordViewModel> WordSearch
        {
            get
            {
                return _wordSearch ?? (_wordSearch = new PopupSearch<WordViewModel>(
                    EntityProducers.WordsProducer.RootSearcher));
            }
        }
        public AutoCompleteBase<WordViewModel> WordAutoComplete
        {
            get
            {
                return _wordAutoComplete ?? (_wordAutoComplete = new WordCheckingAutoComplete(
                    QuerySeparator.Default, new SimpleSearcherSettings(false, false, true, true)));
            }
        }

        public AutoCompleteBase<WordViewModel> WordAutoCompleteComposite
        {
            get
            {
                return _wordAutoCompleteComposite ?? (_wordAutoCompleteComposite = new WordCompositeAutoComplete(
                    QuerySeparator.Default, new SimpleSearcherSettings(false, false, true, true)));
            }
        }

        public PopupSearch<DiagnosisViewModel> DiagnosisSearch
        {
            get
            {
                return _diaSearch ?? (_diaSearch = new PopupSearch<DiagnosisViewModel>(EntityProducers.DiagnosisProducer.RootSearcher));
            }
        }

        public AutoCompleteBase<DiagnosisViewModel> DiagnosisAutoComplete
        {
            get
            {
                return _diaAutoComplete ?? (_diaAutoComplete = new DiagnosisAutoComplete(
                    QuerySeparator.Default, new SimpleSearcherSettings(true, true, false, true)));
            }
        }

        public PopupSearch<DiagnosisViewModel> DiagnosisFilteringSearch
        {
            get
            {
                return _diaFilteringSearch ?? (_diaFilteringSearch = new PopupSearch<DiagnosisViewModel>(
                    EntityProducers.DiagnosisProducer.RootFiltratingSearcher));
            }
        }

        public AutoCompleteBoxViewModel AutoComplete
        {
            get
            {
                return _autoComplete ?? (_autoComplete = new AutoCompleteBoxViewModel(QuerySeparator.Default));
            }
        }
        public ObservableCollection<WordViewModel> Words
        {
            get;
            private set;
        }
        public ObservableCollection<DiagnosisViewModel> Diagnoses
        {
            get;
            private set;
        }

        public SearchTester()
        {
            Words = new ObservableCollection<WordViewModel>();
            Diagnoses = new ObservableCollection<DiagnosisViewModel>();
            this.Subscribe((int)EventID.WordCheckedChanged, (e) =>
            {
                var word = e.GetValue<WordViewModel>(Messages.Word);

                if (word.IsChecked)
                {
                    Words.Add(word);
                }
                else
                {
                    Words.Remove(word);
                }
            });
            this.Subscribe((int)EventID.DiagnosisCheckedChanged, (e) =>
            {
                var dia = e.GetValue<DiagnosisViewModel>(Messages.Diagnosis);

                if (dia.IsChecked)
                {
                    Diagnoses.Add(dia);
                }
                else
                {
                    Diagnoses.Remove(dia);
                }
            });
        }
    }
}