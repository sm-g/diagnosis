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

        public SearcherSettings(bool withNonCheckable = false, bool withChecked = false, bool withCreatingNew = true, bool allChildren = true)
        {
            AllChildren = allChildren;
            WithNonCheckable = withNonCheckable;
            WithChecked = withChecked;
            WithCreatingNew = withCreatingNew;
        }
    }
}
