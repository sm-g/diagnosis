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

namespace Diagnosis.ViewModels.Search.Autocomplete
{
    /// <summary>
    /// Создает сущности из тегов, ищет предположения.
    /// </summary>
    public class Recognizer
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Recognizer));
        private readonly ISession session;

        private List<Word> created = new List<Word>();

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

        private bool CanMakeEntityFrom(string query)
        {
            if (query.IsNullOrEmpty() || OnlyWords)
                return false;
            return true;
        }

        public void SetBlank(Tag tag, object suggestion, bool exactMatchRequired, bool inverse)
        {
            if (suggestion == null ^ inverse) // direct no suggestion or inverse with suggestion
            {
                if (CanMakeEntityFrom(tag.Query))
                {
                    tag.Blank = tag.Query; // текст-комментарий
                    Debug.Assert(tag.BlankType == Tag.BlankTypes.Query);
                }
                else
                {
                    tag.Blank = null; // для поиска
                    Debug.Assert(tag.BlankType == Tag.BlankTypes.None);
                }
            }
            else if (!inverse) // direct with suggestion
            {
                if (!exactMatchRequired || Recognizer.Matches(suggestion, tag.Query))
                {
                    tag.Blank = suggestion; // main
                    Debug.Assert(tag.BlankType == Tag.BlankTypes.Word ||
                                 tag.BlankType == Tag.BlankTypes.Measure ||
                                 tag.BlankType == Tag.BlankTypes.Icd);
                }
                else
                {
                    tag.Blank = tag.Query; // запрос не совпал с предположением (CompleteOnLostFocus)
                    Debug.Assert(tag.BlankType == Tag.BlankTypes.Query);
                }
            }
            else // inverse, no suggestion
            {
                tag.Blank = FirstMatchingOrNewWord(tag.Query);
                Debug.Assert(tag.BlankType == Tag.BlankTypes.Word);
            }
        }

        public void ConvertBlank(Tag tag, Tag.BlankTypes toType)
        {
            Contract.Requires(tag.BlankType != toType);
            Contract.Requires(toType != Tag.BlankTypes.None && toType != Tag.BlankTypes.Query);

            string query;
            if (tag.BlankType == Tag.BlankTypes.Measure)
            {
                query = (tag.Blank as Measure).Word.Title;
            }
            else
            {
                query = tag.Query; //
            }

            switch (toType)
            {
                case Tag.BlankTypes.Comment: //
                    tag.Blank = new Comment(tag.Query);
                    break;

                case Tag.BlankTypes.Word: // новое или существующее
                    tag.Blank = FirstMatchingOrNewWord(query);
                    // отдельный комментарий из числа измерения, везде?
                    break;

                case Tag.BlankTypes.Measure: // слово 
                    var w = FirstMatchingOrNewWord(query);
                    var vm = new MeasureEditorViewModel(w);
                    this.Send(Events.OpenDialog, vm.AsParams(MessageKeys.Dialog));
                    if (vm.DialogResult == true)
                    {
                        tag.Blank = vm.Measure;
                    }
                    break;

                case Tag.BlankTypes.Icd: // слово/коммент в поисковый запрос                   

                    var vm0 = new IcdSelectorViewModel(query);
                    this.Send(Events.OpenDialog, vm0.AsParams(MessageKeys.Dialog));
                    if (vm0.DialogResult == true)
                    {
                        tag.Blank = vm0.SelectedIcd;
                    }
                    break;
            }

            //if (tag.BlankType == Tag.BlankTypes.Word)
            //{
            //    // слово меняем на коммент
            //    tag.Blank = new Comment(tag.Query);
            //}
            //else // Icd !
            //{
            //    tag.Blank = FirstMatchingOrNewWord(tag.Query);
            //}
        }

        /// <summary>
        /// Возвращает сущности из тега. Может получиться одно: слово, коммент, диагноз, измерение со словом.
        /// Кеширует созданные сущности в теге.
        /// </summary>
        /// <param name="blank"></param>
        /// <returns></returns>
        public IEnumerable<IHrItemObject> EntitiesOf(Tag tag)
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
                case Tag.BlankTypes.Query: // нераспознаный запрос
                    var c = new Comment(tag.Blank as string);
                    tag.Entities = new List<IHrItemObject>() { c };
                    yield return c;
                    break;

                case Tag.BlankTypes.Word:
                    tag.Entities = new List<IHrItemObject>() { tag.Blank as Word };
                    yield return tag.Blank as Word;
                    break;

                case Tag.BlankTypes.Comment:
                    tag.Entities = new List<IHrItemObject>() { tag.Blank as Comment };
                    yield return tag.Blank as Comment;
                    break;

                case Tag.BlankTypes.Icd:
                    tag.Entities = new List<IHrItemObject>() { tag.Blank as IcdDisease };
                    yield return tag.Blank as IcdDisease;
                    break;

                case Tag.BlankTypes.Measure:
                    tag.Entities = new List<IHrItemObject>() { tag.Blank as Measure };
                    yield return tag.Blank as Measure;
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
                    results.Add(query);
                }
            }

            return results;
        }

        public TagData SyncWithSession(TagData data)
        {
            for (int i = 0; i < data.ItemObjects.Count; i++)
            {
                Word word = data.ItemObjects[i] as Word;
                if (word != null)
                {
                    // при вставке создается другой объект Word

                    if (word.IsTransient)
                    {
                        // несохраненное слово
                        // word1.Equals(word2) == false, but word1.CompareTo(word2) == 0
                        // willSet in SetOrderedHrItems будет с первым совпадающим элементом в entitiesToBe
                        var same = created.Where(e => e is Word).Where(e => (e as Word).CompareTo(word) == 0).FirstOrDefault();

                        data.ItemObjects[i] = same;
                    }
                    else
                    {
                        data.ItemObjects[i] = session.Get<Word>(word.Id);
                    }

                    if (data.ItemObjects[i] == null)
                    {
                        // новое скопировано в другом автокомплите и не сохранено?
                        // скопированно новое в поиск - static?
                        logger.WarnFormat("word not synced: {0}", word);
                    }
                }
            }
            return data;
        }

        // первое подходящее слово или новое
        private Word FirstMatchingOrNewWord(string q)
        {
            var exists = (Word)SearchForSuggesstions(q, null, null).FirstOrDefault();
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