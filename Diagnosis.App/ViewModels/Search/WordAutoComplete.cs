namespace Diagnosis.App.ViewModels
{
    public class WordAutoComplete : AutoCompleteBase<WordViewModel>
    {
        protected override HierarchicalSearch<WordViewModel> MakeSearch(WordViewModel parent)
        {
            if (parent == null)
            {
                return new WordSearch(EntityManagers.WordsManager.Words[0].Parent, false, false, true);
            }
            else                                                                // groups, cheсked, all children
            {
                return new WordSearch(parent, false, false, true);
            }
        }

        protected override string GetQueryString(WordViewModel item)
        {
            return item.Name;
        }
    }
}