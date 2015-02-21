using Diagnosis.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models.Enums
{
    [Flags]
    public enum PatientsViewSortingColumn
    {
        None = 0,
        FullNameOrCreatedAt = 1,
        IsMale = 2,
        Age = 4,
        LastHrUpdatedAt = 8,
    }
}
