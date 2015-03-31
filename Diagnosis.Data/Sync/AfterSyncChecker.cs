using Diagnosis.Common;
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
            var scopesToDeprovision = new HashSet<Scope>();
            var types = addedIdsPerType.Keys
                .OrderByDescending(x => x, new RefModelsComparer()); // сначала родители

            // дети обновляются на нового родителя
            // старый родитель удаляется без каскадного удаления детей
            // потом удаляются дети
            foreach (var type in types)
            {
                // uow start

                IEnumerable<IEntity> replaced = null;
                var ids = addedIdsPerType[type];
                var entities = ids.Select(id => session.Get(type, id)).ToList();

                // проверяем справочные сущности на совпадение
                if (type == typeof(HrCategory))
                {
                    var replacing = GetReplaceEntities<HrCategory>(entities);
                    if (replacing.Count > 0)
                    {
                        UpdateParents<HrCategory, HealthRecord>(replacing,
                            x => x.Category,
                            (x, value) => x.Category = value);

                        scopesToDeprovision.Add(typeof(HealthRecord).GetScope());
                    }
                    replaced = replacing.Keys;
                }
                else if (type == typeof(Uom))
                {
                    var replacing = GetReplaceEntities<Uom>(entities);
                    if (replacing.Count > 0)
                    {
                        UpdateParents<Uom, HrItem>(replacing,
                            x => x.Measure != null ? x.Measure.Uom : null,
                            (x, value) => { if (x.Measure != null) x.Measure.Uom = value; });

                        scopesToDeprovision.Add(typeof(HrItem).GetScope());
                    }
                    replaced = replacing.Keys;
                }
                else if (type == typeof(UomType))
                {
                    var replacing = GetReplaceEntities<UomType>(entities);
                    if (replacing.Count > 0)
                    {
                        UpdateParents<UomType, Uom>(replacing,
                            x => x.Type,
                            (x, value) => x.Type = value);

                        scopesToDeprovision.Add(typeof(Uom).GetScope());
                    }
                    replaced = replacing.Keys;
                }
                else if (type == typeof(Speciality))
                {
                    var replacing = GetReplaceEntities<Speciality>(entities);
                    if (replacing.Count > 0)
                    {
                        UpdateParents<Speciality, Doctor>(replacing,
                            x => x.Speciality,
                            (x, value) => x.Speciality = value);
                        UpdateParents<Speciality, SpecialityIcdBlocks>(replacing,
                            x => x.Speciality,
                            (x, value) => x.Speciality = value);

                        scopesToDeprovision.Add(typeof(Doctor).GetScope());
                        scopesToDeprovision.Add(typeof(SpecialityIcdBlocks).GetScope());
                    }
                    replaced = replacing.Keys;
                }
                else if (type == typeof(SpecialityIcdBlocks))
                {
                    var replacing = GetReplaceEntities<SpecialityIcdBlocks>(entities);
                    replaced = replacing.Keys;
                    // нет ссылок на SpecialityIcdBlocks, нечего обновлять
                }
                else
                    throw new NotImplementedException();

                CleanupReplaced(replaced);
            }
            return scopesToDeprovision;
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
        private void UpdateParents<T, TUpdate>(Dictionary<T, T> toReplace, Func<TUpdate, T> propertyGetter, Action<TUpdate, T> propertySetter)
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
            if (replaced == null)
                return;

            var list = replaced.ToArray();

            new Saver(session).Delete(list);
            OnReplaced(new ListEventArgs<IEntity>(list));
        }
    }
}