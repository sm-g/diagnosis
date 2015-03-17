using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Diagnosis.Common.Presentation.Behaviors
{
    // from http://stackoverflow.com/questions/501886/how-should-the-viewmodel-close-the-form
    public static class DialogCloser
    {
        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.RegisterAttached(
                "DialogResult",
                typeof(bool?),
                typeof(DialogCloser),
                new PropertyMetadata(DialogResultChanged));

        private static void DialogResultChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;
            if (window != null)
            {
                window.DialogResult = e.NewValue as bool?;
                return;
            }
            var chwindow = d as Xceed.Wpf.Toolkit.ChildWindow;
            if (chwindow != null)
            {
                chwindow.DialogResult = e.NewValue as bool?;
                return;
            }
        }
        public static void SetDialogResult(DependencyObject target, bool? value)
        {
            target.SetValue(DialogResultProperty, value);
        }


    }
}
