using Diagnosis.Common;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Autocomplete
{
    /// <summary>
    /// Ищет предположения, создает слова.
    /// </summary>
    public sealed class SuggestionsMaker
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(SuggestionsMaker));

        private readonly ISession session;
        private Doctor doctor;
        private bool _addQueryToSug;

        public SuggestionsMaker(ISession session, Doctor doctor)
        {
            Contract.Requires(session != null);
            Contract.Requires(doctor != null);
            this.session = session;
            this.doctor = doctor;

            AddNotPersistedToSuggestions = true;
            CanChangeAddQueryToSuggstions = true;
        }

        /// <summary>
        /// При поиске предположений-слов первыми - дети предыдущего слова.
        /// </summary>
        public bool ShowChildrenFirst { get; set; }

        /// <summary>
        /// Показывать все предположения-слова при пустом запросе. Если false, требуется первый символ.
        /// </summary>
        public bool ShowAllWordsOnEmptyQuery { get; set; }

        /// <summary>
        /// Добавлять запрос как новое слово в список предположений, если нет соответствующего слова.
        /// </summary>
        public bool AddQueryToSuggestions
        {
            get { return _addQueryToSug; }
            set
            {
                if (CanChangeAddQueryToSuggstions && _addQueryToSug != value)
                {
                    _addQueryToSug = value;
                }
            }
        }

        /// <summary>
        /// Можно менять режим добавления запроса в предположения. Default is true.
        /// </summary>
        public bool CanChangeAddQueryToSuggstions { get; set; }

        /// <summary>
        /// Добавлять созданное несохраненное слово в список предположений. Default is true.
        /// </summary>
        public bool AddNotPersistedToSuggestions { get; set; }

        internal ISession Session { get { return session; } }

        /// <summary>
        /// Возвращает список предполжений для запроса. По запросу определяет, какой поиск использовать.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="prevEntityBlank">Предыдущая заготовка.</param>
        /// <param name="exclude">Предположения, исключаемые из результатов (например, уже выбранные в автокомплите заготовки).</param>
        /// <returns>Предположения - слова.</returns>
        public List<Word> SearchForSuggesstions(string query, object prevEntityBlank, IEnumerable<object> exclude = null)
        {
            Contract.Requires(query != null);
            Contract.Ensures(Contract.Result<List<Word>>().IsUnique(x => x.Title, StringComparer.OrdinalIgnoreCase));

            // слова, доступные для ввода
            var wordsForDoctor = QueryWords(query, prevEntityBlank, AddNotPersistedToSuggestions)
                    .Where(x => CreatedWordsManager.Created.Contains(x) || doctor.Words.Contains(x));

            // кроме исключенных
            if (exclude != null)
                wordsForDoctor = wordsForDoctor.Where(i => !exclude.Contains(i));

            // по алфавиту
            var results = new List<Word>(wordsForDoctor.OrderBy(x => x.Title));

            if (AddQueryToSuggestions)
            {
                bool existsSame = results.Any(item => Matches(item, query));
                if (exclude != null)
                    existsSame |= exclude.Any(item => item != null ? Matches(item, query) : false);

                // не добавляем запрос, совпадающий со словом в результатах или словом/запросом в исключенных предположениях
                if (!existsSame && !query.IsNullOrEmpty())
                {
                    var w = FirstMatchingOrNewWord(query);
                    results.Insert(0, w); // добавленное слово должно быть первым
                }
            }

            return results;
        }

        /// <summary>
        /// Первое точно подходящее слово из БД или новое.
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public Word FirstMatchingOrNewWord(string q)
        {
            Contract.Requires(q != null);
            Contract.Ensures(Contract.Result<Word>().Title.Equals(q, StringComparison.OrdinalIgnoreCase));

            var existing = QueryWords(q, null, true).FirstOrDefault();
            if (existing != null && Matches(existing, q))
                return existing; // берем слово из словаря
            else
            {
                var word = new Word(q); // или создаем слово из запроса. добавляется в словарь при сохранении записи
                return word;
            }
        }

        /// <summary>
        /// Определяет сходство предположения и запроса.
        /// </summary>
        private static bool Matches(object suggestion, string query)
        {
            return query.MatchesAsStrings(suggestion);
        }

        /// <summary>
        /// Все слова с началом как у запроса с учетом предыдущего.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="prev"></param>
        /// <returns></returns>
        private IEnumerable<Word> QueryWords(string query, object prev, bool withNotPersisted)
        {
            if (query.IsNullOrEmpty() && !ShowAllWordsOnEmptyQuery)
                return Enumerable.Empty<Word>();

            Word parent = prev as Word;

            var unsaved = withNotPersisted
                ? CreatedWordsManager.Created.Where(w => w.Title.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                : Enumerable.Empty<Word>();

            var fromDB = ShowChildrenFirst ?
                WordQuery.StartingWithChildrenFirst(session)(parent, query) :
                WordQuery.StartingWith(session)(query);

            return fromDB.Union(unsaved);
        }
    }
}