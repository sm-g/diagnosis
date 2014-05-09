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
            if (parent == null)
            {
                return new DiagnosisSearcher(
                    EntityManagers.DiagnosisManager.Diagnoses[0].Parent, true, true, false);
            }
            else                                                        // groups, cheсked, all children
            {
                return new DiagnosisSearcher(parent, true, true, false);
            }
        }

        protected override string GetQueryString(DiagnosisViewModel item)
        {
            return item.Name;
        }
    }
}
