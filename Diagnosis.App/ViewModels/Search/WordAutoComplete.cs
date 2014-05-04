namespace Diagnosis.App.ViewModels
{
    public class WordAutoComplete : AutoCompleteBase<WordViewModel>
    {
        protected override SearchBase<WordViewModel> MakeSearch(WordViewModel parent)
        {
            if (parent == null)
            {
                return new SearchBase<WordViewModel>(new WordSearcher(
                    EntityManagers.WordsManager.Words[0].Parent, false, false, true));
            }
            else                                                // groups, cheсked, all children
            {
                return new SearchBase<WordViewModel>(new WordSearcher(parent, false, false, true));
            }
        }

        protected override string GetQueryString(WordViewModel item)
        {
            return item.Name;
        }
    }
}