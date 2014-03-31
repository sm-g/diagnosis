namespace Diagnosis.App.ViewModels
{
    public class SymptomAutoComplete : AutoCompleteBase<SymptomViewModel>
    {
        protected override HierarchicalSearch<SymptomViewModel> MakeSearch(SymptomViewModel parent)
        {
            if (parent == null)
            {
                return new SymptomSearch(EntityManagers.SymptomsManager.Symptoms[0].Parent, false, false, true);
            }
            else                                                                // groups, cheсked, all children
            {
                return new SymptomSearch(parent, false, false, true);
            }
        }

        protected override string GetQueryString(SymptomViewModel item)
        {
            return item.Name;
        }
    }
}