using Diagnosis.Models;
using Diagnosis.Common;
using NHibernate.Event;
using System;

namespace Diagnosis.Data
{
    internal interface IEntityChecher
    {
        void Delete(IEntity entity);
        void Insert(IEntity entity);
        void Update(IEntity entity);
        void Load(IEntity entity);
    }

    [Serializable]
    internal class EntityChecker : IEntityChecher
    {
        public void Delete(IEntity entity)
        {

        }

        public void Insert(IEntity entity)
        {
            entity.IsDirty = false;
            if (entity is Word)
            {
                this.Send(Events.WordPersisted, entity.AsParams(MessageKeys.Word));
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
        private readonly IEntityChecher _checker;

        public PostEventListener()
            : this(new EntityChecker())
        { }

        public PostEventListener(IEntityChecher checker)
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