using Diagnosis.Core;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    /// <summary>
    /// Ищет по всем словам, отмечает слово при выборе.
    /// </summary>
    public class WordCheckingAutoComplete : AutoCompleteBase<WordViewModel>
    {
        protected override ISimpleSearcher<WordViewModel> MakeSearch(WordViewModel parent)
        {
            WordSearcher searcher;
            searcher = new WordSearcher(EntityManagers.WordsManager.Root, settings);
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

        public WordCheckingAutoComplete(QuerySeparator separator, SimpleSearcherSettings settings)
            : base(separator, settings)
        { }
    }
}