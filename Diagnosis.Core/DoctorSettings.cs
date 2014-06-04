using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Core
{
    [Flags]
    public enum DoctorSettings
    {
        None = 0,
        OnlyTopLevelIcdDisease = 1
    }

}
