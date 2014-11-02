using Diagnosis.Core;
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

        /// <summary>
        /// Создание новых сущностей (слов) из текста запроса. По умолчанию создается коммент.
        /// </summary>
        public bool AutoNewFromQuery { get; set; }

        /// <summary>
        /// При поиске предположений-слов первыми - дети предыдущего слова.
        /// </summary>
        public bool ShowChildrenFirst { get; set; }

        /// <summary>
        /// Показывать все предположения-слова при пустом запросе. Если false, требуется первый символ.
        /// </summary>
        public bool ShowAllWordsOnEmptyQuery { get; set; }

        /// <summary>
        /// Добавлять запрос в список предположений, если нет соответствующего слова.
        /// </summary>
        public bool AddQueryToSuggestions { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="session"></param>
        public Recognizer(ISession session)
        {
            this.session = session;
        }

        /// <summary>
        /// Создает сущности из тега. Может получиться одно слово или один коммент.
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
                    if (AutoNewFromQuery)
                    {
                        var w = new Word(tag.Blank as string);
                        tag.Entities = new List<IHrItemObject>() { w };
                        yield return w;
                    }
                    else
                    {
                        var c = new Comment(tag.Blank as string);
                        tag.Entities = new List<IHrItemObject>() { c };
                        yield return c;
                    }
                    break;

                case Tag.BlankTypes.Word:
                    tag.Entities = new List<IHrItemObject>() { tag.Blank as Word };
                    yield return tag.Blank as Word;
                    break;

                case Tag.BlankTypes.Comment:
                    tag.Entities = new List<IHrItemObject>() { tag.Blank as Comment };
                    yield return tag.Blank as Comment;
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
            Contract.Ensures(Contract.Result<List<object>>().All(o => o is Word || o is string));
            IEnumerable<IDomainEntity> found;
            List<object> results;

            found = QueryWords(query, prevEntityBlank);
            if (exclude != null)
            {
                found = found.Where(i => !exclude.Contains(i));
            }

            results = new List<object>(found);

            if (AddQueryToSuggestions)
            {
                bool existsSame = results.Any(item => query.MatchesAsStrings(item));
                if (exclude != null)
                {
                    existsSame |= exclude.Any(item => item != null ? query.MatchesAsStrings(item) : false);
                }

                // не добавляем запрос, совпадающий со словом в результатах или словом/запросом в исключенных предположениях
                if (!existsSame && !query.IsNullOrEmpty())
                {
                    results.Add(query);
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
                return query.MatchesAsStrings(suggestion as Word);
            return query.MatchesAsStrings(suggestion);
        }

        private IEnumerable<Word> QueryWords(string query, object prev)
        {
            if (query.IsNullOrEmpty() && !ShowAllWordsOnEmptyQuery)
                return Enumerable.Empty<Word>();

            Word parent = prev as Word;

            if (ShowChildrenFirst)
                return WordQuery.StartingWithChildrenFirst(session)(parent, query);
            else
                return WordQuery.StartingWith(session)(query);
        }
    }
}