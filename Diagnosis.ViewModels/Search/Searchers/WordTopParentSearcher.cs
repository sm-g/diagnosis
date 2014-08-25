using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System;
using System.Linq;

namespace Diagnosis.ViewModels
{
    /// <summary>
    /// Ищет верхних родителей
    /// </summary>
    public class WordTopParentSearcher : WordSearcher
    {
        public WordTopParentSearcher() :
            base(EntityProducers.WordsProducer.Root, new HierarchicalSearchSettings() { WithNonCheckable = true, WithChecked = true, AllChildren = true })
        {
        }
        /// <summary>
        /// Вовращает все слова, которые начинаются на указанную строку. 
        /// Если у слова есть родитель, вместо этого слова возвращается самый верхний предок слова.
        /// 
        /// Например, слова:
        /// сидя
        /// образование
        ///     высшее
        ///     среднее
        ///     
        /// Ищем «с», возвращаются «сидя, образование».
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public override IEnumerable<WordViewModel> Search(string query)
        {
            List<WordViewModel> results = new List<WordViewModel>();
            var filtered = Collection.Where(word => Filter(word, query) && FilterCheckable(word));
            foreach (var item in filtered)
            {
                var w = item;
                while (!w.Parent.IsRoot)
                {
                    w = w.Parent;
                }
                results.Add(w);
            }

            return results.Distinct();
        }

    }
}