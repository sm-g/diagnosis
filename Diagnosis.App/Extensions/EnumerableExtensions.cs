using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Diagnosis.App.ViewModels
{
    static class EnumerableExtensions
    {
        public static bool IsSubsetOf<T>(this IEnumerable<T> x, IEnumerable<T> y)
        {
            bool isSubset = !x.Except(y).Any();
            return isSubset;
        }

        public static void ForBranch<T>(this IList<T> list, Action<T> action) where T : class, IHierarchical<T>
        {
            Contract.Assume(list.Count > 0);

            foreach (var item in list[0].Parent.AllChildren)
            {
                action(item);
            }
        }

        public static void ForAll<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }
    }
}
