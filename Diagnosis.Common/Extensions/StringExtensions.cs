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
    }
}
