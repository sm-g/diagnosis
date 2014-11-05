using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Common
{
    /// <summary>
    /// Выполняет разбиение данной последовательности n элементов
    /// c сохранением порядка на все возможные группы (всего 2^(n-1)).
    /// <para> </para>
    /// Например, {1, 2, 3} разбивается на<para>
    /// {{{1, 2, 3}},</para><para>
    /// {{1, 2}, {3}},</para><para>
    /// {{1}, {2, 3}},</para><para>
    /// {{1}, {2}, {3}}}.</para>
    /// {{1, 3}, {2}} не входит в разбиение.
    /// </summary>
    public static class SequencePartition
    {
        /// <summary>
        /// Разбивает последовательность на все возможные группы.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<IEnumerable<T>>> Part<T>(IEnumerable<T> sequence)
        {
            var group = new Group<T>(sequence);
            var result = new List<Line<T>>();

            Process<T>(group, result);

            result.Reverse();
            return result;
        }

        private static IEnumerable<Line<T>> PartInner<T>(Group<T> vars)
        {
            var result = new List<Line<T>>();
            if (vars.Count == 1)
            {
                result.Add(new Line<T>(vars.ToEnumerable()));
            }
            else
            {
                Process<T>(vars, result);
            }
            return result;
        }

        private static void Process<T>(Group<T> vars, List<Line<T>> result)
        {
            for (int i = vars.Count(); i > 0; i--)
            {
                var recursed = PartInner<T>(new Group<T>(vars.Skip(i)));
                if (recursed.Count() == 0)
                {
                    var gr = new Group<T>(vars.Take(i));
                    result.Add(new Line<T>(gr.ToEnumerable()));
                }
                else
                {
                    foreach (var item in recursed)
                    {
                        var l = new Line<T>(item);
                        l.Add(new Group<T>(vars.Take(i)));
                        result.Add(l);
                    }
                }
            }
        }

        /// <summary>
        /// Группа из элементов последовательности.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class Group<T> : List<T>
        {
            public Group(IEnumerable<T> e)
                : base(e)
            {
            }
        }

        /// <summary>
        /// Линия из нескольких групп.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class Line<T> : List<Group<T>>
        {
            public Line(IEnumerable<Group<T>> e)
                : base(e)
            {
            }
        }
    }

    /// <summary>
    /// Выполняет разбиение данной строки по разделителю c сохранением порядка
    /// на все возможные группы слов (всего 2^n), n — число разделителей в строке).
    /// <para> </para>
    /// Например, "порок сердца впервые" разбивается на<para>
    /// {{"порок сердца впервые"},</para><para>
    /// {"порок сердца", "впервые"},</para><para>
    /// {"порок", "сердца впервые"},</para><para>
    /// {"порок", "сердца", "впервые"}}.</para>
    /// {"порок впервые", "сердца"} не входит в разбиение.
    /// </summary>
    public static class StringSequencePartition
    {
        /// <summary>
        /// Разбивает строку на все возможные группы.
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="delim">Разделитель слов.</param>
        /// <returns></returns>
        public static IEnumerable<IList<string>> Part(string sequence, char delim = ' ')
        {
            var group = sequence.Split(delim);
            var result = new List<Line>();

            Process(group, result, delim);

            result.Reverse();
            return result;
        }

        private static IEnumerable<Line> PartInner(string chunk, char delim)
        {
            var group = chunk.Split(delim);
            var result = new List<Line>();
            if (!String.IsNullOrEmpty(chunk))
            {
                if (group.Count() == 1)
                {
                    result.Add(new Line(chunk.ToEnumerable()));
                }
                else
                {
                    Process(group, result, delim);
                }
            }
            return result;
        }

        private static string Join(char delim, IEnumerable<string> strings)
        {
            return String.Join(delim.ToString(), strings);
        }

        private static void Process(string[] group, List<Line> result, char delim)
        {
            for (int i = group.Count(); i > 0; i--)
            {
                var recursed = PartInner(Join(delim, group.Skip(i)), delim);
                if (recursed.Count() == 0)
                {
                    var gr = Join(delim, group.Take(i));
                    result.Add(new Line(gr.ToEnumerable()));
                }
                else
                {
                    foreach (var item in recursed)
                    {
                        var l = new Line(item);
                        l.Add(Join(delim, group.Take(i)));
                        result.Add(l);
                    }
                }
            }
        }

        /// <summary>
        /// Линия из нескольких слов.
        /// </summary>
        private class Line : List<string>
        {
            public Line(IEnumerable<string> e)
                : base(e)
            {
            }
        }
    }
}
