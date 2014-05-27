using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.App.ViewModels
{
    public class SearchTester : ViewModelBase
    {
        private AutoCompleteBase<WordViewModel> _wordAutoComplete;
        private AutoCompleteBase<DiagnosisViewModel> _diaAutoComplete;
        private SearchBase<DiagnosisViewModel> _diagnosisSearch;
        private AutoComplete _autoComplete;
        private SearchBase<DiagnosisViewModel> _diaSearch;

        public AutoCompleteBase<WordViewModel> WordAutoComplete
        {
            get
            {
                return _wordAutoComplete ?? (_wordAutoComplete = new WordAutoComplete());
            }
        }

        public AutoCompleteBase<DiagnosisViewModel> DiagnosisAutoComplete
        {
            get
            {
                return _diaAutoComplete ?? (_diaAutoComplete = new DiagnosisAutoComplete(new SearcherSettings(true, true, false)));
            }
        }

        public ISearch<DiagnosisViewModel> DiagnosisFilteringSearch
        {
            get
            {
                return _diagnosisSearch ?? (_diagnosisSearch = new SearchBase<DiagnosisViewModel>(
                    EntityManagers.DiagnosisManager.FiltratingSearcher));
            }
        }

        public AutoComplete AutoComplete
        {
            get
            {
                return _autoComplete ?? (_autoComplete = new AutoComplete());
            }
        }

        public SearchBase<DiagnosisViewModel> DiagnosisSearch
        {
            get
            {
                return _diaSearch ?? (_diaSearch = new SearchBase<DiagnosisViewModel>(
                    new DiagnosisSearcher(EntityManagers.DiagnosisManager.Diagnoses[0].Children[1], new SearcherSettings())));
            }
        }
    }
}