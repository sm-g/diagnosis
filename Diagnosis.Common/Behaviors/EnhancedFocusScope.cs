using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Diagnosis.Common.Behaviors
{
    /// <summary>
    /// Attached behavior for improved focus scope handling.
    /// http://www.codeproject.com/Articles/38507/Using-the-WPF-FocusScope
    /// </summary>
    public static class EnhancedFocusScope
    {
        public static void SetFocusOnActiveElementInScope(UIElement scope)
        {
            IInputElement focusedElement = FocusManager.GetFocusedElement(scope);
            if (focusedElement != null)
            {
                Keyboard.Focus(focusedElement);
            }
            else
            {
                scope.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            }
        }

        public static bool GetIsEnhancedFocusScope(UIElement element)
        {
            return (bool)element.GetValue(IsEnhancedFocusScopeProperty);
        }

        public static void SetIsEnhancedFocusScope(UIElement element, bool value)
        {
            element.SetValue(IsEnhancedFocusScopeProperty, value);
        }

        public static readonly DependencyProperty IsEnhancedFocusScopeProperty =
            DependencyProperty.RegisterAttached(
                "IsEnhancedFocusScope",
                typeof(bool),
                typeof(EnhancedFocusScope),
                new UIPropertyMetadata(false, OnIsEnhancedFocusScopeChanged));

        private static void OnIsEnhancedFocusScopeChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            UIElement item = depObj as UIElement;
            if (item == null)
                return;

            if ((bool)e.NewValue)
            {
                FocusManager.SetIsFocusScope(item, true);
                item.GotKeyboardFocus += OnGotKeyboardFocus;
            }
            else
            {
                FocusManager.SetIsFocusScope(item, false);
                item.GotKeyboardFocus -= OnGotKeyboardFocus;
            }
        }

        private static void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            IInputElement focusedElement = e.NewFocus;
            for (DependencyObject d = focusedElement as DependencyObject; d != null; d = VisualTreeHelper.GetParent(d))
            {
                if (FocusManager.GetIsFocusScope(d))
                {
                    d.SetValue(FocusManager.FocusedElementProperty, focusedElement);
                    if (!(bool)d.GetValue(IsEnhancedFocusScopeProperty))
                    {
                        break;
                    }
                }
            }
        }
    }
}