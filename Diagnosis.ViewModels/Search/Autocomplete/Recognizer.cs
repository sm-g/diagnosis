﻿using Diagnosis.Core;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;

namespace Diagnosis.ViewModels.Search.Autocomplete
{
    public class Recognizer
    {
        private readonly ISession session;
        private bool childrenFirstStrategy;
        private static char[] measureStart = new char[] { '-', '.', ',' };
        private static char[] numbersSeparators = new char[] { '\\', '/', ' ' };

        /// <summary>
        /// Создание новых сущностей (слов) из текста запроса.
        /// </summary>
        public bool AutoNewFromQuery { get; set; }

        /// <summary>
        /// Показывать все предположения-слова при пустом запросе. Если false, требуется первый символ.
        /// </summary>
        public bool ShowAllWordsOnEmptyQuery { get; set; }

        /// <summary>
        /// Показывать все предположения-единицы измерения при пустом запросе. Если false, требуется первый символ.
        /// Нельзя завершить измерение без единицы энтером.
        /// </summary>
        public bool ShowAllUomsOnEmptyQuery { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="session"></param>
        /// <param name="childrenFirstStrategy">При поиске предположений-слов первыми - дети предыдущего слова.</param>
        public Recognizer(ISession session, bool childrenFirstStrategy)
        {
            this.session = session;
            this.childrenFirstStrategy = childrenFirstStrategy;
        }

        public bool CanMakeEntityFrom(string query)
        {
            if (query.IsNullOrEmpty())
                return false;
            return IsMeasure(query)
                || AutoNewFromQuery; // new word
        }

        /// <summary>
        /// Запрос-измерение, начинается на число.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static bool IsMeasure(string query)
        {
            return !query.IsNullOrEmpty() &&
                (char.IsDigit(query[0]) ||
                 query.Length > 1 && char.IsDigit(query[1]) && measureStart.Contains(query[0]));
        }

        /// <summary>
        /// Создает сущности из тега. Может получиться одно слово или несколько измерений.
        /// Кеширует созданные сущности в теге.
        /// </summary>
        /// <param name="blank"></param>
        /// <returns></returns>
        public IEnumerable<IHrItemObject> MakeEntities(Tag tag)
        {
            Contract.Requires(tag.BlankType != Tag.BlankTypes.None);

            // неизмененые теги - сущности уже созданы
            if (tag.Entities != null)
            {
                foreach (var e in tag.Entities)
                {
                    yield return e;
                }
                yield break;
            }

            switch (tag.BlankType)
            {
                case Tag.BlankTypes.Query:
                    if (IsMeasure(tag.Blank as string))
                    {
                        var splittedQuery = SplitMeasureQuery(tag.Blank as string);
                        foreach (var m in MakeMeasures(tag, splittedQuery.Item1, null)) // if splittedQuery.Item2 != "" uom is wrong, ignore such uom
                        {
                            yield return m;
                        }
                    }
                    else if (AutoNewFromQuery)
                    {
                        var w = new Word(tag.Blank as string);
                        tag.Entities = new List<IHrItemObject>() { w };
                        yield return w;
                    }
                    break;

                case Tag.BlankTypes.Word:
                    tag.Entities = new List<IHrItemObject>() { tag.Blank as Word };
                    yield return tag.Blank as Word;
                    break;

                case Tag.BlankTypes.Measure:
                    var numbersWithUom = tag.Blank as NumbersWithUom;
                    foreach (var m in MakeMeasures(tag, numbersWithUom.Item1, numbersWithUom.Item2))
                    {
                        yield return m;
                    }
                    break;
            }
        }

        /// <summary>
        /// Создает измерения, кешируя их в теге.
        /// </summary>
        private IEnumerable<Measure> MakeMeasures(Tag tag, string numbers, Uom uom)
        {
            var measures = new List<Measure>();

            foreach (var val in GetFloats(numbers))
            {
                measures.Add(new Measure(val, uom));
            }
            tag.Entities = new List<IHrItemObject>(measures);
            foreach (var m in measures)
            {
                yield return m;
            }
        }

        /// <summary>
        /// Возвращает список предполжений для запроса. По запросу определяет, какой поиск использовать.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="prevEntityBlank">Предыдущая заготовка.</param>
        /// <param name="exclude">Предположения, исключаемые из результатов (например, уже выбранные в автокомплите заготовки).</param>
        /// <returns>Предположения - слово, число и единица или строка запроса.</returns>
        public List<object> SearchForSuggesstions(string query, object prevEntityBlank, IEnumerable<object> exclude = null)
        {
            Contract.Ensures(Contract.Result<List<object>>().All(o => o is Word || o is NumbersWithUom || o is string));
            IEnumerable<IDomainEntity> found;
            List<object> results;

            if (IsMeasure(query))
            {
                var splitted = SplitMeasureQuery(query);

                found = QueryUoms(splitted.Item2);
                // добавляем числовую часть к каждой единице
                var numbers = splitted.Item1;
                results = new List<object>(found.Select(uom => new NumbersWithUom(numbers, uom as Uom)));
                // измерения могут повторяться
            }
            else
            {
                found = QueryWords(query, prevEntityBlank);
                if (exclude != null)
                {
                    found = found.Where(i => !exclude.Contains(i));
                }

                results = new List<object>(found);

                if (AutoNewFromQuery)
                {
                    bool existsSame = results.Any(item => query == item.ToString());
                    if (exclude != null)
                    {
                        existsSame |= exclude.Any(item => item != null ? query == item.ToString() : false);
                    }

                    // не добавляем запрос, совпадающий со словом в результатах или словом/запросом в исключенных предположениях
                    if (!existsSame && !query.IsNullOrEmpty())
                    {
                        results.Add(query);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Определяет сходство предположения и запроса.
        /// </summary>
        public static bool Matches(object suggestion, string query)
        {
            if (suggestion is Word)
                return (suggestion as Word).Title == query;
            if (suggestion is NumbersWithUom)
                // проверяем только совпадение uom, число копируется из запроса, не запрашивается из БД
                return (suggestion as NumbersWithUom).Item2.Abbr == SplitMeasureQuery(query).Item2;
            return suggestion.ToString() == query;
        }

        /// <summary>
        /// Разделяет запрос на части с числами и единицей измерения.
        /// Если запрос не подходит, возвращает пустые строки.
        /// </summary>
        public static Tuple<string, string> SplitMeasureQuery(string query)
        {
            if (!IsMeasure(query))
                return new Tuple<string, string>("", "");

            // разбиваем строку [.|,|-]#[{/|\| }#][ ][uom]
            int uomStart = 0;
            while (uomStart < query.Length && !char.IsLetter(query[uomStart]))
            {
                uomStart++;
            }
            var numberPart = query.Substring(0, uomStart).Trim();
            var uomPart = query.Substring(uomStart);
            return new Tuple<string, string>(numberPart, uomPart);
        }

        /// <summary>
        /// Возвращает числа из строки разделенных чисел.
        /// </summary>
        private IEnumerable<float> GetFloats(string str)
        {
            var numbers = str.Split(numbersSeparators, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in numbers)
            {
                float val;
                if (float.TryParse(item.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out val)) // both dot and comma
                    yield return val;
            }
        }

        private IEnumerable<Word> QueryWords(string query, object prev)
        {
            if (query.IsNullOrEmpty() && !ShowAllWordsOnEmptyQuery)
                return Enumerable.Empty<Word>();

            Word parent = prev as Word;

            if (childrenFirstStrategy)
                return WordQuery.StartingWithChildrenFirst(session)(parent, query);
            else
                return WordQuery.StartingWith(session)(query);
        }

        private IEnumerable<Uom> QueryUoms(string query)
        {
            if (query.IsNullOrEmpty() && !ShowAllUomsOnEmptyQuery)
                return Enumerable.Empty<Uom>();

            return UomQuery.StartingWith(session)(query);
        }

        /// <summary>
        /// Числа с сущностью-единицей измерения. В предположениях.
        /// </summary>
        internal class NumbersWithUom : Tuple<string, Uom>
        {
            public NumbersWithUom(string numbers, Uom uom)
                : base(numbers, uom)
            {
            }

            public override string ToString()
            {
                return string.Format("{0} {1}", Item1, Item2.Abbr); // descr?
            }

            public override bool Equals(object other)
            {
                NumbersWithUom nwu = other as NumbersWithUom;
                if (nwu == null)
                {
                    return false;
                }

                if (Item2 == nwu.Item2 && Item1 == nwu.Item1)
                {
                    // не проверяем отдельно каждое число
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                return Item1.GetHashCode() ^ Item2.GetHashCode();
            }
        }
    }
}