using Diagnosis.Core;

namespace Diagnosis.App.ViewModels
{
    public class DiagnosisAutoComplete : AutoCompleteBase<DiagnosisViewModel>
    {
        protected override ISearcher<DiagnosisViewModel> MakeSearch(DiagnosisViewModel parent)
        {
            DiagnosisSearcher searcher;
            if (parent == null)
            {
                parent = EntityManagers.DiagnosisManager.Root;
            }
            searcher = new DiagnosisSearcher(parent, settings);
            return searcher;
        }

        protected override string GetQueryString(DiagnosisViewModel item)
        {
            return item.Name;
        }

        public DiagnosisAutoComplete(QuerySeparator separator, SearcherSettings settings)
            : base(separator, settings)
        { }
    }
}
