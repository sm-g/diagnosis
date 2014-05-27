using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.App.ViewModels
{
    public class DiagnosisAutoComplete : AutoCompleteBase<DiagnosisViewModel>
    {
        protected override ISearcher<DiagnosisViewModel> MakeSearch(DiagnosisViewModel parent)
        {
            DiagnosisSearcher searcher;
            if (parent == null)
            {
                parent = EntityManagers.DiagnosisManager.Diagnoses[0].Parent;
            }
            searcher = new DiagnosisSearcher(parent, true, true, false); // groups, cheсked, all children
            return searcher;
        }

        protected override string GetQueryString(DiagnosisViewModel item)
        {
            return item.Name;
        }
    }
}
