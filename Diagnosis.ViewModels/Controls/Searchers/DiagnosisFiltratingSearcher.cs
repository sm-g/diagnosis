using Diagnosis.ViewModels.Autocomplete;
using System.Collections.Generic;

namespace Diagnosis.ViewModels.Controls
{
    /// <summary>
    /// Для поиска по дереву, когда в Collection только элементы первого уровня.
    /// Search возвращает такие из них, у которых есть отфильтрованные потомки.
    /// </summary>
    public class DiagnosisFiltratingSearcher : DiagnosisSearcher
    {
        private List<DiagnosisViewModel> ignoreList;
        private string lastQuery = "";

        public DiagnosisFiltratingSearcher(DiagnosisViewModel parent)
            : base(parent, true)
        {
            ignoreList = new List<DiagnosisViewModel>();
        }

        /// <summary>
        /// Фильтрует элемент и рекурсивно всех детей, устанавливая значение <code>IsExpanded = true</code>,
        /// если запросу удовлетворяет сам элемент или хотя бы один потомок.
        /// </summary>
        protected override bool Filter(DiagnosisViewModel item, string query)
        {
            if (!query.StartsWith(lastQuery))
            {
                ignoreList.Clear();
            }
            lastQuery = query;

            // элементы, которые не подошли, можно не проверять при более длинном запросе
            if (ignoreList.Contains(item))
                return false;

            var filterItem = base.Filter(item, query);
            bool anyChild = false;

            foreach (var x in item.Children)
            {
                if (Filter(x, query))
                {
                    anyChild = true;
                }
            }

            var result = filterItem || anyChild;
            if (!result)
            {
                ignoreList.Add(item);
            }

            item.IsExpanded = result;
            return result;
        }

        protected override bool FilterCheckable(CheckableBase obj)
        {
            return base.FilterCheckable(obj) && !obj.IsChecked;
        }
    }
}