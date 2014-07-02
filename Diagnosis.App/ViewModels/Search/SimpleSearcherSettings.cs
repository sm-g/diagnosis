using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.App.ViewModels
{
    public struct SimpleSearcherSettings
    {
        public bool WithNonCheckable;
        public bool WithChecked;
        public bool WithCreatingNew;
        public bool AllChildren;

        public SimpleSearcherSettings(bool withNonCheckable, bool withChecked, bool withCreatingNew, bool allChildren)
        {
            WithNonCheckable = withNonCheckable;
            WithChecked = withChecked;
            WithCreatingNew = withCreatingNew;
            AllChildren = allChildren;
        }
    }

    public struct AllChidrenSearcherSettings : SimpleSearcherSettings
    {
        public bool WithNonCheckable;
        public bool WithChecked;
        public bool WithCreatingNew;
        public bool AllChildren;

        public AllChidrenSearcherSettings(bool withNonCheckable, bool withChecked, bool withCreatingNew, bool allChildren)
        {
            WithNonCheckable = withNonCheckable;
            WithChecked = withChecked;
            WithCreatingNew = withCreatingNew;
            AllChildren = allChildren;
        }
    }
}
