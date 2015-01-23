#if DEBUG
//#define LOG
#endif

using Diagnosis.Models;
using NHibernate.Event;
using NHibernate.Persister.Entity;
using System;
using System.Linq;

namespace Diagnosis.Data.NHibernate
{
    [Serializable]
    internal class PostEventListener : IPostInsertEventListener, IPostUpdateEventListener, IPostDeleteEventListener, IPostLoadEventListener
    {
        private readonly IEntityCrudTracker _checker;

        public PostEventListener()
            : this(new EntityChecker())
        { }

        public PostEventListener(IEntityCrudTracker checker)
        {
            _checker = checker;
        }

        public void OnPostInsert(PostInsertEvent e)
        {
            _checker.Insert(e.Entity as IEntity);
        }

        public void OnPostUpdate(PostUpdateEvent e)
        {
            _checker.Update(e.Entity as IEntity);
        }

        public void OnPostDelete(PostDeleteEvent e)
        {
            _checker.Delete(e.Entity as IEntity);
        }

        public void OnPostLoad(PostLoadEvent e)
        {
            _checker.Load(e.Entity as IEntity);
        }
    }

    [Serializable]
    internal class PreEventListener : IPreInsertEventListener, IPreUpdateEventListener, IPreDeleteEventListener, IPreLoadEventListener
    {
        private readonly IEntityCrudTracker _logger = null;

        public PreEventListener()
            : this(new Logger())
        { }

        public PreEventListener(IEntityCrudTracker logger)
        {
#if LOG
            _logger = logger;
#endif
        }

        public bool OnPreInsert(PreInsertEvent e)
        {
            if (_logger != null)
                _logger.Insert(e.Entity as IEntity);

            return false;
        }

        public bool OnPreUpdate(PreUpdateEvent e)
        {
            if (_logger != null)
                _logger.Update(e.Entity as IEntity);

            var audit = e.Entity as IHaveAuditInformation;
            if (audit == null)
                return false;

            var time = DateTime.Now;
            Set(e.Persister, e.State, "UpdatedAt", time);
            audit.UpdatedAt = time;

            return false;
        }

        public bool OnPreDelete(PreDeleteEvent e)
        {
            if (_logger != null)
                _logger.Delete(e.Entity as IEntity);
            return false;
        }

        public void OnPreLoad(PreLoadEvent e)
        {
            if (_logger != null)
                _logger.Load(e.Entity as IEntity);
        }

        private void Set(IEntityPersister persister, object[] state, string propertyName, object value)
        {
            var index = Array.IndexOf(persister.PropertyNames, propertyName);
            if (index == -1)
                return;
            state[index] = value;
        }
    }
}