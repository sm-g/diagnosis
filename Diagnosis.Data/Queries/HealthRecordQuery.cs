using Diagnosis.Common;
using Diagnosis.Models;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Transform;
using System;
using System.Collections.Generic;
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

                    var q = from hr0 in session.Query<HealthRecord>()
                            join hri in session.Query<HrItem>() on hr0.Id equals hri.HealthRecord.Id
                            where hri.Word != null
                            join w in session.Query<Word>() on hri.Word.Id equals w.Id
                            where wordsIds.Contains(w.Id)
                            select hr0;
                    return q.Distinct().ToList();

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
                        return withAny.Where(hr => words.IsSubsetOf(hr.Words)).ToList();
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

        public static Func<IEnumerable<Word>, IEnumerable<Word>, IEnumerable<Word>, IEnumerable<HealthRecord>> WithAllAnyNotWords(ISession session)
        {
            return (all, any, not) =>
            {
                //using (var tr = session.BeginTransaction())
                //{
                //var allIds = all.Select(w => w.Id).ToList();
                //var anyIds = any.Select(w => w.Id).ToList();
                //var notIds = not.Select(w => w.Id).ToList();


                //var q = from hr in session.Query<HealthRecord>()
                //        join hri in session.Query<HrItem>() on hr.Id equals hri.HealthRecord.Id
                //        where hri.Word != null
                //        join w in session.Query<Word>() on hri.Word.Id equals w.Id
                //        where !notIds.Contains(w.Id)
                //        where (anyIds.Count == 0 || anyIds.Contains(w.Id))
                //        select hr;

                //var wordsInHr = from hr0 in q
                //                join hri0 in session.Query<HrItem>() on hr0.Id equals hri0.HealthRecord.Id
                //                where hri0.Word != null
                //                where allIds.Except(hri.W)

                //                select hr0;
                //    return wordsInHr.Distinct().ToList();

                //}
                if (any.Any() || all.Any())
                {
                    var withAny = WithAnyWord(session)(any.Any() ? any : all);
                    var withall = withAny.Where(hr => all.IsSubsetOf(hr.Words));
                    return withall.Where(hr => !hr.Words.Any(w => not.Contains(w)));
                }
                else
                {
                    return session.Query<HealthRecord>().ToList()
                        .Where(x => x.Words.All(w => !not.Contains(w))).ToList();
                }

            };
        }
    }
}