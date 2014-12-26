using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Diagnosis.Common
{
    public static class StringExtensions
    {
        /// <summary>
        /// Trims string and returns null if result is empty.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TrimedOrNull(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }
            else
            {
                return str.Trim();
            }
        }

        [Pure]
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// "310" -> {0,1,3}
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static IEnumerable<int> ToDigits(this string str)
        {
            return str.ToCharArray().Select(ch => int.Parse(ch.ToString()));
        }

        public static bool MatchesAsStrings(this string str, object obj)
        {
            if (str == null || obj == null)
                return false;
            return str.ToLowerInvariant() == obj.ToString().ToLowerInvariant(); // TODO use everywhere
        }

        public static int CompareToNullSafe(this string one, string two)
        {
            if (one == null ^ two == null)
            {
                return (one == null) ? -1 : 1;
            }

            if (one == null && two == null)
            {
                return 0;
            }

            return one.CompareTo(two);
        }
    }
}
