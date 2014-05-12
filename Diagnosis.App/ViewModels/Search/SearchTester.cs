using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.App.ViewModels.Search
{
    public class SearchTester
    {
        AutoCompleteBase<WordViewModel> _wordAutoComplete;
        public AutoCompleteBase<WordViewModel> WordAutoComplete
        {
            get
            {
                return _wordAutoComplete ?? (_wordAutoComplete = new WordAutoComplete());
            }
        }

        AutoCompleteBase<DiagnosisViewModel> _diaAutoComplete;
        public AutoCompleteBase<DiagnosisViewModel> DiagnosisAutoComplete
        {
            get
            {
                return _diaAutoComplete ?? (_diaAutoComplete = new DiagnosisAutoComplete());
            }
        }

        SearchBase<DiagnosisViewModel> _diagnosisSearch;
        public ISearch<DiagnosisViewModel> DiagnosisFilteringSearch
        {
            get
            {
                return _diagnosisSearch ?? (_diagnosisSearch = new SearchBase<DiagnosisViewModel>(
                    EntityManagers.DiagnosisManager.FiltratingSearcher));
            }
        }

        private AutoComplete _autoComplete;
        public AutoComplete AutoComplete
        {
            get
            {
                return _autoComplete ?? (_autoComplete = new AutoComplete());
            }
        }

        private SearchBase<DiagnosisViewModel> _diaSearch;
        public SearchBase<DiagnosisViewModel> DiagnosisSearch
        {
            get
            {
                return _diaSearch ?? (_diaSearch = new SearchBase<DiagnosisViewModel>(new DiagnosisSearcher(EntityManagers.DiagnosisManager.Diagnoses[0].Children[1])));
            }
        }
    }
}
