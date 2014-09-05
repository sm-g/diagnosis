using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Diagnosis.Data.Queries;
using NHibernate;

namespace Diagnosis.ViewModels.Search.Autocomplete
{
    public class Recognizer
    {
        private readonly ISession session;
        private bool childrenFirstStrategy;

        /// <summary>
        /// Разрешает создание новых слов из текста запроса.
        /// </summary>
        public bool AllowNewFromQuery { get; set; }
        public bool AllowNonCheckable { get; set; }
        public bool CanMakeEntityFrom(string str)
        {
            return char.IsDigit(str[0])  // number
                || AllowNewFromQuery; // new word
        }

        public Recognizer(ISession session, bool childrenFirstStrategy)
        {
            this.session = session;
            this.childrenFirstStrategy = childrenFirstStrategy;
        }

        public IEnumerable<object> MakeEntities(object obj)
        {
            Contract.Ensures(Contract.Result<object>() != null);

            if (obj is string)
            {
                var str = obj as string;
                if (char.IsDigit(str[0]))
                {
                    int firstLetter = 0;
                    while (!char.IsLetter(str[firstLetter]))
                    {
                        firstLetter++;
                    }
                    var numberPart = str.Substring(0, firstLetter);
                    var numbers = numberPart.Split(new[] { '-', '\\', '/', ' ' });
                    var unit = str.Substring(firstLetter);

                    foreach (var item in numbers)
                    {
                        yield return new Word(str + unit); // number
                    }
                }
                if (AllowNewFromQuery)
                    yield return new Word(str); // new word
                Console.WriteLine("новое слово в теге, когда новые слова запрещены");
            }

            yield return obj; // word
        }
        /// <summary>
        /// По запросу определяет, какой поиск использовать. Возвращает сущности разных типов.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="prevEntity">Родительская сущность.</param>
        /// <param name="exclude">Сущности, исключаемые из результатов (например, уже выбранные в автокомплите).</param>
        /// <returns></returns>
        public List<object> Search(string query, object prevEntity, IEnumerable<object> exclude = null)
        {
            var found = Search(query, prevEntity);
            if (exclude != null)
            {
                found = found.Where(i => !exclude.Contains(i));
            }
            if (!AllowNonCheckable)
            {
                // found = found.Where(i => !(i as ICheckable).IsNonCheckable);
            }

            var result = new List<object>(found);
            bool fullMatch = result.Any(item => query == item.ToString());
            if (AllowNewFromQuery && !fullMatch)
            {
                result.Add(query);
            }
            return result;
        }

        private IEnumerable<IEntity> Search(string query, object prev)
        {
            //if (!string.IsNullOrEmpty(query) &&
            //   char.IsDigit(query[0]))
            //    return new UnitSearcher();
            Contract.Assume(prev == null || prev is Word);
            if (childrenFirstStrategy)
                return WordQuery.StartingWithChildrenFirst(session).Invoke(prev as Word, query);
            else
                return WordQuery.StartingWith(session).Invoke(query);
        }
    }
}
