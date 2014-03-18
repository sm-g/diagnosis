﻿using Diagnosis.Models;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;

namespace Diagnosis.Data.Repositories
{
    public class PatientRepository : ModelRepository<Patient>, IPatientRepository
    {
        public Patient GetByName(string name)
        {
            ISession session = NHibernateHelper.GetSession();
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
