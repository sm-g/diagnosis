using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace Diagnosis.Core
{
    public class QuerySeparator
    {
        public readonly char Delimiter;
        public readonly char Spacer;
        public readonly string DelimGroup;

        private static readonly Lazy<QuerySeparator> lazyInstance = new Lazy<QuerySeparator>(() => new QuerySeparator());
        private readonly string repeatingRegex;

        public static QuerySeparator Default
        {
            get { return lazyInstance.Value; }
        }

        /// <summary>
        /// Расставляет разделительные символы в нужном формате, удаляя лишие. Delimiter меняет на DelimGroup.
        /// Например, "а.. б г .ё." → "а. б г. ё. "
        /// </summary>
        public string FormatDelimiters(string value)
        {
            // оставляем по одному пробелу
            var trimed = Regex.Replace(value, @"\s+", Spacer.ToString());
            // повторные группы разделительных символов заменяем на одну группу
            trimed = Regex.Replace(trimed, repeatingRegex, DelimGroup);

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

            var delimiterEsc = Regex.Escape(delimiter.ToString());
            var spacerEsc = Regex.Escape(spacer.ToString());

            repeatingRegex = string.Format(@"{0}*{1}[{0}{1}]*", spacerEsc, delimiterEsc);
        }
    }
}