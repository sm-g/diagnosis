using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels.Screens
{
    public abstract class ScreenBaseViewModel : PaneViewModel
    {
        public static string ScreenContentId = "Screen";

        public ScreenBaseViewModel()
        {
            ContentId = "Screen";
        }
    }
}
