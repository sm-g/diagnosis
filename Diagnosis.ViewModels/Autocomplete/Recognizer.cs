using Diagnosis.Common;
using Diagnosis.Common.Types;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Diagnosis.ViewModels.Autocomplete
{
    /// <summary>
    /// Создает сущности из тегов, ищет предположения.
    /// </summary>
    public class Recognizer : NotifyPropertyChangedBase
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Recognizer));

        /// <summary>
        /// несохраненные слова, созданные через автокомплит
        /// </summary>
        private static List<Word> created = new List<Word>();
        private Doctor doctor;

        private readonly ISession session;
        private bool _addQueryToSug;

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
                    OnPropertyChanged(() => AddQueryToSuggestions);
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

        public bool MeasureEditorWithCompare { get; set; }

        static Recognizer()
        {
            typeof(Recognizer).Subscribe(Event.WordPersisted, (e) =>
            {
                // now word can be retrieved from db
                var word = e.GetValue<Word>(MessageKeys.Word);
                created.Remove(word);
            });
            AuthorityController.LoggedOut += (s, e) =>
            {
                created.Clear();
            };
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="session"></param>
        public Recognizer(ISession session, bool clearCreated = false)
        {
            Contract.Requires(session != null);

            this.session = session;

            if (clearCreated)
                created.Clear();

            AddNotPersistedToSuggestions = true;
            CanChangeAddQueryToSuggstions = true;
            doctor = AuthorityController.CurrentDoctor;

            AuthorityController.LoggedIn += (s, e) =>
            {
                // fix, search надо создавать после логина
                doctor = AuthorityController.CurrentDoctor;
            };
        }

        private bool CanMakeEntityFrom(string query)
        {
            if (query.IsNullOrEmpty())
                return false;
            return true;
        }

        public void SetBlank(TagViewModel tag, IHrItemObject suggestion, bool exactMatchRequired, bool inverse)
        {
            Contract.Requires(tag != null);

            if (suggestion == null ^ inverse) // direct no suggestion or inverse with suggestion
            {
                if (CanMakeEntityFrom(tag.Query))
                {
                    tag.Blank = new Comment(tag.Query); // текст-комментарий
                }
                else
                {
                    tag.Blank = null; // для поиска или ентер в пустом непоследнем
                    Debug.Assert(tag.BlankType == BlankType.None);
                }
            }
            else if (!inverse) // direct with suggestion
            {
                if (!exactMatchRequired || Recognizer.Matches(suggestion, tag.Query))
                {
                    tag.Blank = suggestion; // main
                    Debug.Assert(tag.BlankType != BlankType.None);
                }
                else
                {
                    tag.Blank = new Comment(tag.Query); // запрос не совпал с предположением (CompleteOnLostFocus)
                }
            }
            else // inverse, no suggestion
            {
                Contract.Assume(!tag.Query.IsNullOrEmpty());
                tag.Blank = FirstMatchingOrNewWord(tag.Query);
                Debug.Assert(tag.BlankType == BlankType.Word);
            }
        }

        

        /// <summary>
        /// Возвращает сущность из тега.
        /// </summary>
        public IHrItemObject EntityOf(TagViewModel tag)
        {
            Contract.Requires(tag.BlankType != BlankType.None);
            return tag.Blank;
        }

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

            // слова, доступные для ввода
            var wordsForDoctor = QueryWords(query, prevEntityBlank)
                    .Where(x => created.Contains(x) || doctor.Words.Contains(x));

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
        /// Запоминает новое слово для списка предположений.
        /// </summary>
        /// <param name="tag"></param>
        public void AfterCompleteTag(TagViewModel tag)
        {
            if (tag.BlankType == BlankType.Word)
            {
                var w = tag.Blank as Word;
                if (w.IsTransient)
                {
                    created.Add(w);
                }
            }
        }

        public void SyncAfterPaste(IList<ConfindenceHrItemObject> hios)
        {
            hios.SyncAfterPaste(session);
        }

        public static Word GetWordFromCreated(Word word)
        {
            Contract.Requires(word != null);

            // при вставке создается другой объект

            if (word.IsTransient)
            {
                // несохраненное слово
                // word1.Equals(word2) == false, but word1.CompareTo(word2) == 0
                // willSet in SetOrderedHrItems будет с первым совпадающим элементом в entitiesToBe
                var same = created.Where(e => e.CompareTo(word) == 0).FirstOrDefault();

                if (same != null)
                    return same;
            }
            return word;
        }

        /// <summary>
        /// Первое точно подходящее слово из БД или новое.
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        internal Word FirstMatchingOrNewWord(string q)
        {
            var existing = QueryWords(q, null).FirstOrDefault();
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
        private IEnumerable<Word> QueryWords(string query, object prev)
        {
            if (query.IsNullOrEmpty() && !ShowAllWordsOnEmptyQuery)
                return Enumerable.Empty<Word>();

            Word parent = prev as Word;

            var unsaved = AddNotPersistedToSuggestions
                ? created.Where(w => w.Title.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                : Enumerable.Empty<Word>();

            var fromDB = ShowChildrenFirst ?
                WordQuery.StartingWithChildrenFirst(session)(parent, query) :
                WordQuery.StartingWith(session)(query);

            return fromDB.Union(unsaved);
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            // все несохраннные слова - не в словаре
            Contract.Invariant(created.All(x => x.Vocabularies.Count() == 0));
        }
    }
}