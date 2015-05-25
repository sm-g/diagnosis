using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.IO;

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
                return null;
            else
                return str.Trim();
        }
        /// <summary>
        /// Removes \t \n, trims spaces. Null values ok.
        /// </summary>        
        public static string Prettify(this string str)
        {
            if (str == null) return null;
            return str.Replace(Environment.NewLine, " ").Replace('\t', ' ').Trim();
        }

        [Pure]
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool MatchesAsStrings(this string str, object obj)
        {
            if (str == null || obj == null)
                return false;
            return str.Equals(obj.ToString(), StringComparison.OrdinalIgnoreCase);
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

        public static MemoryStream ToMemoryStream(this string str, Encoding encoding = null)
        {
            var enc = encoding ?? Encoding.Unicode;
            return new MemoryStream(enc.GetBytes(str));
        }

        public static string FormatStr(this string str, params object[] args)
        {
            Contract.Requires(str != null);
            return string.Format(str, args);
        }

        public static string Truncate(this string str, int maxLength)
        {
            if (str != null && str.Length > maxLength)
                str = str.Substring(0, maxLength);

            return str;
        }
    }
}
