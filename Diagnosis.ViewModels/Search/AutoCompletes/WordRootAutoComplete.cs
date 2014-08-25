using Diagnosis.Core;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class WordRootAutoComplete : AutoCompleteBase<WordViewModel>
    {
        protected override ISimpleSearcher<WordViewModel> MakeSearch(WordViewModel parent, IEnumerable<WordViewModel> checkedWords)
        {
            WordSearcher searcher;
            searcher = new WordSearcher(EntityProducers.WordsProducer.Root, settings, checkedWords);
            return searcher;
        }

        protected override string GetQueryString(WordViewModel item)
        {
            return item.Name;
        }

        public WordRootAutoComplete(QuerySeparator separator, HierarchicalSearchSettings settings, IEnumerable<WordViewModel> initItems = null)
            : base(separator, settings, initItems)
        { }
    }
}