using Diagnosis.Models;
using NHibernate;
using System.Collections.Generic;

namespace Diagnosis.Data.Repositories
{
    public class DiagnosisRepository : ModelRepository<Diagnosis.Models.Diagnosis>, IDiagnosisRepository
    {
        public Diagnosis.Models.Diagnosis GetByTitle(string title)
        {
            ISession session = NHibernateHelper.GetSession();
            {
                return session
                    .CreateCriteria(typeof(Diagnosis.Models.Diagnosis))
                    .Add(NHibernate.Criterion.Restrictions.Eq("Title", title))
                    .UniqueResult<Diagnosis.Models.Diagnosis>();
            }
        }
        public Diagnosis.Models.Diagnosis GetByCode(string code)
        {
            ISession session = NHibernateHelper.GetSession();
            {
                return session
                    .CreateCriteria(typeof(Diagnosis.Models.Diagnosis))
                    .Add(NHibernate.Criterion.Restrictions.Eq("Code", code))
                    .UniqueResult<Diagnosis.Models.Diagnosis>();
            }
        }
    }
}
