﻿using Diagnosis.Common;
using Diagnosis.Models;
using EventAggregator;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Data.Sync
{
    public class AfterSyncChecker
    {
        private ISession session;

        public AfterSyncChecker(ISession session)
        {
            this.session = session;
        }
        /// <summary>
        /// После замены, удаленные сущности.
        /// </summary>
        public event EventHandler<ListEventArgs<IEntity>> Replaced;

        /// <summary>
        /// После загрузки проверяем справочные сущности на совпадение.
        /// </summary>
        /// <param name="addedIdsPerType"></param>
        /// <returns></returns>
        public IEnumerable<Scope> CheckReferenceEntitiesAfterDownload(Dictionary<Type, IEnumerable<object>> addedIdsPerType)
        {
            Contract.Requires(addedIdsPerType != null);
            var scopesToDeprovision = new List<Scope>();
            var types = addedIdsPerType.Keys
                .OrderByDescending(x => x, new RefModelsComparer()); // сначала родители

            // дети обновляются на нового родителя
            // старый родитель удаляется без каскадного удаления детей
            // потом удаляются дети
            foreach (var type in types)
            {
                // uow start

                IEnumerable<IEntity> replaced = Enumerable.Empty<IEntity>();
                var ids = addedIdsPerType[type];
                var entities = ids.Select(id => session.Get(type, id)).ToList();

                // проверяем справочные сущности на совпадение
                if (type == typeof(HrCategory))
                {
                    var replacing = GetReplaceEntities<HrCategory>(entities);
                    if (replacing.Count > 0)
                    {
                        UpdateChildren<HrCategory, HealthRecord>(replacing,
                            x => x.Category,
                            (x, value) => x.Category = value);

                        scopesToDeprovision.AddRange(typeof(HealthRecord).GetScopes());
                    }
                    replaced = replacing.Keys;
                }
                else if (type == typeof(Uom))
                {
                    var replacing = GetReplaceEntities<Uom>(entities);
                    if (replacing.Count > 0)
                    {
                        UpdateChildren<Uom, HrItem>(replacing,
                            x => x.Measure != null ? x.Measure.Uom : null,
                            (x, value) => { if (x.Measure != null) x.Measure.Uom = value; });
                        UpdateChildren<Uom, UomFormat>(replacing,
                            x => x.Uom,
                            (x, value) => x.Uom = value);
                        UpdateChildren<Uom, Word>(replacing,
                            x => x.Uom,
                            (x, value) => x.Uom = value);
                        scopesToDeprovision.AddRange(typeof(HrItem).GetScopes());
                        scopesToDeprovision.AddRange(typeof(UomFormat).GetScopes());
                        scopesToDeprovision.AddRange(typeof(Word).GetScopes());
                    }
                    replaced = replacing.Keys;
                }
                else if (type == typeof(UomType))
                {
                    var replacing = GetReplaceEntities<UomType>(entities);
                    if (replacing.Count > 0)
                    {
                        UpdateChildren<UomType, Uom>(replacing,
                            x => x.Type,
                            (x, value) => x.Type = value);

                        scopesToDeprovision.AddRange(typeof(Uom).GetScopes());
                    }
                    replaced = replacing.Keys;
                }
                else if (type == typeof(UomFormat))
                {
                    var replacing = GetReplaceEntities<UomFormat>(entities);
                    // no child
                    replaced = replacing.Keys;
                }
                else if (type == typeof(Speciality))
                {
                    var replacing = GetReplaceEntities<Speciality>(entities);
                    if (replacing.Count > 0)
                    {
                        UpdateChildren<Speciality, Doctor>(replacing,
                            x => x.Speciality,
                            (x, value) => x.Speciality = value);
                        UpdateChildren<Speciality, SpecialityIcdBlocks>(replacing,
                            x => x.Speciality,
                            (x, value) => x.Speciality = value);
                        UpdateChildren<Speciality, SpecialityVocabularies>(replacing,
                           x => x.Speciality,
                           (x, value) => x.Speciality = value);

                        scopesToDeprovision.AddRange(typeof(Doctor).GetScopes());
                        scopesToDeprovision.AddRange(typeof(SpecialityIcdBlocks).GetScopes());
                        scopesToDeprovision.AddRange(typeof(SpecialityVocabularies).GetScopes());
                    }
                    replaced = replacing.Keys;
                }
                else if (type == typeof(Vocabulary))
                {
                    var replacing = GetReplaceEntities<Vocabulary>(entities);
                    if (replacing.Count > 0)
                    {
                        UpdateChildren<Vocabulary, VocabularyWords>(replacing,
                            x => x.Vocabulary,
                            (x, value) => x.Vocabulary = value);
                        UpdateChildren<Vocabulary, SpecialityVocabularies>(replacing,
                           x => x.Vocabulary,
                           (x, value) => x.Vocabulary = value);

                        scopesToDeprovision.AddRange(typeof(VocabularyWords).GetScopes());
                        scopesToDeprovision.AddRange(typeof(SpecialityVocabularies).GetScopes());
                    }
                    replaced = replacing.Keys;
                }
                else if (type == typeof(SpecialityIcdBlocks))
                {
                    var replacing = GetReplaceEntities<SpecialityIcdBlocks>(entities);
                    replaced = replacing.Keys;
                    // нет ссылок на SpecialityIcdBlocks, нечего обновлять
                }
                else if (type == typeof(SpecialityVocabularies))
                {
                    var replacing = GetReplaceEntities<SpecialityVocabularies>(entities);
                    replaced = replacing.Keys;
                    // нет ссылок на SpecialityVocabularies, нечего обновлять
                }
                else
                    throw new NotImplementedException();

                CleanupReplaced(replaced);
            }
            return scopesToDeprovision.Distinct();
        }

        protected virtual void OnReplaced(ListEventArgs<IEntity> e)
        {
            var h = Replaced;
            if (h != null)
            {
                h(this, e);
            }
        }

        /// <summary>
        /// Возвращает сущности для замены с таким же значением. Cловарь со значениями { oldEntity, newEntity }
        /// </summary>
        /// <typeparam name="T">Тип сущности справочника для замены</typeparam>
        /// <param name="entities"></param>
        private Dictionary<T, T> GetReplaceEntities<T>(IList<object> entities)
            where T : IEntity
        {
            var toReplace = new Dictionary<T, T>();

            foreach (var item in entities.Cast<T>())
            {
                var existing = session.Query<T>()
                    .Where(x => x.Id != item.Id)
                    .Where(IEntityExtensions.EqualsByVal(item))
                    .FirstOrDefault();

                if (existing != null)
                    toReplace[existing] = item;
            }

            return toReplace;
        }

        /// <summary>
        /// Меняем поле в сущностях для обновления.
        /// </summary>
        /// <typeparam name="T">Тип сущности справочника для замены</typeparam>
        /// <typeparam name="TUpdate">Тип сущности, в которой меняются сущности справочника для замены</typeparam>
        /// <param name="toReplace">Сущности для замены, значения { oldEntity, newEntity }</param>
        /// <param name="propertyGetter">Геттер свойства для обновления</param>
        /// <param name="propertySetter">Сеттер свойства для обновления</param>
        private void UpdateChildren<T, TUpdate>(Dictionary<T, T> toReplace, Func<TUpdate, T> propertyGetter, Action<TUpdate, T> propertySetter)
            where T : IEntity
            where TUpdate : IEntity
        {
            var existing = session.Query<TUpdate>()
                .ToList();
            var toUpdate = existing.Where(x => propertyGetter(x) != null && toReplace.Keys.Contains(propertyGetter(x)))
                .ToList();

            toUpdate.ForEach(x => propertySetter(x, toReplace[propertyGetter(x)]));

            // сохраняем обновленные
            new Saver(session).Save(toUpdate.Cast<IEntity>().ToArray());
        }

        /// <summary>
        /// Удаляет замененные.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="replaced">Замененные сущности для удаления</param>
        private void CleanupReplaced(IEnumerable<IEntity> replaced)
        {
            var list = replaced.ToArray();

            new Saver(session).Delete(list);
            OnReplaced(new ListEventArgs<IEntity>(list));
        }
    }
}