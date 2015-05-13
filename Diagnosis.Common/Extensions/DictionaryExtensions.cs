using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Common
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Return dict value or default value of TVal or empty collection.
        /// </summary>
        public static TVal GetValueOrDefault<TKey, TVal>(this IDictionary<TKey, TVal> dict, TKey key) where TVal : new()
        {

            TVal result;
            if (dict.TryGetValue(key, out result))
                return result;
            else
            {
                if (typeof(IEnumerable).IsAssignableFrom(typeof(TVal)))
                {
                    return new TVal();
                }
                return default(TVal);
            }
        }
        /// <summary>
        /// Return dict value or empty string.
        /// </summary>
        public static string GetValueOrDefault<TKey>(this IDictionary<TKey, string> dict, TKey key)
        {

            string result;
            if (dict.TryGetValue(key, out result))
                return result;
            else
            {
                return string.Empty;
            }
        }

        public static Dictionary<T1, IEnumerable<T2>> ReverseManyToMany<T1, T2>(this Dictionary<T2, IEnumerable<T1>> crps)
        {
            var table = crps
                .SelectMany(
                    x => x.Value,
                    (key, entry) => new
                    {
                        T2 = key.Key,
                        T1 = entry
                    }
                );

            var reversed = table
                .GroupBy(x => x.T1,
                        x => x.T2,
                        (t1, t2s) => new { T2s = t2s, T1 = t1 })
                .ToDictionary(x => x.T1, x => x.T2s);
            return reversed;
        }
    }
}
