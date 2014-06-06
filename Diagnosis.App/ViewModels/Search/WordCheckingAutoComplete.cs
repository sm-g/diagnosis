using Diagnosis.Core;
using System.Linq;
using System.Collections.Generic;

namespace Diagnosis.App.ViewModels
{
    /// <summary>
    /// Отмечает слово при выборе.
    /// </summary>
    public class WordCheckingAutoComplete : WordRootAutoComplete
    {
        protected override void BeforeAddItem(WordViewModel item)
        {
            item.IsChecked = true;
        }

        public WordCheckingAutoComplete(QuerySeparator separator, SimpleSearcherSettings settings, IEnumerable<WordViewModel> initItems = null)
            : base(separator, settings, initItems)
        { }
    }
}