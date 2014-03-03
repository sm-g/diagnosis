using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Diagnosis.Helpers
{
    public class FocusChecker
    {
        public static bool IsFocusOutsideDepObject<T>(T obj) where T : DependencyObject
        {
            var element = FocusManager.GetFocusedElement(Application.Current.MainWindow);
            return ChildFinder.FindVisualChildren(obj).FirstOrDefault(child => child == element) == null;
        }
    }
}