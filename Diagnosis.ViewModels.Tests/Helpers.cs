using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.ViewModels.Search;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diagnosis.ViewModels.Tests
{
    public static class Helpers
    {
        public static void AddTag(this QueryEditorViewModel qe, object tagOrContent)
        {
            qe.QueryBlocks[0].AutocompleteAll.AddTag(tagOrContent);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="qe"></param>
        /// <param name="title">Текст, которого нет в БД</param>
        /// <returns></returns>
        public static Word CompleteWord(this AutocompleteViewModel auto, string title)
        {
            Contract.Ensures(Contract.Result<Word>().IsTransient);

            var tag = auto.LastTag;
            auto.SelectedTag = tag;
            tag.Query = title;
            auto.CompleteOnEnter(tag, inverse: true);
            return tag.Blank as Word;
        }
    }
}
