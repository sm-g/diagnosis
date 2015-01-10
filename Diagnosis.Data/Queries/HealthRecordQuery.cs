using Diagnosis.Data.Specs;
using Diagnosis.Models;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using Diagnosis.Common;
using System.Linq;

namespace Diagnosis.Data.Queries
{
    public static class HealthRecordQuery
    {

        /// <summary>
        /// Возвращает записи с любым из слов.
        /// </summary>
        public static Func<IEnumerable<Word>, IEnumerable<HealthRecord>> WithAnyWord(ISession session)
        {
            return (words) =>
            {
                using (var tr = session.BeginTransaction())
                {
                    var wordsIds = words.Select(w => w.Id).ToList();
                    Word word = null;
                    HealthRecord hr = null;
                    HrItem item = null;

                    return session.QueryOver<HealthRecord>(() => hr)
                        .JoinAlias(() => hr.HrItems, () => item)
                        .WhereRestrictionOn(() => item.Word).IsNotNull
                        .JoinAlias(() => item.Word, () => word)
                        .WhereRestrictionOn(() => word.Id).IsIn(wordsIds)
                        .TransformUsing(Transformers.DistinctRootEntity)
                        .List();
                }
            };
        }
        /// <summary>
        /// Возвращает записи со всеми словами в области поиска.
        /// </summary>
        public static Func<IEnumerable<Word>, HealthRecordQueryAndScope, IEnumerable<HealthRecord>> WithAllWords(ISession session)
        {
            return (words, scope) =>
            {
                var withAny = WithAnyWord(session)(words);
                switch (scope)
                {
                    case HealthRecordQueryAndScope.Appointment:
                        return GetHrsInScope(words, withAny, (hr) => hr.Appointment);
                    case HealthRecordQueryAndScope.Course:
                        return GetHrsInScope(words, withAny, (hr) => hr.GetCourse());
                    case HealthRecordQueryAndScope.Patient:
                        return GetHrsInScope(words, withAny, (hr) => hr.GetPatient());
                    default:
                    case HealthRecordQueryAndScope.HealthRecord:
                        return withAny.Where(hr => words.IsSubsetOf(hr.Words));
                }
            };
        }

        private static IEnumerable<HealthRecord> GetHrsInScope(IEnumerable<Word> words, IEnumerable<HealthRecord> hrs, Func<HealthRecord, IHrsHolder> holderOf)
        {
            return (from hr in hrs
                    group hr by holderOf(hr) into g
                    where g.Key != null
                    let allWords = g.Key.GetAllWords()
                    where words.IsSubsetOf(allWords)
                    select g).SelectMany(x => x).ToList();
        }
    }
}