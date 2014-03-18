using Diagnosis.Models;
using NHibernate;
using System.Collections.Generic;

namespace Diagnosis.Data.Repositories
{
    public class PropertyRepository : ModelRepository<Property>, IPropertyRepository
    {
        public Property GetByTitle(string title)
        {
            ISession session = NHibernateHelper.GetSession();
            {
                return session
                    .CreateCriteria(typeof(Property))
                    .Add(NHibernate.Criterion.Restrictions.Eq("Title", title))
                    .UniqueResult<Property>();
            }
        }
    }
}
