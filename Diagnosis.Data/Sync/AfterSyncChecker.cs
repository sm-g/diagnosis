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
            var scopesToDeprovision = new List<Scope>();
            var types = addedIdsPerType.Keys
                .OrderByDescending(x => x, new RefModelsComparer()); // сначала родители

            // дети обновляются на нового родителя
            // старый родитель удаляется без каскадного удаления детей
            // потом удаляются дети
            foreach (var type in types)
            {
                // uow start
                // TODO в одной транзакции для отмены

                IEnumerable<IEntity> replaced = Enumerable.Empty<IEntity>();
                var ids = addedIdsPerType[type];
                var entities = ids.Select(id => session.Get(type, id)).ToList();

                // проверяем справочные сущности на совпадение
                if (type == typeof(HrCategory))
                    replaced = Do<HrCategory>(scopesToDeprovision, entities);
                else if (type == typeof(Uom))
                    replaced = Do<Uom>(scopesToDeprovision, entities);
                else if (type == typeof(UomType))
                    replaced = Do<UomType>(scopesToDeprovision, entities);
                else if (type == typeof(UomFormat))
                    replaced = Do<UomFormat>(scopesToDeprovision, entities);
                else if (type == typeof(Speciality))
                    replaced = Do<Speciality>(scopesToDeprovision, entities);
                else if (type == typeof(Vocabulary))
                    replaced = Do<Vocabulary>(scopesToDeprovision, entities);
                else if (type == typeof(SpecialityIcdBlocks))
                    replaced = Do<SpecialityIcdBlocks>(scopesToDeprovision, entities);
                else if (type == typeof(SpecialityVocabularies))
                    replaced = Do<SpecialityVocabularies>(scopesToDeprovision, entities);
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

        private IEnumerable<T> Do<T>(List<Scope> scopesToDeprovision, List<object> entities)
             where T : IEntity
        {
            var rh = RHFactory.Create<T>();

            var replacing = GetReplaceEntities(rh, entities.Cast<T>());
            if (replacing.Count > 0)
            {
                rh.UpdateInChilds(session, replacing);
                rh.Childs.ForAll(x =>
                    scopesToDeprovision.AddRange(x.GetScopes()));
            }
            return replacing.Keys;
        }

        /// <summary>
        /// Возвращает сущности для замены с таким же значением. Cловарь со значениями { oldEntity, newEntity }
        /// </summary>
        /// <typeparam name="T">Тип сущности справочника для замены</typeparam>
        /// <param name="entities"></param>
        private Dictionary<T, T> GetReplaceEntities<T>(RH<T> rh, IEnumerable<T> entities)
            where T : IEntity
        {
            var toReplace = new Dictionary<T, T>();
            foreach (var item in entities)
            {
                var existing = session.Query<T>()
                    .Where(x => x.Id != item.Id)
                    .Where(rh.EqualsByVal(item))
                    .FirstOrDefault();

                if (existing != null)
                    toReplace[existing] = item;
            }

            return toReplace;
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

    /// <summary>
    /// Родители больше детей
    /// </summary>
    public class RefModelsComparer : IComparer<Type>
    {
        public int Compare(Type x, Type y)
        {
            var rh = RHFactory.Create(x);
            return rh.CompareTo(y);
        }
    }
}