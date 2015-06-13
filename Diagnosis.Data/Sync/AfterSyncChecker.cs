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
    public sealed class AfterSyncChecker
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
        /// Дети обновляются на нового родителя.
        /// Старый родитель удаляется (без каскадного удаления детей, которых у него больше нет).
        /// </summary>
        /// <param name="addedIdsPerType"></param>
        /// <returns></returns>
        public IEnumerable<Scope> CheckReferenceEntitiesAfterDownload(Dictionary<Type, IEnumerable<object>> addedIdsPerType)
        {
            Contract.Requires(addedIdsPerType != null);
            var scopesToDeprovision = new List<Scope>();
            var types = addedIdsPerType.Keys
                .OrderByDescending(x => x, new RefModelsComparer()); // сначала родители

            var replacedAll = new List<IEntity>();
            var updatedAll = new List<IEntity>();

            foreach (var type in types)
            {
                EntitiesTuple replacedUpdated;
                var ids = addedIdsPerType[type];
                var newEntities = ids.Select(id => session.Get(type, id)).ToList();

                // проверяем справочные сущности на совпадение
                if (type == typeof(HrCategory))
                    replacedUpdated = Do<HrCategory>(scopesToDeprovision, newEntities);
                else if (type == typeof(Uom))
                    replacedUpdated = Do<Uom>(scopesToDeprovision, newEntities);
                else if (type == typeof(UomType))
                    replacedUpdated = Do<UomType>(scopesToDeprovision, newEntities);
                else if (type == typeof(UomFormat))
                    replacedUpdated = Do<UomFormat>(scopesToDeprovision, newEntities);
                else if (type == typeof(Speciality))
                    replacedUpdated = Do<Speciality>(scopesToDeprovision, newEntities);
                else if (type == typeof(Vocabulary))
                    replacedUpdated = Do<Vocabulary>(scopesToDeprovision, newEntities);
                else if (type == typeof(SpecialityIcdBlocks))
                    replacedUpdated = Do<SpecialityIcdBlocks>(scopesToDeprovision, newEntities);
                else if (type == typeof(SpecialityVocabularies))
                    replacedUpdated = Do<SpecialityVocabularies>(scopesToDeprovision, newEntities);
                else
                    throw new NotImplementedException();

                replacedAll.AddRange(replacedUpdated.replaced);
                updatedAll.AddRange(replacedUpdated.updatedChilds);
            }

            // сохраняем обновленных детей
            // потом удаляем замененные
            // может быть так, что однобленные дети тут же будут удалены
            session.DeleteAndSave(replacedAll, updatedAll);

            OnReplaced(new ListEventArgs<IEntity>(replacedAll));

            return scopesToDeprovision.Distinct();
        }

        private struct EntitiesTuple
        {
            public IEntity[] replaced;
            public IEntity[] updatedChilds;

            public EntitiesTuple(IEntity[] replaced, IEntity[] updatedChilds)
            {
                this.replaced = replaced;
                this.updatedChilds = updatedChilds;
            }
        }

        /// <summary>
        /// определяет сущности для замены
        /// меняет ссылку у их детей
        /// добавляет scopes детей в scopesToDeprovision
        /// </summary>
        /// <param name="newEntities">Новые сущности типа T</param>
        private EntitiesTuple Do<T>(List<Scope> scopesToDeprovision, List<object> newEntities)
             where T : IEntity
        {
            var rh = RHFactory.Create<T>();
            IEntity[] toSave = new IEntity[0];

            var replacing = GetReplaceEntities(rh, newEntities.Cast<T>());
            if (replacing.Count > 0)
            {
                toSave = rh.UpdateInChilds(session, replacing).ToArray();
                rh.Childs.ForAll(x =>
                    scopesToDeprovision.AddRange(x.GetScopes()));
            }
            return new EntitiesTuple(replacing.Keys.Cast<IEntity>().ToArray(), toSave);
        }

        /// <summary>
        /// Возвращает словарь сущностей { oldEntity, newEntity } для замены с таким же значением.
        /// </summary>
        /// <typeparam name="T">Тип сущности справочника для замены</typeparam>
        /// <param name="newEntities"></param>
        private Dictionary<T, T> GetReplaceEntities<T>(RH<T> rh, IEnumerable<T> newEntities)
            where T : IEntity
        {
            var toReplace = new Dictionary<T, T>();
            foreach (var newE in newEntities)
            {
                var existing = session.Query<T>()
                    .Where(x => x.Id != newE.Id)
                    .Where(rh.EqualsByVal(newE))
                    .FirstOrDefault();

                if (existing != null)
                    toReplace[existing] = newE;
            }

            return toReplace;
        }

        private void OnReplaced(ListEventArgs<IEntity> e)
        {
            var h = Replaced;
            if (h != null)
            {
                h(this, e);
            }
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