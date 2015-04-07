using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Common
{

    /// <summary>
    /// Returns -1 instead of 1 if y is IsNullOrWhiteSpace when x is Not.
    /// </summary>
    public class EmptyStringsAreLast : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (String.IsNullOrWhiteSpace(y) && !String.IsNullOrWhiteSpace(x))
            {
                return -1;
            }
            else if (!String.IsNullOrWhiteSpace(y) && String.IsNullOrWhiteSpace(x))
            {
                return 1;
            }
            else
            {
                return String.Compare(x, y);
            }
        }
    }

    public class KeyEqualityComparer<T, TKey> : IEqualityComparer<T>
    {
        protected readonly Func<T, TKey> keyExtractor;

        public KeyEqualityComparer(Func<T, TKey> keyExtractor)
        {
            this.keyExtractor = keyExtractor;
        }

        public virtual bool Equals(T x, T y)
        {
            return this.keyExtractor(x).Equals(this.keyExtractor(y));
        }

        public int GetHashCode(T obj)
        {
            return this.keyExtractor(obj).GetHashCode();
        }
    }

    public class StrictKeyEqualityComparer<T, TKey> : KeyEqualityComparer<T, TKey>
        where TKey : IEquatable<TKey>
    {
        public StrictKeyEqualityComparer(Func<T, TKey> keyExtractor)
            : base(keyExtractor)
        { }

        public override bool Equals(T x, T y)
        {
            // This will use the overload that accepts a TKey parameter
            // instead of an object parameter.
            return this.keyExtractor(x).Equals(this.keyExtractor(y));
        }
    }

}
