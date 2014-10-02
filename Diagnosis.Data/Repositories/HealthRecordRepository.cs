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
        /// <summary>
        /// Записи, содержащие любое из слов.
        /// </summary>
        public IEnumerable<HealthRecord> GetByWords(IEnumerable<Word> words)
        {
            var wordsIds = words.Select(w => w.Id).ToList();
            Word word = null;
            HealthRecord hr = null;
            HrItem item = null;

            ISession session = NHibernateHelper.GetSession();
            {
                var w1 = session.QueryOver<HealthRecord>(() => hr)
                    .JoinAlias(() => hr.HrItems, () => item)
                    .WhereRestrictionOn(() => item.Word).IsNotNull
                    .JoinAlias(() => item.Word, () => word)
                    .WhereRestrictionOn(() => word.Id).IsIn(wordsIds)
                    .TransformUsing(Transformers.DistinctRootEntity)
                    .List();

                return w1;
            }
        }
        /// <summary>
        /// Записи, в которых есть все слова, могут быть и другие.
        /// </summary>
        public IEnumerable<HealthRecord> GetWithWordsSubset(IEnumerable<Word> words)
        {
            var wordsIds = words.Select(w => w.Id).ToList();
            return GetAll().Where(hr => words.IsSubsetOf(hr.Words));
        }
        /// <summary>
        /// Записи, в которых есть все слова и нет других.
        /// </summary>
        public IEnumerable<HealthRecord> GetWithAllWords(IEnumerable<Word> words)
        {
            //var wordsIds = words.Select(w => w.Id).ToList();
            //var comparator = new CompareWord();
            //return GetAll().Where(hr => hr.Symptom != null
            //    && hr.Symptom.Words.OrderBy(w => w, comparator).SequenceEqual(
            //                  words.OrderBy(w => w, comparator)));
            return null;
        }
    }
}
