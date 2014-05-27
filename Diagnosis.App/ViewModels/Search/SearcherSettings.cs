using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.App.ViewModels
{
    public struct SearcherSettings
    {
        public bool WithNonCheckable;
        public bool WithChecked;
        public bool WithCreatingNew;
        public bool AllChildren;

        public SearcherSettings(bool withNonCheckable, bool withChecked, bool withCreatingNew, bool allChildren)
        {
            WithNonCheckable = withNonCheckable;
            WithChecked = withChecked;
            WithCreatingNew = withCreatingNew;
            AllChildren = allChildren;
        }
    }
}
