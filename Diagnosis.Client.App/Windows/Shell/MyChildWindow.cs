using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xceed.Wpf.Toolkit;

namespace Diagnosis.Client.App.Windows.Shell
{
    public class MyChildWindow : ChildWindow
    {
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            // don't Move window
        }
    }
}
