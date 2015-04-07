using Diagnosis.Common.Attributes;
using Diagnosis.Common.Util;
using System;
using System.Linq;

namespace Diagnosis.Models.Enums
{
    public enum HrViewColumn
    {
        [LocalizableDescription(@"Sorting_None")]
        None,

        [LocalizableDescription(@"Sorting_Ord")]
        Ord,

        [LocalizableDescription(@"Sorting_Category")]
        Category,

        [LocalizableDescription(@"Sorting_Date")]
        Date,

        [LocalizableDescription(@"Sorting_CreatedAt")]
        CreatedAt
    }
}