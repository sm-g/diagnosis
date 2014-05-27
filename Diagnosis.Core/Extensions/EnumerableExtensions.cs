using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Diagnosis.Core
{
    public static class EnumerableExtensions
    {
        public static bool IsSubsetOf<T>(this IEnumerable<T> x, IEnumerable<T> y)
        {
            bool isSubset = !x.Except(y).Any();
            return isSubset;
        }

        public static void ForAll<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }

        public static IEnumerable<T> ToEnumerable<T>(this T item) where T : class
        {
            return new[] { item };
        }
    }
}
