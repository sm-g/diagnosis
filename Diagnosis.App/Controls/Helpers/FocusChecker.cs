using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Diagnosis.App.Controls
{
    public class FocusChecker
    {
        public static bool IsFocusOutsideDepObject<T>(T obj) where T : DependencyObject
        {
            var element = FocusManager.GetFocusedElement(Window.GetWindow(obj));
            return ChildFinder.FindVisualChildren(obj).FirstOrDefault(child => child == element) == null;
        }
    }
}