﻿using Diagnosis.Common.Util;
using System;
using System.Collections.Generic;
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

        //[LocalizableDescription(@"Sorting_Date")]
        //GroupingDate,
        [LocalizableDescription(@"Sorting_CreatedAt")]
        GroupingCreatedAt
    }
}