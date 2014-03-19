using Diagnosis.Models;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;

namespace Diagnosis.Data.Repositories
{
    public abstract class ModelRepository<T> : IRepository<T> where T : class
    {
        public void Add(T entity)
        {
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Save(entity);
                transaction.Commit();
            }
        }
        public void Update(T entity)
        {
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Update(entity);
                transaction.Commit();
            }
        }
        public void SaveOrUpdate(T entity)
        {
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(entity);
                transaction.Commit();
            }
        }
        public void Remove(T entity)
        {
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Delete(entity);
                transaction.Commit();
            }
        }
        public IEnumerable<T> GetAll()
        {
            ISession session = NHibernateHelper.GetSession();
            return session.CreateCriteria(typeof(T)).List<T>();
        }
        public T GetById(int entityId)
        {
            ISession session = NHibernateHelper.GetSession();
            return session.Get<T>(entityId);
        }
    }
}
