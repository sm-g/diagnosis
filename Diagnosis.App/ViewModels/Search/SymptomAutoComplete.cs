namespace Diagnosis.App.ViewModels
{
    public class SymptomAutoComplete : AutoCompleteBase<SymptomViewModel>
    {
        protected override HierarchicalSearch<SymptomViewModel> MakeSearch(SymptomViewModel parent)
        {
            if (parent == null)
            {
                return new SymptomSearch(EntityManagers.SymptomsManager.Symptoms[0].Parent, true, true, false);  // groups and cheсked, but not all children
            }
            else
            {
                return new SymptomSearch(parent, true, true, false);
            }
        }

        protected override string GetQueryString(SymptomViewModel item)
        {
            return item.Name;
        }
    }
}