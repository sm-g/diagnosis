using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels.Search
{
    public struct HierarchicalSearchSettings
    {
        public bool WithNonCheckable;

        public HierarchicalSearchSettings(bool withNonCheckable)
        {
            WithNonCheckable = withNonCheckable;
        }
    }
}
