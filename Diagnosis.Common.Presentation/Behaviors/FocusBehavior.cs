using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Diagnosis.Common.Presentation.Behaviors
{
    public static partial class FocusBehavior
    {
        public static readonly DependencyProperty FocusFirstProperty =
            DependencyProperty.RegisterAttached(
                "FocusFirst",
                typeof(bool),
                typeof(FocusBehavior),
                new PropertyMetadata(false, OnFocusFirstPropertyChanged));

        public static readonly DependencyProperty FocusablePanelProperty =
           DependencyProperty.RegisterAttached(
               "FocusablePanel",
               typeof(bool),
               typeof(FocusBehavior),
               new PropertyMetadata(false, OnFocusablePanelPropertyChanged));


        public static bool GetFocusFirst(Control control)
        {
            return (bool)control.GetValue(FocusFirstProperty);
        }

        public static void SetFocusFirst(Control control, bool value)
        {
            control.SetValue(FocusFirstProperty, value);
        }
        public static bool GetFocusablePanel(Panel panel)
        {
            return (bool)panel.GetValue(FocusablePanelProperty);
        }

        public static void SetFocusablePanel(Panel panel, bool value)
        {
            panel.SetValue(FocusablePanelProperty, value);
        }


        static void OnFocusFirstPropertyChanged(
            DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var element = obj as FrameworkElement;
            if (element == null || !(args.NewValue is bool))
            {
                return;
            }

            if ((bool)args.NewValue)
            {
                element.Loaded += (sender, e) =>
                    element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
        private static void OnFocusablePanelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var panel = d as Panel;
            if (panel == null || !(args.NewValue is bool))
            {
                return;
            }

            if ((bool)args.NewValue)
            {
                panel.Focusable = true;
                panel.Background = Brushes.Transparent;
                panel.MouseDown += (s, e) =>
                {
                    panel.Focus();
                };
            }
        }
    }
}
