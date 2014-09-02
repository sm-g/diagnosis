using LinqSpecs;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Data.Repositories
{
    public interface INHibernateRepository<T>
    {
        void Create(T entity);
        void Update(T entity);
        void Delete(T entity);
        IEnumerable<T> FindAll(Specification<T> specification);
        T GetById(int id);
    }

    public class NHibernateRepository<T> : INHibernateRepository<T>
    {
        ISession session;
        public NHibernateRepository(ISession session)
        {
            this.session = session;
        }

        public T GetById(int id)
        {
            return session.Get<T>(id);
        }
        public void Create(T entity)
        {
            session.SaveOrUpdate(entity);
        }

        public void Update(T entity)
        {
            session.SaveOrUpdate(entity);
        }

        public void Delete(T entity)
        {
            session.Delete(entity);
        }

        public IEnumerable<T> FindAll(Specification<T> specification)
        {
            var query = GetQuery(specification);
            return query.ToList(); // sql executed here, keep all sql exceptions here
        }

        private IQueryable<T> GetQuery(Specification<T> specification)
        {
            return session.Query<T>()
              .Where(specification.IsSatisfiedBy());
        }
    }


}
