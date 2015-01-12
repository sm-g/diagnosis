using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;

namespace Diagnosis.App.Controls
{
    internal static class VisualTreeExtensions
    {
        /// <summary>
        /// Finds a Child of a given item in the visual tree.
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter.
        /// If not matching item can be found,
        /// a null parent is being returned.</returns>
        public static T FindChild<T>(this DependencyObject parent, string childName)
           where T : DependencyObject
        {
            // Confirm parent and childName are valid.
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child.
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        /// <summary>
        /// Return collection of all children of queried type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="depObj"></param>
        /// <returns></returns>
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                var childCount = VisualTreeHelper.GetChildrenCount(depObj);

                for (int i = 0; i < childCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
            yield break;
        }

        /// <summary>
        /// Return collection of all children of any type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="depObj"></param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> FindVisualChildren(this DependencyObject depObj)
        {
            if (depObj != null)
            {
                var childCount = VisualTreeHelper.GetChildrenCount(depObj);

                for (int i = 0; i < childCount; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null)
                    {
                        yield return child;
                    }

                    foreach (var childOfChild in FindVisualChildren(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
            yield break;
        }

        public static DependencyObject GetParent(this DependencyObject obj)
        {
            if (obj == null)
                return null;

            ContentElement ce = obj as ContentElement;
            if (ce != null)
            {
                DependencyObject parent = ContentOperations.GetParent(ce);
                if (parent != null)
                    return parent;

                FrameworkContentElement fce = ce as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            return VisualTreeHelper.GetParent(obj);
        }

        public static T FindAncestorOrSelf<T>(this DependencyObject obj) where T : DependencyObject
        {
            while (obj != null)
            {
                T objTest = obj as T;
                if (objTest != null)
                    return objTest;
                obj = GetParent(obj);
            }

            return null;
        }

        public static T FindAncestor<T>(this DependencyObject obj) where T : DependencyObject
        {
            obj = GetParent(obj);
            while (obj != null)
            {
                T objTest = obj as T;
                if (objTest != null)
                    return objTest;
                obj = GetParent(obj);
            }

            return null;
        }

        /// <summary>
        /// Return true if one DependencyObject is visual child of other.
        /// </summary>
        /// <param name="dep"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public static bool IsChildOf(this DependencyObject dep, DependencyObject container)
        {
            if (container == null)
                return false;

            while (dep != null)
            {
                dep = GetParent(dep);
                if (dep == container)
                    return true;
            }
            return false;
        }

        public static bool IsChildOf(this IInputElement input, DependencyObject container)
        {
            if (container == null)
                return false;

            var dep = input as DependencyObject;
            while (dep != null)
            {
                dep = GetParent(dep);
                if (dep == container)
                    return true;
            }
            return false;
        }
    }
}