using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xceed.Wpf.AvalonDock;

namespace Diagnosis.App.Windows.Shell
{
    public class MyDockingManager : DockingManager
    {
        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            // don't Show NavigatorWindow
        }
    }
}
