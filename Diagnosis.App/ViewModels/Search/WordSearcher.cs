using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class WordSearcher : ISearcher<WordViewModel>
    {
        public bool WithNonCheckable { get; set; }

        public bool WithChecked { get; set; }

        public bool WithCreatingNew { get; set; }
        public bool AllChildren { get; set; }

        public IEnumerable<WordViewModel> Collection { get; private set; }

        public WordSearcher(WordViewModel parent, bool withNonCheckable = false, bool withChecked = false, bool withCreatingNew = true, bool allChildren = true)
        {
            Contract.Requires(parent != null);
            Collection = AllChildren ? parent.AllChildren : parent.Children;

            AllChildren = allChildren;
            WithNonCheckable = withNonCheckable;
            WithChecked = withChecked;
            WithCreatingNew = withCreatingNew;
        }

        public IEnumerable<WordViewModel> Search(string query)
        {
            List<WordViewModel> results = new List<WordViewModel>();

            results.AddRange(Collection.Where(c => Filter(c as WordViewModel, query) && Filter(c as WordViewModel)));
            if (WithCreatingNew
               && query != string.Empty
               && !results.Any(c => Filter(c as WordViewModel, query)))
            {
                // добавляем запрос к результатам
                results.Add(FromQuery(query));
            }
            return results;
        }

        protected virtual WordViewModel FromQuery(string query)
        {
            return EntityManagers.WordsManager.Create(query);
        }

        protected bool Filter(WordViewModel item, string query)
        {
            return item.Name.StartsWith(query, StringComparison.InvariantCultureIgnoreCase);
            // && (UpperPriority <= item.Priority);
        }

        private bool Filter(ICheckable obj)
        {
            return (WithChecked || !obj.IsChecked)
                   && (WithNonCheckable || !obj.IsNonCheckable);
        }
    }
}