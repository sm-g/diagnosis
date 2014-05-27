using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class DiagnosisFiltratingSearcher : DiagnosisSearcher
    {
        public DiagnosisFiltratingSearcher(DiagnosisViewModel parent, bool withChecked = false)
            : base(parent, new SearcherSettings(withNonCheckable: true, withChecked: withChecked, withCreatingNew: false, allChildren: false))
        {
        }
        /// <summary>
        /// Фильтрует элемент и рекурсивно всех детей, устанавливая значение <code>IsFiltered</code>.
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