using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Diagnosis.Client.App.Windows
{
    public static class WindowHelper
    {
        /// <summary>
        /// from http://stackoverflow.com/a/4831839/3009578
        /// </summary>
        /// <param name="win"></param>
        public static void BringToFront(this Window win)
        {
            if (!win.IsVisible)
            {
                win.Show();
            }

            if (win.WindowState == WindowState.Minimized)
            {
                win.WindowState = WindowState.Normal;
            }

            win.Activate();
            win.Topmost = true;  // important
            win.Topmost = false; // important
            win.Focus();         // important
        }
    }
}
