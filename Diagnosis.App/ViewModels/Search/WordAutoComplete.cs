﻿using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class WordAutoComplete : AutoCompleteBase<WordViewModel>
    {
        protected override ISearcher<WordViewModel> MakeSearch(WordViewModel parent)
        {
            if (parent == null)
            {
                return new WordSearcher(
                    EntityManagers.WordsManager.Words[0].Parent, false, false, true);
            }
            else                                                // groups, cheсked, all children
            {
                return new WordSearcher(parent, false, false, true);
            }
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
    }
}