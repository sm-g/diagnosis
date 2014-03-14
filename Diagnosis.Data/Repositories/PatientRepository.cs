using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diagnosis.Models;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Criterion;

namespace Diagnosis.Data.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        public void Add(Patient entity)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Save(entity);
                transaction.Commit();
            }
        }
        public void Update(Patient entity)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Update(entity);
                transaction.Commit();
            }
        }
        public void Remove(Patient entity)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Delete(entity);
                transaction.Commit();
            }
        }
        public IEnumerable<Patient> GetAll()
        {
            using (ISession session = NHibernateHelper.OpenSession())
                return session.CreateCriteria(typeof(Patient)).List<Patient>().AsQueryable();
        }
        public Patient GetById(int entityId)
        {
            using (ISession session = NHibernateHelper.OpenSession())
                return session.Get<Patient>(entityId);
        }
        public Patient GetByName(string name)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                Patient patient = session
                    .CreateCriteria(typeof(Patient))
                    .Add(Restrictions.Eq("LastName", name))
                    .UniqueResult<Patient>();
                return patient;
            }
        }
    }
}
