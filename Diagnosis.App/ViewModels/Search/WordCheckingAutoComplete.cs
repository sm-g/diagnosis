using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class WordCheckingAutoComplete : AutoCompleteBase<WordViewModel>
    {
        protected override ISearcher<WordViewModel> MakeSearch(WordViewModel parent)
        {
            WordSearcher searcher;
            searcher = new WordSearcher(EntityManagers.WordsManager.Root, settings);
            searcher.UpperPriority = parent.Priority;
            return searcher;
        }

        protected override string GetQueryString(WordViewModel item)
        {
            return item.Name;
        }

        protected override void BeforeAddItem(WordViewModel item)
        {
            item.IsChecked = true;
        }

        public WordCheckingAutoComplete(QuerySeparator separator, SearcherSettings settings)
            : base(separator, settings)
        { }
    }
}