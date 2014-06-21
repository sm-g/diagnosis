using Diagnosis.Models;
using NHibernate;
using NHibernate.Transform;
using NHibernate.Criterion;
using System.Linq;
using System.Collections.Generic;
using Diagnosis.Core;

namespace Diagnosis.Data.Repositories
{
    public class HealthRecordRepository : ModelRepository<HealthRecord>
    {
        public IEnumerable<HealthRecord> GetByWords(IEnumerable<Word> words)
        {
            var wordsIds = words.Select(w => w.Id).ToList();
            Word word = null;
            Symptom symptom = null;
            HealthRecord hr = null;
            SymptomWords sw = null;

            ISession session = NHibernateHelper.GetSession();
            {
                var w1 = session.QueryOver<HealthRecord>(() => hr)
                    .JoinAlias(() => hr.Symptom, () => symptom)
                    .JoinAlias(() => symptom.SymptomWords, () => sw)
                    .JoinAlias(() => sw.Word, () => word)
                    .WhereRestrictionOn(() => word.Id).IsIn(wordsIds)
                    .TransformUsing(Transformers.DistinctRootEntity)
                    .List();

                return w1;
            }
        }

        public IEnumerable<HealthRecord> GetWithWordsSubset(IEnumerable<Word> words)
        {
            var wordsIds = words.Select(w => w.Id).ToList();
            var comparator = new CompareWord();
            return GetAll().Where(hr => hr.Symptom != null
                && words.IsSubsetOf(hr.Symptom.Words));
        }

        public IEnumerable<HealthRecord> GetWithAllWords(IEnumerable<Word> words)
        {
            var wordsIds = words.Select(w => w.Id).ToList();
            var comparator = new CompareWord();
            return GetAll().Where(hr => hr.Symptom != null
                && hr.Symptom.Words.OrderBy(w => w, comparator).SequenceEqual(
                              words.OrderBy(w => w, comparator)));
        }
    }
}
