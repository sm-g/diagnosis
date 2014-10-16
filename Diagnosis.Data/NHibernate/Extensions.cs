using NHibernate;
using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Persister.Entity;
using NHibernate.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Data
{
    public static class DirtyExtensions
    {
        /// <summary>
        /// Возвращает индексы измененных свойств.
        /// </summary>
        public static IList<int> GetDirtyProps(this ISession session, object entity)
        {
            EntityEntry oldEntry = GetOldEntry(session, entity);

            ISessionImplementor sessionImpl = session.GetSessionImplementation();
            string className = oldEntry.EntityName;
            IEntityPersister persister = sessionImpl.Factory.GetEntityPersister(className);

            object[] oldState = oldEntry.LoadedState;
            object[] currentState = persister.GetPropertyValues(entity, sessionImpl.EntityMode);
            int[] dirtyProps = persister.FindDirty(currentState, oldState, entity, sessionImpl);

            List<int> dirty = new List<int>();
            for (int i = 0; i < currentState.Length; i++)
            {
                var ipc = currentState[i] as IPersistentCollection;
                if (ipc != null && ipc.IsDirty)
                {
                    dirty.Add(i);
                }
            }

            if (dirtyProps != null)
            {
                dirty.AddRange(dirtyProps);
            }
            return dirty;
        }

        /// <summary>
        /// Возвращает индексы измененных свойств, в том числе типа IPersistentCollection.
        /// </summary>
        public static IList<int> GetDirtyPropsAndCollections(this ISession session, object entity)
        {
            EntityEntry oldEntry = GetOldEntry(session, entity);

            ISessionImplementor sessionImpl = session.GetSessionImplementation();
            string className = oldEntry.EntityName;
            IEntityPersister persister = sessionImpl.Factory.GetEntityPersister(className);

            object[] currentState = persister.GetPropertyValues(entity, sessionImpl.EntityMode);

            var dirtyProps = GetDirtyProps(session, entity);

            List<int> dirty = new List<int>(dirtyProps);
            for (int i = 0; i < currentState.Length; i++)
            {
                var ipc = currentState[i] as IPersistentCollection;
                if (ipc != null && ipc.IsDirty)
                {
                    dirty.Add(i);
                }
            }

            return dirty;
        }

        public static object GetOriginalEntityProperty(this ISession session, object entity, String propertyName)
        {
            EntityEntry oldEntry = GetOldEntry(session, entity);
            ISessionImplementor sessionImpl = session.GetSessionImplementation();
            string className = oldEntry.EntityName;
            IEntityPersister persister = sessionImpl.Factory.GetEntityPersister(className);

            object[] oldState = oldEntry.LoadedState;
            object[] currentState = persister.GetPropertyValues(entity, sessionImpl.EntityMode);
            int[] dirtyProps = persister.FindDirty(currentState, oldState, entity, sessionImpl);
            int index = Array.IndexOf(persister.PropertyNames, propertyName);

            bool isDirty = (dirtyProps != null) ? (Array.IndexOf(dirtyProps, index) != -1) : false;

            return ((isDirty == true) ? oldState[index] : currentState[index]);
        }

        public static bool IsDirtyEntity(this ISession session, object entity)
        {
            return GetDirtyPropsAndCollections(session, entity).Count > 0;
        }
        public static bool IsDirtyProperty(this ISession session, object entity, String propertyName)
        {
            EntityEntry oldEntry = GetOldEntry(session, entity);
            ISessionImplementor sessionImpl = session.GetSessionImplementation();
            string className = oldEntry.EntityName;
            IEntityPersister persister = sessionImpl.Factory.GetEntityPersister(className);

            int propIndex = Array.IndexOf(persister.PropertyNames, propertyName);
            var dirtyProps = GetDirtyPropsAndCollections(session, entity);
            return dirtyProps.Contains(propIndex);
        }

        public static T Unproxy<T>(this ISession session, T obj)
        {
            return (T)session.GetSessionImplementation().PersistenceContext.Unproxy(obj);
        }

        private static EntityEntry GetOldEntry(ISession session, object entity)
        {
            var sessionImpl = session.GetSessionImplementation();
            var oldEntry = sessionImpl.PersistenceContext.GetEntry(entity);

            if ((oldEntry == null) && (entity is INHibernateProxy))
            {
                oldEntry = sessionImpl.PersistenceContext.GetEntry(
                    sessionImpl.PersistenceContext.Unproxy(entity));
            }
            return oldEntry;
        }
    }
}
