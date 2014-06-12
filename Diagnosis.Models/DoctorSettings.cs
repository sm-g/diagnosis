using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    [Flags]
    public enum DoctorSettings
    {
        None = 0,
        ShowIcdDisease = 1,
        OnlyTopLevelIcdDisease = 2
    }

}
