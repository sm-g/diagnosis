﻿using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
{
    /// <summary>
    /// Хранит созданные в автокомплитах еще не сохраненные в БД слова.
    /// </summary>
    internal static class CreatedWordsManager
    {
        private static HashSet<Word> created = new HashSet<Word>(Compare.By<Word, string>(x => x.Title, StringComparer.OrdinalIgnoreCase));

        static CreatedWordsManager()
        {
            typeof(CreatedWordsManager).Subscribe(Event.EntityPersisted, (e) =>
            {
                // now word can be retrieved from db
                var word = e.GetValue<IEntity>(MessageKeys.Entity) as Word;
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
        /// <param name="w"></param>
        public static void AfterCompleteTagWith(Word w)
        {
            if (w.IsTransient)
            {
                created.Add(w);
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