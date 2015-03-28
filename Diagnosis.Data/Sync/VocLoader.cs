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
        /// Создает слова по шаблонам словаря, убирает слова, для которых нет шаблона.
        /// </summary>
        /// <param name="voc"></param>
        public void LoadOrUpdateVocs(Vocabulary voc)
        {
            LoadOrUpdateVocs(voc.ToEnumerable());
        }

        /// <summary>
        /// Создает слова по шаблонам словаря, убирает слова, для которых нет шаблона.
        /// </summary>
        /// <param name="voc"></param>
        public void LoadOrUpdateVocs(IEnumerable<Vocabulary> vocs)
        {
            var wordsToSave = new List<Word>();
            foreach (var voc in vocs)
            {
                // убираем лишние слова из словаря
                var templates = voc.WordTemplates;
                var removed = GetRemovedWordsByTemp(voc, templates);
                var wordsInOtherVoc = RemoveFromVoc(voc, removed);
                wordsToSave.AddRange(wordsInOtherVoc);
                // делаем слова по текстам из словаря
                CreateWordsFromTemp(voc, templates);
            }
            // save words - by voc cascade
            new Saver(session).Save(vocs.ToArray());
            // сохраняем убранность словаря в оставшихся словах
            new Saver(session).Save(wordsToSave.ToArray());
        }
        /// <summary>
        /// Удаляет словарь, сначала убирая все слова.
        /// </summary>
        /// <param name="vocabulary"></param>
        public void DeleteVocs(Vocabulary vocabulary)
        {
            DeleteVocs(vocabulary.ToEnumerable());
        }

        /// <summary>
        /// Удаляет словари, сначала убирая все слова.
        /// </summary>
        /// <param name="vocs"></param>
        public void DeleteVocs(IEnumerable<Vocabulary> vocs)
        {
            var wordsToSave = new List<Word>();
            var vocsToDel = vocs.ToArray();
            vocsToDel.ForAll(x =>
            {
                var wordsInOtherVoc = RemoveFromVoc(x, x.Words.ToList());
                wordsToSave.AddRange(wordsInOtherVoc);
            });
            // убрали все слова из словаря - удаляем его на клиенте
            new Saver(session).Delete(vocsToDel);

            // сохраняем убранность словаря в оставшихся словах
            new Saver(session).Save(wordsToSave.ToArray());
        }

        /// <summary>
        /// Удаляем убранные, обновляем оставшиеся загруженные словари
        /// </summary>
        /// <param name="mustBeDeletedIdsPerType"></param>
        public void AfterSyncVocs(Dictionary<Type, IEnumerable<object>> mustBeDeletedIdsPerType)
        {
            var vocs = session.Query<Vocabulary>().ToList();

            IEnumerable<object> ids;
            mustBeDeletedIdsPerType.TryGetValue(typeof(Vocabulary), out ids);

            if (ids != null)
            {
                var vocsToDel = vocs.Where(x => ids.Contains(x.Id)).ToList();
                DeleteVocs(vocsToDel);
            }
            vocs = session.Query<Vocabulary>().ToList();
            LoadOrUpdateVocs(vocs);
        }

        /// <summary>
        /// Возвращает слова, для которых больше нет шаблонов в словаре.
        /// </summary>
        /// <param name="voc"></param>
        /// <returns></returns>
        private static IList<Word> GetRemovedWordsByTemp(Vocabulary voc, IEnumerable<WordTemplate> templates)
        {
            Contract.Ensures(voc.Words.ScrambledEquals(Contract.OldValue(voc.Words))); // только возвращает слова

            var strings = templates.Select(X => X.Title).ToList();
            var removed = new List<Word>();
            foreach (var word in voc.Words)
            {
                if (!strings.Contains(word.Title))
                    removed.Add(word);
            }
            return removed;
        }

        /// <summary>
        /// Убирает слова из словаря.
        /// Возвращает слова словаря, оставшиеся в других словарях (нужно сохранить).
        /// </summary>
        /// <param name="voc"></param>
        /// <param name="toRemove"></param>
        /// <returns></returns>
        private IEnumerable<Word> RemoveFromVoc(Vocabulary voc, IList<Word> toRemove)
        {
            Contract.Ensures(voc.Words.Intersect(toRemove).Count() == 0);
            Contract.Ensures(Contract.Result<IEnumerable<Word>>().All(x => x.Vocabularies.Count() > 0));

            var toDelete = new List<Word>();
            foreach (var word in toRemove)
            {
                voc.RemoveWord(word);
                if (word.Vocabularies.Count() == 0)
                    if (word.HealthRecords.Count() == 0)
                        // убрать слово, если не используется и только из этого словаря
                        toDelete.Add(word);
                    else
                        // использованное старое становится Пользовательскикм если у него нет других словарей
                        custom.AddWord(word);
                // есть еще словари у слова - оставляем в них
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
        private IEnumerable<Word> CreateWordsFromTemp(Vocabulary voc, IEnumerable<WordTemplate> templates)
        {
            // для каждого шаблона есть слово
            Contract.Ensures(templates.Select(x => x.Title.ToLower())
                     .Except(voc.Words.Select(x => x.Title.ToLower())).Count() == 0);

            var created = new List<Word>();
            foreach (var text in templates.Select(x => x.Title))
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
    }
}