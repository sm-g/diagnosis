using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using EventAggregator;
using Diagnosis.Core;
using System.Text;

namespace Diagnosis.App.ViewModels
{
    public class SearchTester : ViewModelBase
    {
        private AutoCompleteBase<WordViewModel> _wordAutoComplete;
        private AutoCompleteBase<WordViewModel> _wordAutoCompleteComposite;
        private AutoCompleteBase<DiagnosisViewModel> _diaAutoComplete;
        private SearchBase<DiagnosisViewModel> _diaFilteringSearch;
        private AutoComplete _autoComplete;
        private SearchBase<DiagnosisViewModel> _diaSearch;
        private SearchBase<WordViewModel> _wordSearch;

        public SearchBase<WordViewModel> WordSearch
        {
            get
            {
                return _wordSearch ?? (_wordSearch = new SearchBase<WordViewModel>(
                    EntityManagers.WordsManager.RootSearcher));
            }
        }
        public AutoCompleteBase<WordViewModel> WordAutoComplete
        {
            get
            {
                return _wordAutoComplete ?? (_wordAutoComplete = new WordCheckingAutoComplete(
                    QuerySeparator.Default, new SearcherSettings(false, false, true, true)));
            }
        }

        public AutoCompleteBase<WordViewModel> WordAutoCompleteComposite
        {
            get
            {
                return _wordAutoCompleteComposite ?? (_wordAutoCompleteComposite = new WordCompositeAutoComplete(
                    QuerySeparator.Default, new SearcherSettings(false, false, true, true)));
            }
        }

        public SearchBase<DiagnosisViewModel> DiagnosisSearch
        {
            get
            {
                return _diaSearch ?? (_diaSearch = new SearchBase<DiagnosisViewModel>(EntityManagers.DiagnosisManager.RootSearcher));
            }
        }

        public AutoCompleteBase<DiagnosisViewModel> DiagnosisAutoComplete
        {
            get
            {
                return _diaAutoComplete ?? (_diaAutoComplete = new DiagnosisAutoComplete(
                    QuerySeparator.Default, new SearcherSettings(true, true, false, true)));
            }
        }

        public ISearch<DiagnosisViewModel> DiagnosisFilteringSearch
        {
            get
            {
                return _diaFilteringSearch ?? (_diaFilteringSearch = new SearchBase<DiagnosisViewModel>(
                    EntityManagers.DiagnosisManager.FiltratingSearcher));
            }
        }

        public AutoComplete AutoComplete
        {
            get
            {
                return _autoComplete ?? (_autoComplete = new AutoComplete(QuerySeparator.Default));
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
                var isChecked = e.GetValue<bool>(Messages.CheckedState);

                if (isChecked)
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
                var isChecked = e.GetValue<bool>(Messages.CheckedState);

                if (isChecked)
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