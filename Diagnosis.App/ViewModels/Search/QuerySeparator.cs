using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class QuerySeparator
    {
        public readonly char Delimiter;
        public readonly char Spacer;
        public readonly string DelimGroup;

        private static readonly Lazy<QuerySeparator> lazyInstance = new Lazy<QuerySeparator>(() => new QuerySeparator());
        private string delimeterEsc;

        public static QuerySeparator Default
        {
            get { return lazyInstance.Value; }
        }

        /// <summary>
        /// Удаляет лишние разделительные символы. Delimiter меняет на DelimGroup.
        /// Например, "абв..  где.  .ё." → "абс. где. ё. "
        /// </summary>
        public string TrimExcessDelimiters(string value)
        {
            Console.WriteLine("before trim '{0}'", value);

            // оставляем по одному пробелу
            var trimed = Regex.Replace(value, @"\s+", Spacer.ToString());
            // повторные группы разделительных символов заменяем на одну группу
            trimed = Regex.Replace(trimed, @"[\s" + delimeterEsc + "]+", DelimGroup);

            Console.WriteLine("after trim '{0}'", trimed);
            return trimed;
        }

        /// <summary>
        /// Заменяет DelimGroup на Delimiter.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string RemoveSpacers(string value)
        {
            return value.Replace(DelimGroup, Delimiter.ToString());
        }

        /// <summary>
        /// Если в конце строки Delimiter, добавляет к нему Spacer.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string RestoreLastDelimGroup(string value)
        {
            if (value.LastOrDefault() == Delimiter)
            {
                return value += Spacer;
            }
            return value;
        }

        public QuerySeparator(char spacer = ' ', char delimiter = '.')
        {
            Delimiter = delimiter;
            Spacer = spacer;
            DelimGroup = delimiter.ToString() + spacer;
            delimeterEsc = Regex.Escape(delimiter.ToString());
        }
    }
}