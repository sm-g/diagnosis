using Diagnosis.Models;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;

namespace Diagnosis.Data.Repositories
{
    public class DoctorRepository : ModelRepository<Doctor>, IDoctorRepository
    {
        public Doctor GetByName(string name)
        {
            ISession session = NHibernateHelper.GetSession();
            {
                Doctor doctor = session
                    .CreateCriteria(typeof(Patient))
                    .Add(Restrictions.Eq("LastName", name))
                    .UniqueResult<Doctor>();
                return doctor;
            }
        }
    }
}