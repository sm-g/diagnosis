using Diagnosis.Models;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;

namespace Diagnosis.Data.Repositories
{
    public class PropertyValueRepository : IPropertyValueRepository
    {
        public void Add(PropertyValue entity)
        {
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Save(entity);
                transaction.Commit();
            }
        }
        public void Update(PropertyValue entity)
        {
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Update(entity);
                transaction.Commit();
            }
        }
        public void Remove(PropertyValue entity)
        {
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Delete(entity);
                transaction.Commit();
            }
        }
        public IEnumerable<PropertyValue> GetAll()
        {
            ISession session = NHibernateHelper.GetSession();
                return session.CreateCriteria(typeof(PropertyValue)).List<PropertyValue>();
        }
        public PropertyValue GetById(int entityId)
        {
            ISession session = NHibernateHelper.GetSession();
                return session.Get<PropertyValue>(entityId);
        }
        public PropertyValue GetByValue(string name)
        {
            ISession session = NHibernateHelper.GetSession();
            {
                PropertyValue PropertyValue = session
                    .CreateCriteria(typeof(PropertyValue))
                    .Add(Restrictions.Eq("Title", name))
                    .UniqueResult<PropertyValue>();
                return PropertyValue;
            }
        }
    }
}
