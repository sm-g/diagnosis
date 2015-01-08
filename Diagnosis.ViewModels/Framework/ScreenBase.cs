using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels.Screens
{
    public abstract class ScreenBase : PaneViewModel
    {
        public ScreenBase()
        {
            ContentId = "Screen";
        }
    }
}
