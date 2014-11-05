using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;

namespace Diagnosis.Common
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

        public static T[] Concat<T>(this T[] x, T[] y)
        {
            var z = new T[x.Length + y.Length];
            x.CopyTo(z, 0);
            y.CopyTo(z, x.Length);
            return z;
        }

        /// <summary>
        /// Добавляет в коллекцию отсутствующие и удаляет пропавшие элементы.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="current"></param>
        /// <param name="toBe"></param>
        public static void SyncWith<T>(this ObservableCollection<T> current, IEnumerable<T> toBe)
        {
            foreach (var item in current.Except(toBe).ToList())
            {
                current.Remove(item);
            }
            foreach (var item in toBe.Except(current).ToList())
            {
                current.Add(item);
            }
        }
    }
}
