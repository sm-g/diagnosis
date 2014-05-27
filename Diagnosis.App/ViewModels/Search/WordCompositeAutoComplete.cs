using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class WordCompositeAutoComplete : AutoCompleteBase<WordViewModel>
    {
        protected override ISearcher<WordViewModel> MakeSearch(WordViewModel parent)
        {
            WordSearcher searcher;
            if (parent == null)
            {
                parent = EntityManagers.WordsManager.Root;
            }
            searcher = new WordCompositeSearcher(parent, settings);
            searcher.UpperPriority = parent.Priority;
            return searcher;
        }

        protected override string GetQueryString(WordViewModel item)
        {
            return item.Name;
        }
        public WordCompositeAutoComplete(QuerySeparator separator, SearcherSettings settings)
            : base(separator, settings)
        { }
    }
}