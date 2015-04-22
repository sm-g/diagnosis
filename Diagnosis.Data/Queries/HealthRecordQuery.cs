using Diagnosis.Common;
using Diagnosis.Models;
using NHibernate;
using NHibernate.Linq;
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

                    var hrs = from hr in session.Query<HealthRecord>()
                              join hri in session.Query<HrItem>() on hr.Id equals hri.HealthRecord.Id
                              where hri.Word != null
                              join w in session.Query<Word>() on hri.Word.Id equals w.Id
                              where wordsIds.Contains(w.Id)
                              select hr;
                    return hrs.Distinct().ToList();
                }
            };
        }

        /// <summary>
        /// Возвращает записи с хотя бы N из слов (c повторами слов).
        /// </summary>
        public static Func<IEnumerable<Word>, int, IEnumerable<HealthRecord>> WithAnyWords(ISession session)
        {
            return (words, mincount) =>
            {
                mincount = mincount > 0 ? mincount : 1;
                using (var tr = session.BeginTransaction())
                {
                    var wordsIds = words.Select(w => w.Id).ToList();

                    // fix for sqlce
                    var hrIdQ = session.CreateSQLQuery(string.Join(" ",
                        "select distinct hr.ID",
                        "from HealthRecord as hr",
                        "join HrItem as hri on hr.Id = hri.HealthRecordID",
                        "join  Word as w on hri.WordID = w.Id",
                        "where hri.WordID is not null",
                        "and (hri.WordID in (:words))",
                        "group by hr.ID",
                        "having count(hri.WordID)>=:mincount")
                    );
                    hrIdQ.SetParameterList("words", wordsIds);
                    hrIdQ.SetParameter("mincount", mincount);

                    var hrIds = hrIdQ.List<Guid>();
                    var hrQ = from hr in session.Query<HealthRecord>()
                              where hrIds.Contains(hr.Id)
                              select hr;

                    return hrQ.ToList();

                    var hrs = from hr in session.Query<HealthRecord>()
                              let hris = from hri in session.Query<HrItem>()
                                         where hr.Id == hri.HealthRecord.Id
                                         where hri.Word != null
                                         join w in session.Query<Word>() on hri.Word.Id equals w.Id
                                         where wordsIds.Contains(w.Id)
                                         select hri
                              where hris.Count() >= mincount
                              select hr;
                    return hrs.Distinct().ToList();
                }
            };
        }

        /// <summary>
        /// Возвращает записи со всеми словами в области поиска.
        /// </summary>
        public static Func<IEnumerable<Word>, HealthRecordQueryAndScope, IEnumerable<HealthRecord>> WithAllWordsInScope(ISession session)
        {
            return (words, scope) =>
            {
                if (scope == HealthRecordQueryAndScope.HealthRecord)
                {
                    // with help of http://stackoverflow.com/questions/8425232/sql-select-all-rows-where-subset-exists
                    // and http://stackoverflow.com/questions/448203/linq-to-sql-using-group-by-and-countdistinct
                    // also http://sqlfiddle.com/#!2/327514/1 and http://johnreilly.me/2009/12/sql-contains-all-query/
                    var wordsIds = words.Select(w => w.Id).ToList();

                    var hriWithAnyWords = (from hr in session.Query<HealthRecord>()
                                           join hri in session.Query<HrItem>() on hr.Id equals hri.HealthRecord.Id
                                           where hri.Word != null
                                           join w in session.Query<Word>() on hri.Word.Id equals w.Id
                                           where wordsIds.Contains(w.Id)
                                           select hri).ToList();

                    // one query - nhib Query Source could not be identified.

                    var hrIds = (from hri in hriWithAnyWords
                                 group hri by hri.HealthRecord.Id into g
                                 where g.Select(x => x.Word.Id).Distinct().Count() == wordsIds.Count // а если 2 слова в запросе
                                 select g.Key).ToList();

                    var qq = from hr in session.Query<HealthRecord>()
                             where hrIds.Contains(hr.Id)
                             select hr;
                    return qq.ToList();
                }
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
                //int count = 1;

                //var hriWithAnyWords = (from hr in session.Query<HealthRecord>()
                //                       join hri in session.Query<HrItem>() on hr.Id equals hri.HealthRecord.Id
                //                       where hri.Word != null
                //                       join w in session.Query<Word>() on hri.Word.Id equals w.Id
                //                       where allIds.Contains(w.Id)
                //                       select hri).ToList();

                //// one query - nhib Query Source could not be identified.

                //var hrIds = (from hri in hriWithAnyWords
                //             group hri by hri.HealthRecord.Id into g
                //             where g.Select(x => x.Word.Id).Distinct().Count() == allIds.Count
                //             select g.Key).ToList();

                //var qq = from hr in session.Query<HealthRecord>()
                //         where hrIds.Contains(hr.Id)
                //         select hr;

                //var q = from hr in session.Query<HealthRecord>()
                //        let hris = from hri in session.Query<HrItem>()
                //                   where hr.Id == hri.HealthRecord.Id
                //                   where hri.Word != null
                //                   join w in session.Query<Word>() on hri.Word.Id equals w.Id
                //                   where !notIds.Contains(w.Id)
                //                   where (anyIds.Count == 0 || anyIds.Contains(w.Id))
                //                   select hri
                //        where hris.Count() >= count
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
                    // все записи без этих слов
                    return session.Query<HealthRecord>().ToList()
                        .Where(x => x.Words.All(w => !not.Contains(w))).ToList();
                }
            };
        }

        public static Func<IEnumerable<Word>, IEnumerable<Word>, IEnumerable<Word>, int, IEnumerable<HealthRecord>> WithAllAnyNotWordsMinAny(ISession session)
        {
            return (all, any, not, minAny) =>
            {
                if (any.Any() || all.Any())
                {
                    var withAny = WithAnyWords(session)(any.Any() ? any : all, minAny);
                    var withall = withAny.Where(hr => all.IsSubsetOf(hr.Words));
                    return withall.Where(hr => !hr.Words.Any(w => not.Contains(w)));
                }
                else
                {
                    // все записи без этих слов
                    return session.Query<HealthRecord>().ToList()
                        .Where(x => x.Words.All(w => !not.Contains(w))).ToList();
                }
            };
        }
    }
}