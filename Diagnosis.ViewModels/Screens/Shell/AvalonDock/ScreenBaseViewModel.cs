using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Diagnosis.ViewModels.Screens
{
    /// <summary>
    /// Ёкран (авалон-документ).
    /// </summary>
    public abstract class ScreenBaseViewModel : PaneViewModel
    {
        public const string ScreenContentId = "Screen";

        public ScreenBaseViewModel()
        {
            ContentId = "Screen";
        }

        protected void DoWithCursor(Task act, Cursor cursor)
        {
            Mouse.OverrideCursor = cursor;
            act.ContinueWith((t) =>
            {
                uiTaskFactory.StartNew(() =>
                    Mouse.OverrideCursor = null);
            });
        }
    }
}
