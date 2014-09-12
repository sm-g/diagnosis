using Diagnosis.Models;
using NHibernate.Event;
using System;

namespace Diagnosis.Data
{
    internal interface IEntityChecher
    {
        void Delete(EntityBase entity);
        void Insert(EntityBase entity);
        void Update(EntityBase entity);
        void Load(EntityBase entity);
    }

    [Serializable]
    internal class EntityChecker : IEntityChecher
    {
        public void Delete(EntityBase entity)
        {

        }

        public void Insert(EntityBase entity)
        {
            entity.IsDirty = false;
        }

        public void Update(EntityBase entity)
        {
            entity.IsDirty = false;
        }

        public void Load(EntityBase entity)
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
            _checker.Insert(e.Entity as EntityBase);
        }

        public void OnPostUpdate(PostUpdateEvent e)
        {
            _checker.Update(e.Entity as EntityBase);
        }

        public void OnPostDelete(PostDeleteEvent e)
        {
            _checker.Delete(e.Entity as EntityBase);
        }

        public void OnPostLoad(PostLoadEvent e)
        {
            _checker.Load(e.Entity as EntityBase);
        }
    }
}