using Diagnosis.Models;
using NHibernate;
using System.Collections.Generic;

namespace Diagnosis.Data.Repositories
{
    public class IcdChapterRepository : ModelRepository<IcdChapter>, IRepository<IcdChapter>
    {
        public IcdChapter GetByTitle(string title)
        {
            ISession session = NHibernateHelper.GetSession();
            {
                return session
                    .CreateCriteria(typeof(IcdChapter))
                    .Add(NHibernate.Criterion.Restrictions.Eq("Title", title))
                    .UniqueResult<IcdChapter>();
            }
        }
        public IcdChapter GetByCode(string code)
        {
            ISession session = NHibernateHelper.GetSession();
            {
                return session
                    .CreateCriteria(typeof(IcdChapter))
                    .Add(NHibernate.Criterion.Restrictions.Eq("Code", code))
                    .UniqueResult<IcdChapter>();
            }
        }
    }
}
