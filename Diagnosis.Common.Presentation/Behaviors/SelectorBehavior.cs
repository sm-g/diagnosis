using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Diagnosis.Common.Presentation.Behaviors
{
    // http://stackoverflow.com/a/3921384/3009578
    public class SelectorBehavior
    {

        public static bool GetKeepSelection(DependencyObject obj)
        {
            return (bool)obj.GetValue(KeepSelectionProperty);
        }

        public static void SetKeepSelection(DependencyObject obj, bool value)
        {
            obj.SetValue(KeepSelectionProperty, value);
        }

        public static readonly DependencyProperty KeepSelectionProperty =
            DependencyProperty.RegisterAttached("KeepSelection", typeof(bool), typeof(SelectorBehavior),
                new UIPropertyMetadata(false, new PropertyChangedCallback(onKeepSelectionChanged)));

        static void onKeepSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var selector = d as Selector;
            var value = (bool)e.NewValue;
            if (value)
            {
                selector.SelectionChanged += selector_SelectionChanged;
            }
            else
            {
                selector.SelectionChanged -= selector_SelectionChanged;
            }
        }

        static void selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selector = sender as Selector;

            if (e.RemovedItems.Count > 0)
            {
                var deselectedItem = e.RemovedItems[0];
                if (selector.SelectedItem == null)
                {
                    selector.SelectedItem = deselectedItem;
                    e.Handled = true;
                }
            }
        }
    }
}
