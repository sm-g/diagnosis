using LinqSpecs;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Diagnosis.Data.Specs
{
    public class NHibernateRepository<T>
    {
        protected ISession session;
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
        public IEnumerable<T> FindAll(Expression<Func<T, bool>> linq)
        {
            return session.Query<T>()
              .Where(linq).ToList();
        }
        public IEnumerable<T> FindAll()
        {
            return session.Query<T>().ToList();
        }

        private IQueryable<T> GetQuery(Specification<T> specification)
        {
            return session.Query<T>()
              .Where(specification.IsSatisfiedBy());
        }
    }


}
