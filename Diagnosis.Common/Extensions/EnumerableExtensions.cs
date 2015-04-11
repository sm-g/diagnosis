using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

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

        public static IEnumerable<T> ToEnumerable<T>(this T item)
        {
            return new[] { item };
        }
        public static IEnumerable<T> Except<T>(this IEnumerable<T> collection, T item)
        {
            return collection.Except(item.ToEnumerable());
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
        /// Добавляет в коллекцию отсутствующие и удаляет пропавшие элементы c сохранением порядка.
        /// </summary>
        public static void SyncWith<T, TKey>(this ObservableCollection<T> current, IEnumerable<T> toBe, Func<T, TKey> keyExtractor)
            where TKey : IComparable<TKey>
        {
            foreach (var item in current.Except(toBe).ToList())
            {
                current.Remove(item);
            }
            foreach (var item in toBe.Except(current).ToList())
            {
                current.Add(item);
            }
            current.Sort(keyExtractor);
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
        /// Возвращает первый элемент, которого нет во втором списке, рядом с первым элементом,
        /// равным <paramref name="startIndex"/>-му во втором списке. Если такого элемента нет,
        /// возвращается просто первый элемент списка. Возвращает null, если все элементы есть во
        /// втором списке.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first">     </param>
        /// <param name="second">    </param>
        /// <param name="startIndex">Индекс, с которого начинается поиск.</param>
        /// <param name="forward">
        /// Направление, с которого начинается поиск. По умолчанию вперед, к концу списка.
        /// </param>
        /// <returns></returns>
        public static T FirstAfterAndNotIn<T>(this IList<T> first, IList<T> second, int startIndex = 0, bool forward = true) where T : class
        {
            if (second.Count == 0 || first.Count == 0 || second.Count - 1 < startIndex || startIndex < 0)
                return first.FirstOrDefault();

            int ind = first.IndexOf(second[startIndex]);
            if (ind < 0)
                return first.FirstOrDefault();

            if (forward)
            {
                while (second.Contains(first[ind]) && ind < first.Count - 1)
                    ind++;
                while (second.Contains(first[ind]) && ind > 0)
                    ind--;
            }
            else
            {
                while (second.Contains(first[ind]) && ind > 0)
                    ind--;
                while (second.Contains(first[ind]) && ind < first.Count - 1)
                    ind++;
            }
            if (second.Contains(first[ind]))
                return null;

            return first.ElementAt(ind);
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
        public static void AddSorted<T, TKey>(this IList<T> list, T item, Func<T, TKey> keyExtractor, bool reverse = false, IComparer<TKey> comparer = null)
            where TKey : IComparable<TKey>
        {
            comparer = comparer ?? Comparer<TKey>.Default;

            int i = 0;
            if (reverse)
                while (i < list.Count && comparer.Compare(keyExtractor(list[i]), keyExtractor(item)) > 0)
                    i++;
            else
                while (i < list.Count && comparer.Compare(keyExtractor(list[i]), keyExtractor(item)) < 0)
                    i++;

            list.Insert(i, item);
        }

        /// <summary>
        /// from http://stackoverflow.com/a/16344936/3009578
        /// </summary>
        public static void Sort<T, TKey>(this ObservableCollection<T> collection, Func<T, TKey> keyExtractor, IComparer<TKey> comparer = null)
            where TKey : IComparable<TKey>
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

        /// <summary>
        /// True if items ordered by not descending key (next >= prev).
        /// from http://stackoverflow.com/a/1940284/3009578
        /// </summary>
        [Pure]
        public static bool IsOrdered<T, TKey>(this IEnumerable<T> items, Func<T, TKey> keyExtractor)
            where TKey : IComparable<TKey>
        {
            return items.Zip(items.Skip(1), (a, b) => new { a, b })
                .All(x => !(keyExtractor(x.a).CompareTo(keyExtractor(x.b)) > 0));
        }

        /// <summary>
        /// True if items ordered by ascending key without equal keys (next > prev).
        /// </summary>
        [Pure]
        public static bool IsStrongOrdered<T, TKey>(this IEnumerable<T> items, Func<T, TKey> keyExtractor)
            where TKey : IComparable<TKey>
        {
            return items.Zip(items.Skip(1), (a, b) => new { a, b })
                .All(x => keyExtractor(x.a).CompareTo(keyExtractor(x.b)) < 0);
        }

        [Pure]
        public static bool IsSequential(this IEnumerable<int> numbers)
        {
            return numbers.Zip(numbers.Skip(1), (a, b) => (a + 1) == b).All(x => x);
        }

        /// <summary>
        /// True if two lists contain same items.
        /// from http://stackoverflow.com/questions/3669970/compare-two-listt-objects-for-equality-ignoring-order
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        [Pure]
        public static bool ScrambledEquals<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }

        public static IEnumerable<T> DistinctBy<T, TIdentity>(this IEnumerable<T> source, Func<T, TIdentity> identitySelector)
        {
            return source.Distinct(Compare.By(identitySelector));
        }
    }

    public static class Compare
    {
        public static IEqualityComparer<TSource> By<TSource, TIdentity>(Func<TSource, TIdentity> identitySelector)
        {
            return new DelegateComparer<TSource, TIdentity>(identitySelector);
        }

        private class DelegateComparer<T, TIdentity> : IEqualityComparer<T>
        {
            private readonly Func<T, TIdentity> identitySelector;

            public DelegateComparer(Func<T, TIdentity> identitySelector)
            {
                this.identitySelector = identitySelector;
            }

            public bool Equals(T x, T y)
            {
                return Equals(identitySelector(x), identitySelector(y));
            }

            public int GetHashCode(T obj)
            {
                return identitySelector(obj).GetHashCode();
            }
        }
    }
}