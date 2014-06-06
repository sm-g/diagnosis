using Diagnosis.Core;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class WordRootAutoComplete : AutoCompleteBase<WordViewModel>
    {
        protected override ISimpleSearcher<WordViewModel> MakeSearch(WordViewModel parent, IEnumerable<WordViewModel> checkedWords)
        {
            WordSearcher searcher;
            searcher = new WordSearcher(EntityManagers.WordsManager.Root, settings, checkedWords);
            return searcher;
        }

        protected override string GetQueryString(WordViewModel item)
        {
            return item.Name;
        }

        public WordRootAutoComplete(QuerySeparator separator, SimpleSearcherSettings settings)
            : base(separator, settings)
        { }
    }
}