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
    public class PatientPropertyRepository : IPatientPropertyRepository
    {
        public void Add(PatientProperty entity)
        {
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Save(entity);
                transaction.Commit();
            }
        }
        public void Update(PatientProperty entity)
        {
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Update(entity);
                transaction.Commit();
            }
        }
        public void Remove(PatientProperty entity)
        {
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Delete(entity);
                transaction.Commit();
            }
        }
        public IEnumerable<PatientProperty> GetAll()
        {
            ISession session = NHibernateHelper.GetSession();
                return session.CreateCriteria(typeof(PatientProperty)).List<PatientProperty>();
        }
        public PatientProperty GetById(int entityId)
        {
            ISession session = NHibernateHelper.GetSession();
                return session.Get<PatientProperty>(entityId);
        }

        public IEnumerable<PatientProperty> GetByPatient(Patient patient)
        {
            ISession session = NHibernateHelper.GetSession();
                return session.CreateCriteria(typeof(PatientProperty)).
                    Add(Restrictions.Where<PatientProperty>(x => x.Patient == patient)).List<PatientProperty>();
        }
    }
}
