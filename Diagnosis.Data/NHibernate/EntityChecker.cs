using Diagnosis.Models;
using Diagnosis.Common;
using NHibernate.Event;
using System;

namespace Diagnosis.Data
{
    internal interface IEntityCrudTracker
    {
        void Load(IEntity entity);
        void Insert(IEntity entity);
        void Update(IEntity entity);
        void Delete(IEntity entity);
    }

    [Serializable]
    internal class EntityChecker : IEntityCrudTracker
    {
        public void Delete(IEntity entity)
        {

        }

        public void Insert(IEntity entity)
        {
            entity.IsDirty = false;
            if (entity is Word)
            {
                this.Send(Event.WordPersisted, entity.AsParams(MessageKeys.Word));
            }
        }

        public void Update(IEntity entity)
        {
            entity.IsDirty = false;
        }

        public void Load(IEntity entity)
        {

        }
    }

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
}