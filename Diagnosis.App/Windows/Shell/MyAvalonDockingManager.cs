using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xceed.Wpf.AvalonDock;

namespace Diagnosis.App.Windows.Shell
{
    public class MyAvalonDockingManager : DockingManager
    {
        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {

            base.OnPreviewKeyDown(e);
        }
    }
}
