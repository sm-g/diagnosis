using Diagnosis.Core;
using System.Collections.Generic;

namespace Diagnosis.App.ViewModels
{
    public class DiagnosisAutoComplete : AutoCompleteBase<DiagnosisViewModel>
    {
        protected override ISimpleSearcher<DiagnosisViewModel> MakeSearch(DiagnosisViewModel parent, IEnumerable<DiagnosisViewModel> checkedDiagnoses)
        {
            DiagnosisSearcher searcher;
            if (parent == null)
            {
                parent = EntityProducers.DiagnosisProducer.Root;
            }
            searcher = new DiagnosisSearcher(parent, settings, checkedDiagnoses);
            return searcher;
        }

        protected override string GetQueryString(DiagnosisViewModel item)
        {
            return item.Name;
        }

        public DiagnosisAutoComplete(QuerySeparator separator, SimpleSearcherSettings settings)
            : base(separator, settings)
        { }
    }
}
