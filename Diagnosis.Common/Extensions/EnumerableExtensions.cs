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
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection.ToList())
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
        /// <summary>
        /// Возвращает элемент по индексу или первый/последний, если индекс за пределами.
        /// Если коллекция пуста, возарщает null.
        /// </summary>
        public static T ElementNear<T>(this IEnumerable<T> collection, int i) where T : class
        {
            var count = collection.Count();
            if (count == 0)
                return null;

            if (count <= i)
                i = count - 1;
            if (i < 0)
                i = 0;
            return collection.ElementAt(i);
        }

        /// <summary>
        /// Return {prev, current, next} triad where current mathces predicate.
        /// from http://stackoverflow.com/questions/8759849/get-previous-and-next-item-in-a-ienumerable-using-linq
        /// </summary>
        public static IEnumerable<T> FindTriad<T>(this IEnumerable<T> items, Predicate<T> matchFilling)
        {
            if (items == null)
                throw new ArgumentNullException("items");
            if (matchFilling == null)
                throw new ArgumentNullException("matchFilling");
            Contract.EndContractBlock();

            using (var iter = items.GetEnumerator())
            {
                T previous = default(T);
                while (iter.MoveNext())
                {
                    if (matchFilling(iter.Current))
                    {
                        yield return previous;
                        yield return iter.Current;
                        if (iter.MoveNext())
                            yield return iter.Current;
                        else
                            yield return default(T);
                        yield break;
                    }
                    previous = iter.Current;
                }
            }
            // If we get here nothing has been found so return three default values
            yield return default(T); // Previous
            yield return default(T); // Current
            yield return default(T); // Next
        }

        /// <summary>
        /// from http://codereview.stackexchange.com/questions/37208/sort-observablecollection-after-added-new-item
        /// </summary>
        public static void AddSorted<T, TKey>(this IList<T> list, T item, Func<T, TKey> keyExtractor, IComparer<TKey> comparer = null)
        {
            comparer = comparer ?? Comparer<TKey>.Default;

            int i = 0;
            while (i < list.Count && comparer.Compare(keyExtractor(list[i]), keyExtractor(item)) < 0)
                i++;

            list.Insert(i, item);
        }

        /// <summary>
        /// from http://stackoverflow.com/a/16344936/3009578
        /// </summary>
        public static void Sort<T, TKey>(this ObservableCollection<T> collection, Func<T, TKey> keyExtractor, IComparer<TKey> comparer = null)
        {
            comparer = comparer ?? Comparer<TKey>.Default;

            List<T> sorted = collection.OrderBy(x => keyExtractor(x), comparer).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                var index = collection.IndexOf(sorted[i]);
                if (index != i)
                    collection.Move(index, i);
            }
        }
    }
}
