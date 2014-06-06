using Diagnosis.Core;
using System.Linq;

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

        public WordCheckingAutoComplete(QuerySeparator separator, SimpleSearcherSettings settings)
            : base(separator, settings)
        { }
    }
}