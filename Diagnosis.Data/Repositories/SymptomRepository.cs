using Diagnosis.Models;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;

namespace Diagnosis.Data.Repositories
{
    public class SymptomRepository : ModelRepository<Symptom>, ISymptomRepository
    {
        public Symptom GetByTitle(string title)
        {
            ISession session = NHibernateHelper.GetSession();
            {
                Symptom symptom = session
                    .CreateCriteria(typeof(Symptom))
                    .Add(Restrictions.Eq("Title", title))
                    .UniqueResult<Symptom>();
                return symptom;
            }
        }
    }
}
