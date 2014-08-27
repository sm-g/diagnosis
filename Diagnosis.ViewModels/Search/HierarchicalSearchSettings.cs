using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels
{
    public struct HierarchicalSearchSettings
    {
        public bool WithNonCheckable;
        public bool WithChecked;
        public bool WithCreatingNew;

        public HierarchicalSearchSettings(bool withNonCheckable, bool withChecked, bool withCreatingNew)
        {
            WithNonCheckable = withNonCheckable;
            WithChecked = withChecked;
            WithCreatingNew = withCreatingNew;
        }
    }
}
