using Diagnosis.Models;
using NHibernate;
using System.Collections.Generic;

namespace Diagnosis.Data.Repositories
{
    public class IcdDiseaseRepository : ModelRepository<IcdDisease>, IRepository<IcdDisease>
    {
        public IcdDisease GetByTitle(string title)
        {
            ISession session = NHibernateHelper.GetSession();
            {
                return session
                    .CreateCriteria(typeof(IcdDisease))
                    .Add(NHibernate.Criterion.Restrictions.Eq("Title", title))
                    .UniqueResult<IcdDisease>();
            }
        }
        public IcdDisease GetByCode(string code)
        {
            ISession session = NHibernateHelper.GetSession();
            {
                return session
                    .CreateCriteria(typeof(IcdDisease))
                    .Add(NHibernate.Criterion.Restrictions.Eq("Code", code))
                    .UniqueResult<IcdDisease>();
            }
        }
    }
}
