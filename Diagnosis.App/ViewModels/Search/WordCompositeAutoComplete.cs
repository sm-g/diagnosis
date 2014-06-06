﻿using System.Linq;
using Diagnosis.Core;
using System.Collections.Generic;

namespace Diagnosis.App.ViewModels
{
    public class WordCompositeAutoComplete : AutoCompleteBase<WordViewModel>
    {
        protected override ISimpleSearcher<WordViewModel> MakeSearch(WordViewModel parent, IEnumerable<WordViewModel> checkedWords)
        {
            WordSearcher searcher;
            if (parent == null)
            {
                parent = EntityManagers.WordsManager.Root;
            }
            searcher = new WordCompositeSearcher(parent, settings, checkedWords);
            searcher.UpperPriority = parent.Priority;
            return searcher;
        }

        protected override string GetQueryString(WordViewModel item)
        {
            return item.Name;
        }
        public WordCompositeAutoComplete(QuerySeparator separator, SimpleSearcherSettings settings)
            : base(separator, settings)
        { }
    }
}