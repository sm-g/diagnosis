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

        public VocLoader(ISession session)
        {
            this.session = session;
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
            Contract.Requires(vocs != null);

            var wordsToSave = new List<Word>();
            foreach (var voc in vocs.Where(x => !x.IsCustom))
            {
                var templates = voc.WordTemplates;

                // убираем лишние слова из словаря
                var removed = GetRemovedWordsByTemp(voc, templates);
                var wordsInOtherVoc = RemoveFromVoc(voc, removed);
                wordsToSave.AddRange(wordsInOtherVoc);
                // делаем слова по текстам из словаря
                var created = CreateWordsFromTemp(voc, templates);

                // сохраняем слова, добавленные в словарь
                new Saver(session).Save(voc);
            }
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
        /// Удаляет словари, сначала убирая все слова и связи со специальностью.
        /// </summary>
        /// <param name="vocs"></param>
        public void DeleteVocs(IEnumerable<Vocabulary> vocs)
        {
            Contract.Requires(Constants.IsClient);
            Contract.Requires(vocs != null);

            var wordsToSave = new List<Word>();
            var vocsToDel = vocs.Where(x => !x.IsCustom).ToArray();
            vocsToDel.ForAll(x =>
            {
                // убрали словарь из всех специальностей
                x.Specialities.ToList().ForEach(s =>
                    s.RemoveVoc(x));

                // убрали все слова из словаря
                var wordsInOtherVoc = RemoveFromVoc(x, x.Words.ToList());
                wordsToSave.AddRange(wordsInOtherVoc);

            });
            // удаляем его, vocword и vocspec на клиенте
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
            Contract.Requires(mustBeDeletedIdsPerType != null);

            IEnumerable<object> ids;
            if (mustBeDeletedIdsPerType.TryGetValue(typeof(Vocabulary), out ids))
            {
                var vocsToDel = VocabularyQuery.ByIds(session)(ids.Cast<Guid>());
                DeleteVocs(vocsToDel);
            }
            var vocs = EntityQuery<Vocabulary>.All(session)();
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
            Contract.Ensures(Contract.Result<IEnumerable<Word>>().All(x => x.Vocabularies.Any()));

            var toDelete = new List<Word>();
            foreach (var word in toRemove)
            {
                voc.RemoveWord(word);
                if (!word.HealthRecords.Any())
                {
                    // убрать слово, если не используется и не в словарях
                    if (!word.Vocabularies.Any()) // ?или только в пользовательских словарях
                    {
                        word.OnDelete();
                        toDelete.Add(word);
                    }
                }
                else
                {
                    var docs = word.HealthRecords.Select(x => x.Doctor).Distinct();
                    foreach (var doc in docs)
                    {
                        // использованное становится Пользовательским
                        // для всех врачей, которые его использовали
                        // если его нет в других словарях, доступных врачу

                        // должно стать пользовательским для тех врачей, у которых нет словаря с этим словом

                        // при удалении словаря слово все еще доступно врачу через кеш, но его уже нет в словаре
                        if (!doc.Vocabularies.Except(voc).SelectMany(x => x.Words).Contains(word))
                        {
                            doc.CustomVocabulary.AddWord(word);
                        }
                    }
                }
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
                var existing = WordQuery.ByTitle(session)(text); // для любого врача, в любом регистре
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