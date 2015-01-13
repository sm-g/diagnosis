using Diagnosis.Common;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Screens;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Autocomplete
{
    /// <summary>
    /// Создает сущности из тегов, ищет предположения.
    /// </summary>
    public class Recognizer
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Recognizer));
        private readonly ISession session;

        private List<Word> created = new List<Word>(); // not static (или удалять после WordPersisted)

        /// <summary>
        ///
        /// </summary>
        public bool OnlyWords { get; set; }

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
        public bool AddQueryToSuggestions { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="session"></param>
        public Recognizer(ISession session)
        {
            Contract.Requires(session != null);

            this.session = session;
        }

        private bool CanMakeEntityFrom(string query)
        {
            if (query.IsNullOrEmpty() || OnlyWords)
                return false;
            return true;
        }

        public void SetBlank(TagViewModel tag, object suggestion, bool exactMatchRequired, bool inverse)
        {
            Contract.Requires(tag != null);

            if (suggestion == null ^ inverse) // direct no suggestion or inverse with suggestion
            {
                if (CanMakeEntityFrom(tag.Query))
                {
                    tag.Blank = tag.Query; // текст-комментарий
                    Debug.Assert(tag.BlankType == BlankType.Query);
                }
                else
                {
                    tag.Blank = null; // для поиска
                    Debug.Assert(tag.BlankType == BlankType.None);
                }
            }
            else if (!inverse) // direct with suggestion
            {
                if (!exactMatchRequired || Recognizer.Matches(suggestion, tag.Query))
                {
                    tag.Blank = suggestion; // main
                    Debug.Assert(tag.BlankType == BlankType.Word ||
                                 tag.BlankType == BlankType.Measure ||
                                 tag.BlankType == BlankType.Icd);
                }
                else
                {
                    tag.Blank = tag.Query; // запрос не совпал с предположением (CompleteOnLostFocus)
                    Debug.Assert(tag.BlankType == BlankType.Query);
                }
            }
            else // inverse, no suggestion
            {
                tag.Blank = FirstMatchingOrNewWord(tag.Query);
                Debug.Assert(tag.BlankType == BlankType.Word);
            }
        }

        /// <summary>
        /// Изменяет заготовку тега с одной сущности на другую.
        /// Возвращает успешность конвертации.
        /// </summary>
        public bool ConvertBlank(TagViewModel tag, BlankType toType)
        {
            Contract.Requires(!tag.Query.IsNullOrEmpty());
            Contract.Requires(tag.BlankType != toType);
            Contract.Requires(toType != BlankType.None && toType != BlankType.Query);
            Contract.Ensures(Contract.Result<bool>() == (tag.BlankType == toType));

            string query;
            if (tag.BlankType == BlankType.Measure)
            {
                query = (tag.Blank as Measure).Word.Title;
            }
            else
            {
                query = tag.Query;
            }

            switch (toType)
            {
                case BlankType.Comment:
                    tag.Blank = new Comment(tag.Query);
                    return true;

                case BlankType.Word: // новое или существующее
                    tag.Blank = FirstMatchingOrNewWord(query);
                    return true;

                case BlankType.Measure: // слово
                    var w = FirstMatchingOrNewWord(query);
                    var vm = new MeasureEditorViewModel(w);
                    this.Send(Event.OpenDialog, vm.AsParams(MessageKeys.Dialog));
                    if (vm.DialogResult == true)
                    {
                        tag.Blank = vm.Measure;
                        return true;
                    }
                    break;

                case BlankType.Icd: // слово/коммент в поисковый запрос
                    var vm0 = new IcdSelectorViewModel(query);
                    this.Send(Event.OpenDialog, vm0.AsParams(MessageKeys.Dialog));
                    if (vm0.DialogResult == true)
                    {
                        tag.Blank = vm0.SelectedIcd;
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Возвращает сущность из тега. Может получиться одно: слово, коммент, диагноз, измерение со словом.
        /// Кеширует сущность в теге.
        /// </summary>
        public IHrItemObject EntityOf(TagViewModel tag)
        {
            Contract.Requires(tag.BlankType != BlankType.None);

            // неизмененые теги - сущности уже созданы
            if (tag.Entity != null)
            {
                return tag.Entity;
            }

            switch (tag.BlankType)
            {
                case BlankType.Query: // нераспознаный запрос
                    var c = new Comment(tag.Blank as string);
                    tag.Entity = c;
                    return c;

                case BlankType.Word:
                    tag.Entity = tag.Blank as Word;
                    return tag.Blank as Word;

                case BlankType.Comment:
                    tag.Entity = tag.Blank as Comment;
                    return tag.Blank as Comment;

                case BlankType.Icd:
                    tag.Entity = tag.Blank as IcdDisease;
                    return tag.Blank as IcdDisease;

                case BlankType.Measure:
                    tag.Entity = tag.Blank as Measure;
                    return tag.Blank as Measure;
            }
            return null;
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
            Contract.Requires(query != null);
            Contract.Ensures(Contract.Result<List<object>>().All(o => o is Word || o is string));

            IEnumerable<IDomainObject> found;
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
                    var w = FirstMatchingOrNewWord(query);
                    results.Add(w);
                }
            }

            return results;
        }

        public void Sync(IList<IHrItemObject> hios)
        {
            hios.Sync(session, (w) => SyncTransientWord(w));
        }

        public Word SyncTransientWord(Word word)
        {
            Contract.Requires(word != null);

            // при вставке создается другой объект

            if (word.IsTransient)
            {
                // несохраненное слово
                // word1.Equals(word2) == false, but word1.CompareTo(word2) == 0
                // willSet in SetOrderedHrItems будет с первым совпадающим элементом в entitiesToBe
                var same = created.Where(e => e is Word).Where(e => (e as Word).CompareTo(word) == 0).FirstOrDefault();

                if (same != null)
                    return same;
            }
            return word;
        }

        // первое подходящее слово или новое
        private Word FirstMatchingOrNewWord(string q)
        {
            var exists = (Word)SearchForSuggesstions(q, null, q.ToEnumerable()).FirstOrDefault();
            if (exists != null && Recognizer.Matches(exists, q))
                return exists; // берем слово из словаря
            else
            {
                var word = new Word(q); // или создаем слово из запроса
                created.Add(word);
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

        private IEnumerable<Word> QueryWords(string query, object prev)
        {
            if (query.IsNullOrEmpty() && !ShowAllWordsOnEmptyQuery)
                return Enumerable.Empty<Word>();

            Word parent = prev as Word;

            var unsaved = created.Where(w => w.Title.StartsWith(query, StringComparison.InvariantCultureIgnoreCase));

            if (ShowChildrenFirst)
                return WordQuery.StartingWithChildrenFirst(session)(parent, query).Union(unsaved);
            else
                return WordQuery.StartingWith(session)(query).Union(unsaved);
        }
    }
}