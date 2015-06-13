using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Autocomplete
{
    /// <summary>
    /// Хранит созданные в автокомплитах еще не сохраненные в БД слова.
    /// </summary>
    internal static class CreatedWordsManager
    {
        private static HashSet<Word> created = new HashSet<Word>(Compare.By<Word, string>(x => x.Title, StringComparer.OrdinalIgnoreCase));

        static CreatedWordsManager()
        {
            typeof(SuggestionsMaker).Subscribe(Event.WordPersisted, (e) =>
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
        /// Несохраненные слова, созданные через автокомплит
        /// </summary>
        public static IEnumerable<Word> Created { get { return created; } }

        /// <summary>
        /// Запоминает новое слово для списка предположений.
        /// </summary>
        /// <param name="tag"></param>
        public static void AfterCompleteTag(TagViewModel tag)
        {
            if (tag.Blank is Word)
            {
                var w = tag.Blank as Word;
                if (w.IsTransient)
                {
                    created.Add(w);
                }
            }
        }

        public static Word GetSameWordFromCreated(Word word)
        {
            Contract.Requires(word != null);
            Contract.Requires(word.IsTransient);
            Contract.Ensures(Contract.Result<Word>() == null || Contract.Result<Word>().CompareTo(word) == 0);

            // несохраненное слово
            var same = created.Where(e => e.CompareTo(word) == 0).FirstOrDefault();

            return same; // null if transient but not in created
        }

        internal static void ClearCreated()
        {
            created.Clear();
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private static void ObjectInvariant()
        {
            // все несохраннные слова - не в словаре
            Contract.Invariant(created.All(x => x.Vocabularies.Count() == 0));
            Contract.Invariant(created.IsUnique(x => x.Title, StringComparer.OrdinalIgnoreCase));
        }
    }
}