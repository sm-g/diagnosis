using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Core
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
    }
}
