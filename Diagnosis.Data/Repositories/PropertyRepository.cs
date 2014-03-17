using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diagnosis.Models;
using NHibernate;
using NHibernate.Cfg;

namespace Diagnosis.Data.Repositories
{
    public class PropertyRepository : IPropertyRepository
    {
        public void Add(Property entity)
        {
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Save(entity);
                transaction.Commit();
            }
        }
        public void Update(Property entity)
        {
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Update(entity);
                transaction.Commit();
            }
        }
        public void Remove(Property entity)
        {
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Delete(entity);
                transaction.Commit();
            }
        }
        public IEnumerable<Property> GetAll()
        {
            ISession session = NHibernateHelper.GetSession();
            return session.CreateCriteria(typeof(Property)).List<Property>();
        }
        public Property GetById(int entityId)
        {
            ISession session = NHibernateHelper.GetSession();
            return session.Get<Property>(entityId);
        }
        public Property GetByTitle(string name)
        {
            ISession session = NHibernateHelper.GetSession();
            {
                return session
                    .CreateCriteria(typeof(Property))
                    .Add(NHibernate.Criterion.Restrictions.Eq("Title", name))
                    .UniqueResult<Property>();
            }
        }
    }
}
