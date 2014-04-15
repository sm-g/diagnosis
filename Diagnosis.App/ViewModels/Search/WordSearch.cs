using Diagnosis.Models;
using System;
using System.Diagnostics.Contracts;

namespace Diagnosis.App.ViewModels
{
    public class WordSearch : HierarchicalSearch<WordViewModel>
    {
        /// <summary>
        /// Верхняя граница приоритета симптома. Поиск выдаёт симптомы с приоритетом, численно большим верхней границы.
        /// </summary>
        public byte UpperPriority { get; set; }

        public WordSearch(WordViewModel parent, bool withNonCheckable = false, bool withChecked = false, bool allChildren = true, byte upperPriority = 0)
            : base(parent, withNonCheckable, withChecked, allChildren)
        {
            UpperPriority = upperPriority;

            InitQuery();
        }

        protected override WordViewModel FromQuery(string query)
        {
            return EntityManagers.WordsManager.Create(query);
        }

        protected override bool Filter(WordViewModel item, string query)
        {
            return item.Name.StartsWith(query, StringComparison.InvariantCultureIgnoreCase)
                 && (UpperPriority <= item.Priority); ;
        }
    }
}