using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Common
{
    /// <summary>
    /// Генератор сочетаний из нескольких групп элементов.
    /// </summary>
    public class Combinator<T>
    {
        private static void Combine(IList<T> res, int index, List<List<T>> data, IList<IList<T>> results)
        {
            foreach (T v in data[index])
            {
                res[index] = v;
                if (index >= data.Count - 1)
                {
                    T[] copy = new T[data.Count];
                    res.CopyTo(copy, 0);
                    results.Add(copy);
                }
                else
                {
                    Combine(res, index + 1, data, results);
                }
            }
        }
        /// <summary>
        /// Возвращает все возможные сочетания из всех групп элементов по одному.
        /// 
        /// Например, из {{a,b}, {c}, {d,e}} получается {{a,c,d}, {a,c,e}, {b,c,d}, {b,c,e}}
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Combine(List<List<T>> data)
        {
            var results = new List<IList<T>>();
            Combine(new T[data.Count], 0, data, results);
            return results;
        }
    }
}
