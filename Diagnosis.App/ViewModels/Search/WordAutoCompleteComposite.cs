using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class WordAutoCompleteComposite : AutoCompleteBase<WordViewModel>
    {
        protected override ISearcher<WordViewModel> MakeSearch(WordViewModel parent)
        {
            WordSearcher searcher;
            if (parent == null)
            {
                parent = EntityManagers.WordsManager.Words[0].Parent;
            }
            searcher = new WordSearcherComposite(parent, false, false, true); // groups, cheсked, all children
            searcher.UpperPriority = parent.Priority;
            return searcher;
        }

        protected override string GetQueryString(WordViewModel item)
        {
            return item.Name;
        }
    }
}