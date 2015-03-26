using Diagnosis.Common;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Data.Sync
{
    public class VocLoader
    {
        private ISession session;
        private Vocabulary custom;

        public VocLoader(ISession session)
        {
            this.session = session;
            custom = VocabularyQuery.Custom(session)();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="voc"></param>
        public void LoadVoc(Vocabulary voc)
        {
            // делаем слова по текстам из словаря
            CreateWordsFromTemp(voc);

            // save created and updated - by voc cascade
            new Saver(session).Save(voc);
        }
        public void UpdateVoc(Vocabulary voc)
        {

            // убираем лишние слова из словаря
            var removed = GetRemovedWordsByTemp(voc);
            RemoveFromVoc(voc, removed);
            // делаем слова по текстам из словаря
            CreateWordsFromTemp(voc);

            // save created and updated - by voc cascade
            new Saver(session).Save(voc);
        }
        /// <summary>
        /// Убирает слова из словаря. 
        /// Возвращает слова словаря, оставшиеся в других словарях (нужно сохранить).
        /// </summary>
        /// <param name="voc"></param>
        /// <param name="toRemove"></param>
        /// <returns></returns>
        private List<Word> RemoveFromVoc(Vocabulary voc, IList<Word> toRemove)
        {
            var toDelete = new List<Word>();
            foreach (var word in toRemove)
            {
                voc.RemoveWord(word);
                if (word.Vocabularies.Count() == 0)
                    if (word.HealthRecords.Count() == 0)
                        // убрать слово, если не используется и только из этого словаря
                        toDelete.Add(word);
                    else
                        // 	использованное старое становится Пользовательскикм если у него нет других словарей
                        custom.AddWord(word);
            }
            // delete removed words
            new Saver(session).Delete(toDelete.ToArray());
            return toRemove.Except(toDelete).ToList();
        }

        /// <summary>
        /// Делает тексты словаря словами словаря. 
        /// Возвращает созданные слова.
        /// </summary>
        /// <param name="voc"></param>
        private IEnumerable<Word> CreateWordsFromTemp(Vocabulary voc)
        {
            var wordTextes = voc.WordTemplates.Select(x => x.Title);
            var created = new List<Word>();
            foreach (var text in wordTextes)
            {
                var existing = WordQuery.ByTitle(session)(text);
                if (existing == null)
                {
                    var w = new Word(text);
                    voc.AddWord(w);
                    created.Add(w);
                }
                else
                {
                    voc.AddWord(existing);
                }
            }
            return created;
        }

        /// <summary>
        /// Возвращает слова, для которых больше нет шаблонов в словаре.
        /// </summary>
        /// <param name="voc"></param>
        /// <returns></returns>
        private static List<Word> GetRemovedWordsByTemp(Vocabulary voc)
        {
            Contract.Ensures(voc.Words.ScrambledEquals(Contract.OldValue(voc.Words))); // только возвращает слова

            var wordTextes = voc.WordTemplates.Select(x => x.Title);
            var removed = new List<Word>();
            foreach (var word in voc.Words)
            {
                if (!wordTextes.Contains(word.Title))
                    removed.Add(word);
            }
            return removed;
        }

        /// <summary>
        /// Удаляет словари, сначала убирая из них все слова.
        /// </summary>
        /// <param name="vocs"></param>
        public void DeleteVocs(IEnumerable<Vocabulary> vocs)
        {
            var wordsToSave = new List<Word>();
            var vocsToDel = vocs.ToArray();
            vocsToDel.ForAll(x =>
            {
                x.ClearWordTemplates();
                var removed = GetRemovedWordsByTemp(x);
                var wordsInOtherVoc = RemoveFromVoc(x, removed);
                wordsToSave.AddRange(wordsInOtherVoc);
            });
            // убрали все слова из словаря - удаляем его на клиенте
            new Saver(session).Delete(vocsToDel);

            // сохраняем убранность словаря в словах
            new Saver(session).Save(wordsToSave.ToArray());
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="mustBeDeletedIdsPerType"></param>
        public void ProceedDeletedOnServerVocs(Dictionary<Type, IEnumerable<object>> mustBeDeletedIdsPerType)
        {
            var vocs = session.Query<Vocabulary>().ToList();

            IEnumerable<object> ids;
            mustBeDeletedIdsPerType.TryGetValue(typeof(Vocabulary), out ids);

            if (ids != null)
            {
                var vocsToDel = vocs.Where(x => ids.Contains(x.Id)).ToList();
                DeleteVocs(vocsToDel);
            }
        }
    }
}