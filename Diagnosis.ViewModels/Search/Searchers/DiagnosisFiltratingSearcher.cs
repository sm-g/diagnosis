using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System;
using System.Linq;

namespace Diagnosis.ViewModels
{
    /// <summary>
    /// Для поиска по дереву, когда в Collection только элементы первого уровня.
    /// Search возвращает такие из них, у которых есть отфильтрованные потомки.
    /// </summary>
    public class DiagnosisFiltratingSearcher : DiagnosisSearcher
    {
        public DiagnosisFiltratingSearcher(DiagnosisViewModel parent, bool withChecked = false)
            : base(parent, new HierarchicalSearchSettings() { WithNonCheckable = true, WithChecked = withChecked })
        {
        }
        /// <summary>
        /// Фильтрует элемент и рекурсивно всех детей, устанавливая значение <code>IsFiltered = true</code>,
        /// если запросу удовлетворяет сам элемент или хотя бы один потомок.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        protected override bool Filter(DiagnosisViewModel item, string query)
        {
            var filterRoot = base.Filter(item, query);
            bool anyChild = false;

            foreach (var diaVm in item.Children.Where(child => FilterCheckable(child) && Filter(child, query)))
            {
                anyChild = true;
            }
            var result = filterRoot || anyChild;

            item.IsFiltered = result;
            return result;
        }
    }
}