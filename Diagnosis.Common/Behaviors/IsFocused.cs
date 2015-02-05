using System;
using System.Windows;
using System.Windows.Input;

namespace Diagnosis.Common.Behaviors
{
    // see http://stackoverflow.com/questions/1356045
    public static class FocusExtension
    {
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached("IsFocused", typeof(bool?), typeof(FocusExtension),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsFocusedChanged));

        /// <summary>
        /// When element got logic focus, call Focus() only if it has keyboard focus.
        /// </summary>
        public static readonly DependencyProperty DirectProperty =
            DependencyProperty.RegisterAttached("Direct", typeof(bool), typeof(FocusExtension));

        public static bool? GetIsFocused(DependencyObject element)
        {
            return (bool?)element.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject element, bool? value)
        {
            element.SetValue(IsFocusedProperty, value);
        }

        public static bool GetDirect(DependencyObject element)
        {
            return (bool)element.GetValue(DirectProperty);
        }

        public static void SetDirect(DependencyObject element, bool? value)
        {
            element.SetValue(DirectProperty, value);
        }

        private static void IsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fe = (FrameworkElement)d;

            if (e.OldValue == null)
            {
                fe.GotFocus += FrameworkElement_GotFocus;
                fe.LostFocus += FrameworkElement_LostFocus;
            }

            //if (!fe.IsVisible)
            //{
            //    fe.IsVisibleChanged += new DependencyPropertyChangedEventHandler(fe_IsVisibleChanged);
            //}

            if (e.NewValue != null && (bool)e.NewValue)
            {
                fe.Focus();
            }
        }

        //private static void fe_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    var fe = (FrameworkElement)sender;
        //    var value = fe.GetValue(IsFocusedProperty);
        //    if (value != null)
        //        if (fe.IsVisible && (bool)fe.GetValue(IsFocusedProperty))
        //        {
        //            fe.IsVisibleChanged -= fe_IsVisibleChanged;
        //            fe.Focus();
        //        }
        //}

        private static void FrameworkElement_GotFocus(object sender, RoutedEventArgs e)
        {
            if (e.Source == e.OriginalSource)
                if (!GetDirect(sender as DependencyObject) || Keyboard.FocusedElement == sender)
                    ((FrameworkElement)sender).SetValue(IsFocusedProperty, true);
        }

        private static void FrameworkElement_LostFocus(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)sender).SetValue(IsFocusedProperty, false);
        }
    }
}