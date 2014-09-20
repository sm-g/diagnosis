using Diagnosis.Core;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
        /// Разрешает создание новых слов из текста запроса.
        /// </summary>
        public bool AllowNewFromQuery { get; set; }

        public bool AllowNonCheckable { get; set; }

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
                || AllowNewFromQuery; // new word
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
        /// Создает сущности из заготовки. Из одной заготовки может получиться несколько измерений.
        /// </summary>
        /// <param name="blank"></param>
        /// <returns></returns>
        public IEnumerable<IDomainEntity> MakeEntities(object blank)
        {
            var blankType = Tag.GetBlankType(blank);
            switch (blankType)
            {
                case Tag.BlankTypes.None:
                    break;

                case Tag.BlankTypes.Query:
                    if (IsMeasure(blank as string))
                    {
                        var splittedQuery = SplitMeasureQuery(blank as string);
                        if (splittedQuery.Item2 != "")
                            System.Diagnostics.Debug.Print("! недописанный uom, создаем измерение без него");

                        foreach (var val in GetFloats(splittedQuery.Item1))
                        {
                            yield return new Measure(val);
                        }
                    }
                    else
                    {
                        if (AllowNewFromQuery)
                            yield return new Word(blank as string);
                        else
                            System.Diagnostics.Debug.Print("новое слово в теге, когда новые слова запрещены");
                    }
                    break;

                case Tag.BlankTypes.Word:
                    yield return blank as Word;
                    break;

                case Tag.BlankTypes.Measure:
                    var numbersWithUom = (blank as NumbersWithUom);
                    foreach (var val in GetFloats(numbersWithUom.Item1))
                    {
                        yield return new Measure(val, numbersWithUom.Item2);
                    }
                    break;
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
                if (splitted.Item2.IsNullOrEmpty())
                {
                    results = new List<object>(); // не показываем все возможные единицы
                }
                else
                {
                    found = UomQuery.StartingWith(session)(splitted.Item2);
                    // добавляем числовую часть к каждой единице
                    var numbers = splitted.Item1;
                    results = new List<object>(found.Select(uom => new NumbersWithUom(numbers, uom as Uom)));
                    // измерения могут повторяться
                }
            }
            else
            {
                found = QueryWords(query, prevEntityBlank);
                if (exclude != null)
                {
                    found = found.Where(i => !exclude.Contains(i));
                }
                if (!AllowNonCheckable)
                {
                    // found = found.Where(i => !(i as ICheckable).IsNonCheckable);
                }
                results = new List<object>(found);

                if (AllowNewFromQuery)
                {
                    bool existsSame = results.Any(item => query == item.ToString());
                    if (exclude != null)
                    {
                        existsSame |= exclude.Any(item => item != null ? query == item.ToString() : false);
                    }

                    // не добавляем запрос, совпадающий со словом в результатах или словом/запросом в исключенных предположениях
                    if (!existsSame)
                    {
                        results.Add(query);
                    }
                }
            }

            return results;
        }

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
                if (float.TryParse(item, out val))
                    yield return val;
            }
        }

        private IEnumerable<Word> QueryWords(string query, object prev)
        {
            Word parent = prev as Word;

            if (childrenFirstStrategy)
                return WordQuery.StartingWithChildrenFirst(session)(parent, query);
            else
                return WordQuery.StartingWith(session)(query);
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