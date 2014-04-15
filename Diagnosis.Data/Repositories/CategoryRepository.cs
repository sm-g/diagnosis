using Diagnosis.Models;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;

namespace Diagnosis.Data.Repositories
{
    public class CategoryRepository : ModelRepository<Category>, ICategoryRepository
    {
        public Category GetByTitle(string title)
        {
            ISession session = NHibernateHelper.GetSession();
            {
                Category cat = session
                    .CreateCriteria(typeof(Category))
                    .Add(Restrictions.Eq("Title", title))
                    .UniqueResult<Category>();
                return cat;
            }
        }
    }
}
