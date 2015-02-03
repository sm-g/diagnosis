using System.Windows;
using System.Linq;
using System.Windows.Media;
using System.Collections.Generic;

namespace Diagnosis.Common.Controls
{
    public static class VisualTreeExtensions
    {
        /// <summary>
        /// Finds a Child of a given item in the visual tree.
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childNameOrAny">x:Name or Name of child or return first child of given type. </param>
        /// <returns>The first parent item that matches the submitted type parameter.
        /// If not matching item can be found,
        /// a null parent is being returned.</returns>
        public static T FindChild<T>(this FrameworkElement parent, string childNameOrAny = null)
           where T : FrameworkElement
        {
            if (!string.IsNullOrEmpty(childNameOrAny))
                return FindVisualChildren<T>(parent, true)
                    .FirstOrDefault(x => x.Name == childNameOrAny);
            else
                return FindVisualChildren<T>(parent, true).FirstOrDefault();

        }
        /// <summary>
        /// Return collection of all children of queried type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject parent, bool withInherited = true) where T : DependencyObject
        {
            if (parent != null)
            {
                var childCount = VisualTreeHelper.GetChildrenCount(parent);

                for (int i = 0; i < childCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }
                    if (withInherited)
                        foreach (T childOfChild in FindVisualChildren<T>(child, withInherited))
                        {
                            yield return childOfChild;
                        }
                }
            }
            yield break;
        }

        /// <summary>
        /// Return collection of children of any type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> FindVisualChildren(this DependencyObject parent, bool withInherited = true)
        {
            return FindVisualChildren<DependencyObject>(parent, withInherited);
        }
        /// <summary>
        /// This method is an alternative to WPF's
        /// <see cref="VisualTreeHelper.GetParent"/> method, which also
        /// supports content elements. Keep in mind that for content element,
        /// this method falls back to the logical tree of the element!
        /// </summary>
        /// <param name="child">The item to be processed.</param>
        /// <returns>The submitted item's parent, if available. Otherwise
        /// null.</returns>
        public static DependencyObject GetParent(this DependencyObject child)
        {
            if (child == null)
                return null;

            //handle content elements separately
            ContentElement ce = child as ContentElement;
            if (ce != null)
            {
                DependencyObject parent = ContentOperations.GetParent(ce);
                if (parent != null)
                    return parent;

                FrameworkContentElement fce = ce as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }
            //also try searching for parent in framework elements (such as DockPanel, etc)
            FrameworkElement frameworkElement = child as FrameworkElement;
            if (frameworkElement != null)
            {
                DependencyObject parent = frameworkElement.Parent;
                if (parent != null) return parent;
            }

            return VisualTreeHelper.GetParent(child);
        }

        public static T FindAncestorOrSelf<T>(this DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                T objTest = child as T;
                if (objTest != null)
                    return objTest;
                child = GetParent(child);
            }

            return null;
        }

        public static T FindAncestor<T>(this DependencyObject child) where T : DependencyObject
        {
            child = GetParent(child);
            return FindAncestorOrSelf<T>(child);
        }

        /// <summary>
        /// Return true if one DependencyObject is visual child of other.
        /// </summary>
        /// <param name="dep"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static bool IsChildOf(this DependencyObject dep, DependencyObject parent)
        {
            if (parent == null)
                return false;

            while (dep != null)
            {
                dep = GetParent(dep);
                if (dep == parent)
                    return true;
            }
            return false;
        }

        public static bool IsChildOf(this IInputElement input, DependencyObject parent)
        {
            return IsChildOf(input as DependencyObject, parent);
        }
    }
}