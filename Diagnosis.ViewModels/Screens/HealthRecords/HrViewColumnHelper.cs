using Diagnosis.Models.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels.Screens
{
    public static class HrViewColumnHelper
    {
        /// <summary>
        /// Свойство для сортировки по колонке.
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        [Pure]
        public static string ToSortingProperty(this HrViewColumn col)
        {
            switch (col)
            {
                case HrViewColumn.Category:
                case HrViewColumn.CreatedAt:
                case HrViewColumn.DescribedAt:
                case HrViewColumn.Ord:
                    return col.ToString();

                case HrViewColumn.Date:
                    return "SortingDate";

                case HrViewColumn.None:
                default:
                    return null;
            }
        } /// <summary>
        /// Свойство для группировки по колонке.
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        [Pure]
        public static string ToGroupingProperty(this HrViewColumn col)
        {
            switch (col)
            {
                case HrViewColumn.Category:
                    return col.ToString();

                case HrViewColumn.CreatedAt:
                    return "GroupingCreatedAt";

                case HrViewColumn.None:
                default:
                    return null;
            }
        }
    }
}
