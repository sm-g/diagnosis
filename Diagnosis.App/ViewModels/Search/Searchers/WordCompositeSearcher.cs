using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    /// <summary>
    /// Ищет по детям переданного родителя и всем словам отдельно. 
    /// Результаты поиска по всем словам возвращаются только если строка запроса не пуста.
    /// </summary>
    public class WordCompositeSearcher : WordSearcher
    {
        public WordCompositeSearcher(WordViewModel parent, SimpleSearcherSettings settings, IEnumerable<WordViewModel> checkedWords) :
            base(parent, settings, checkedWords)
        {
        }
        /// <summary>
        /// Вовращает все слова, которые начинаются на указанную строку.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public override IEnumerable<WordViewModel> Search(string query)
        {
            List<WordViewModel> results = new List<WordViewModel>(base.Search(query));
            if (!String.IsNullOrWhiteSpace(query))
            {
                results.AddRange(EntityProducers.WordsProducer.RootSearcher.Search(query).Where(
                    w => FilterCheckable(w)
                      && !results.Contains(w)
                      && UpperPriority <= w.Priority));

            }
            return results;
        }

    }
}