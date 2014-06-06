using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    /// <summary>
    /// Ищет по детям переданного родителя. 
    /// </summary>
    public class WordSearcher : ISimpleSearcher<WordViewModel>
    {
        IEnumerable<WordViewModel> checkedWords;

        public bool WithNonCheckable { get; set; }
        public bool WithChecked { get; set; }
        public bool WithCreatingNew { get; set; }
        public bool AllChildren { get; set; }
        public byte UpperPriority { get; set; }

        public IEnumerable<WordViewModel> Collection { get; private set; }

        public WordSearcher(WordViewModel parent, SimpleSearcherSettings settings, IEnumerable<WordViewModel> checkedWords = null)
        {
            Contract.Requires(parent != null);
            Collection = settings.AllChildren ? parent.AllChildren : parent.Children;

            AllChildren = settings.AllChildren;
            WithNonCheckable = settings.WithNonCheckable;
            WithChecked = settings.WithChecked;
            WithCreatingNew = settings.WithCreatingNew;
            this.checkedWords = checkedWords;
        }

        /// <summary>
        /// Вовращает слова из колллекции, которые начинаются на указанную строку
        /// и у которых приоритет больше или равен <code>UpperPriority</code>.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public virtual IEnumerable<WordViewModel> Search(string query)
        {
            List<WordViewModel> results = new List<WordViewModel>();

            results.AddRange(Collection.Where(word => Filter(word, query) && FilterCheckable(word)));
            if (WithCreatingNew
               && query != string.Empty
               && !results.Any(word => Equals(word, query)))
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
            return item.Name.StartsWith(query, StringComparison.InvariantCultureIgnoreCase)
                && UpperPriority <= item.Priority;
        }

        private bool Equals(WordViewModel item, string query)
        {
            return item.Name.ToLower().Equals(query.ToLower());
        }

        protected bool FilterCheckable(ICheckable obj)
        {
            return (WithChecked || !obj.IsChecked)
                   && (WithNonCheckable || !obj.IsNonCheckable)
                   && (WithChecked || checkedWords == null || !checkedWords.Contains(obj));
        }
    }
}