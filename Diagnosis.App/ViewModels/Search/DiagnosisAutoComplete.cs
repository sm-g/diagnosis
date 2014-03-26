using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.App.ViewModels
{
    public class DiagnosisAutoComplete : AutoCompleteBase<DiagnosisViewModel>
    {
        protected override HierarchicalSearch<DiagnosisViewModel> MakeSearch(DiagnosisViewModel parent)
        {
            if (parent == null)
            {
                return new DiagnosisSearch(EntityManagers.DiagnosisManager.Diagnoses[0].Parent, true, true, false);  // groups and cheсked, but not all children
            }
            else
            {
                return new DiagnosisSearch(parent, true, true, false);
            }
        }

        protected override string GetQueryString(DiagnosisViewModel item)
        {
            return item.Name;
        }
    }
}
