using Diagnosis.Models;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;

namespace Diagnosis.Data.Repositories
{
    public class WordRepository : ModelRepository<Word>, IWordRepository
    {
        public Word GetByTitle(string title)
        {
            ISession session = NHibernateHelper.GetSession();
            {
                Word word = session
                    .CreateCriteria(typeof(Word))
                    .Add(Restrictions.Eq("Title", title))
                    .UniqueResult<Word>();
                return word;
            }
        }
    }
}
