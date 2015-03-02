using Diagnosis.Client.App.Windows;
using System;
using System.Windows;

namespace Diagnosis.Client.App.Behaviors
{
    /// <summary>
    /// from http://stackoverflow.com/a/12254217/3009578
    /// </summary>
    public static class ActivateBehavior
    {
        public static bool? GetIsActive(DependencyObject obj)
        {
            return (bool?)obj.GetValue(IsActiveProperty);
        }

        public static void SetIsActive(DependencyObject obj, bool value)
        {
            obj.SetValue(IsActiveProperty, value);
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.RegisterAttached("IsActive", typeof(bool?), typeof(ActivateBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnActivatedChanged));

        private static void OnActivatedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var win = (Window)dependencyObject;

            if (e.OldValue == null)
            {
                win.Activated += OnActivated;
                win.Deactivated += OnDeactivated;
            }
            if (e.NewValue == null)
            {
                win.Activated -= OnActivated;
                win.Deactivated -= OnDeactivated;
                return;
            }

            if ((bool)e.NewValue)
            {
                if (!(GetIsActive(win) ?? false) || win.IsActive)
                    return;

                // The Activated property is set to true but the Activated event hasn't been fired.
                win.BringToFront();
            }
        }

        private static void OnActivated(Object sender, EventArgs eventArgs)
        {
            ((Window)sender).SetValue(IsActiveProperty, true);
        }

        private static void OnDeactivated(Object sender, EventArgs eventArgs)
        {
            ((Window)sender).SetValue(IsActiveProperty, false);
        }
    }
}