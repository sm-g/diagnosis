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
        public bool AllChildren;

        public HierarchicalSearchSettings(bool withNonCheckable, bool withChecked, bool withCreatingNew, bool allChildren)
        {
            WithNonCheckable = withNonCheckable;
            WithChecked = withChecked;
            WithCreatingNew = withCreatingNew;
            AllChildren = allChildren;
        }
    }
}
