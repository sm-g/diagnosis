using Diagnosis.Common.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Diagnosis.Models.Enums
{
    public enum HrViewSortingColumn
    {
#if DEBUG

        [LocalizableDescription(@"Sorting_None")]
        None,

#endif

        [LocalizableDescription(@"Sorting_Ord")]
        Ord,

        [LocalizableDescription(@"Sorting_Category")]
        Category,

        [LocalizableDescription(@"Sorting_Date")]
        SortingDate,

        [LocalizableDescription(@"Sorting_CreatedAt")]
        CreatedAt
    }

    public enum HrViewGroupingColumn
    {
        [LocalizableDescription(@"Sorting_None")]
        None,

        [LocalizableDescription(@"Sorting_Category")]
        Category,

        [LocalizableDescription(@"Sorting_CreatedAt")]
        GroupingCreatedAt
    }

    public static class HrViewConverter
    {
        public static HrViewGroupingColumn ToGroupingColumn(this HrViewSortingColumn col)
        {
            switch (col)
            {
                case HrViewSortingColumn.Category:
                    return HrViewGroupingColumn.Category;

                case HrViewSortingColumn.CreatedAt:
                    return HrViewGroupingColumn.GroupingCreatedAt;

                case HrViewSortingColumn.SortingDate:
                default:
                    return HrViewGroupingColumn.None;
            }
        }

        public static HrViewSortingColumn? ToSortingColumn(this HrViewGroupingColumn col)
        {
            switch (col)
            {
                case HrViewGroupingColumn.Category:
                    return HrViewSortingColumn.Category;

                case HrViewGroupingColumn.GroupingCreatedAt:
                    return HrViewSortingColumn.CreatedAt;

                case HrViewGroupingColumn.None:
                default:
                    return null;
            }
        }
        /// <summary>
        /// Свойство для сортировки при группировке по колонке.
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        [Pure]
        public static string ToSortingProperty(this HrViewGroupingColumn col)
        {
            switch (col)
            {
                case HrViewGroupingColumn.Category:
                    return col.ToString();

                case HrViewGroupingColumn.GroupingCreatedAt:
                    return col.ToString();

                case HrViewGroupingColumn.None:
                default:
                    return null;
            }
        }
    }
}
