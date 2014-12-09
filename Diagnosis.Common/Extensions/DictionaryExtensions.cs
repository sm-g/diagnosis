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
    }
}
