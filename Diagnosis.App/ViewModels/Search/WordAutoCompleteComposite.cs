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
                parent = EntityManagers.WordsManager.Root;
            }
            searcher = new WordSearcherComposite(parent, settings);
            searcher.UpperPriority = parent.Priority;
            return searcher;
        }

        protected override string GetQueryString(WordViewModel item)
        {
            return item.Name;
        }
        public WordAutoCompleteComposite(QuerySeparator separator, SearcherSettings settings)
            : base(separator, settings)
        { }
    }
}