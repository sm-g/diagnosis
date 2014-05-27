using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class WordAutoComplete : AutoCompleteBase<WordViewModel>
    {
        protected override ISearcher<WordViewModel> MakeSearch(WordViewModel parent)
        {
            WordSearcher searcher;
            if (parent == null)
            {
                parent = EntityManagers.WordsManager.Root;
            }
            searcher = new WordSearcher(parent, settings);
            searcher.UpperPriority = parent.Priority;
            return searcher;
        }

        protected override string GetQueryString(WordViewModel item)
        {
            return item.Name;
        }

        protected override void BeforeAddItem(WordViewModel item)
        {
            if (items.Count > 0)
                items.Last().AddIfNotExists(item, searcher.AllChildren);
        }

        public WordAutoComplete(QuerySeparator separator, SearcherSettings settings)
            : base(separator, settings)
        { }
    }
}