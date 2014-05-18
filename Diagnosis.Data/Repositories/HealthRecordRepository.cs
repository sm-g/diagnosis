using Diagnosis.Models;
using NHibernate;
using NHibernate.Criterion;
using System.Linq;
using System.Collections.Generic;

namespace Diagnosis.Data.Repositories
{
    public class HealthRecordRepository : ModelRepository<HealthRecord>
    {
        public IEnumerable<HealthRecord> GetByWords(IEnumerable<Word> words)
        {
            //var comparator = new CompareWord();
            //return 
            //GetAll().Where(hr => hr.Symptom != null
            //    && hr.Symptom.Words.OrderBy(w => w, comparator).SequenceEqual(
            //                  words.OrderBy(w => w, comparator)));
            var wordsIds = words.Select(w => w.Id).ToList();
            Word word = null;
            Symptom symptom = null;
            // HealthRecord hr = null;

            var subq1 = QueryOver.Of<Word>(() => word)
                .WhereRestrictionOn(() => word.Id).IsIn(wordsIds)
                .Select(s => symptom.Id);

            var subq = QueryOver.Of<Symptom>(() => symptom)
                .Inner.JoinAlias(() => symptom.Words, () => word)
                .WhereRestrictionOn(() => word.Id).IsIn(wordsIds)
                .Select(s => symptom.Id);

            ISession session = NHibernateHelper.GetSession();
            {
                var w1 = session.QueryOver<HealthRecord>().WithSubquery
                    .WhereProperty(hr => hr.Symptom.Id)
                    .In(subq)
                    .List();

                return w1;
            }
        }
    }
}
