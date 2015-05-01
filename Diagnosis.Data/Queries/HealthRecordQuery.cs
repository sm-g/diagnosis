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
        /// Возвращает все записи.
        /// </summary>
        public static Func<IEnumerable<HealthRecord>> All(ISession session)
        {
            return () =>
            {
                using (var tr = session.BeginTransaction())
                {
                    return session.Query<HealthRecord>().ToList();
                }
            };
        }

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
                              where hr.HrItems.Any(x => wordsIds.Contains(x.Word.Id))
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
                    var hrIdQ = session.CreateSQLQuery(
                      @"select distinct hr.ID
                        from HealthRecord as hr
                        join HrItem as hri on hr.Id = hri.HealthRecordID
                        join  Word as w on hri.WordID = w.Id
                        where hri.WordID is not null
                        and (hri.WordID in (:words))
                        group by hr.ID
                        having count(hri.WordID)>=:mincount"
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
        /// Возвращает записи с любым из слов учитывая уверенность.
        /// </summary>
        public static Func<IEnumerable<Confindencable<Word>>, IEnumerable<HealthRecord>> WithAnyConfWord(ISession session)
        {
            return (cwords) =>
            {
                using (var tr = session.BeginTransaction())
                {
                    var wordsIds = cwords.Select(w => w.HIO.Id).ToList();

                    var hriWithAnyWord = (from hr in session.Query<HealthRecord>()
                                          join hri in session.Query<HrItem>() on hr.Id equals hri.HealthRecord.Id
                                          join w in session.Query<Word>() on hri.Word.Id equals w.Id
                                          where wordsIds.Contains(w.Id)
                                          select hri).ToList();
                    var hrs = from x in hriWithAnyWord
                              where cwords.Any(p => x.Confidence == p.Confidence && x.Word == p.HIO)
                              select x.HealthRecord;
                    return hrs.Distinct().ToList();
                }
            };
        }
        /// <summary>
        /// Возвращает записи с хотя бы N из слов учитывая уверенность (c повторами).
        /// </summary>
        public static Func<IEnumerable<Confindencable<Word>>, int, IEnumerable<HealthRecord>> WithAnyConfWords(ISession session)
        {
            return (cwords, mincount) =>
            {
                using (var tr = session.BeginTransaction())
                {
                    var wordsIds = cwords.Select(w => w.HIO.Id).ToList();

                    var hriWithAnyWord = (from hr in session.Query<HealthRecord>()
                                          join hri in session.Query<HrItem>() on hr.Id equals hri.HealthRecord.Id
                                          join w in session.Query<Word>() on hri.Word.Id equals w.Id
                                          where wordsIds.Contains(w.Id)
                                          select hri).ToList();

                    var hrs = from hri in hriWithAnyWord.Where(x => cwords.Any(p => x.Confidence == p.Confidence && x.Word == p.HIO))
                              group hri by hri.HealthRecord into g
                              where g.Count() >= mincount
                              select g.Key;

                    return hrs.Distinct().ToList();
                }
            };
        }
        /// <summary>
        /// Возвращает записи со всеми из слов.
        /// </summary>
        public static Func<IEnumerable<Word>, IEnumerable<HealthRecord>> WithAllWords(ISession session)
        {
            return (words) =>
            {
                using (var tr = session.BeginTransaction())
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
                                 where g.Select(x => x.Word.Id).Count() == wordsIds.Count // те hr, где кол-во слов в hri == переданному
                                 select g.Key).ToList();

                    var qq = from hr in session.Query<HealthRecord>()
                             where hrIds.Contains(hr.Id)
                             select hr;
                    return qq.ToList();
                }
            };
        }

        /// <summary>
        /// Возвращает записи где нет ни одного из слов.
        /// </summary>
        public static Func<IEnumerable<Word>, IEnumerable<HealthRecord>> WithoutAnyWord(ISession session)
        {
            return (words) =>
            {
                using (var tr = session.BeginTransaction())
                {
                    var wordsIds = words.Select(w => w.Id).ToList();

                    var hrs = from hr in session.Query<HealthRecord>()
                              where !hr.HrItems.Any(x => wordsIds.Contains(x.Word.Id)) // все элементы записи без слова
                              select hr;
                    return hrs.Distinct().ToList();
                }
            };
        }

        public static Func<IEnumerable<Confindencable<Word>>, IEnumerable<HealthRecord>> WithoutAnyConfWord(ISession session)
        {
            return (cwords) =>
            {
                throw new NotImplementedException();
                //using (var tr = session.BeginTransaction())
                //{
                //    var wordsIds = words.Select(w => w.Id).ToList();

                //    var hrs = from hr in session.Query<HealthRecord>()
                //              where !hr.HrItems.Any(x => wordsIds.Contains(x.Word.Id)) // все элементы записи без слова
                //              select hr;
                //    return hrs.Distinct().ToList();
                //}
            };
        }
        /// <summary>
        /// Возвращает записи кроме тех, где есть все слова.
        /// </summary>
        public static Func<IEnumerable<Word>, IEnumerable<HealthRecord>> WithoutAllWords(ISession session)
        {
            return (words) =>
            {
                // throw new NotImplementedException();
                using (var tr = session.BeginTransaction())
                {
                    var wordsIds = words.Select(w => w.Id).ToList();
                    var hrsw = (from hr in session.Query<HealthRecord>()
                                join hri in session.Query<HrItem>() on hr.Id equals hri.HealthRecord.Id
                                //  where hri.Word != null
                                join w in session.Query<Word>() on hri.Word.Id equals w.Id
                                group w.Id by hr).ToList();
                    var hrs = from p in hrsw
                              let ws1 = from wId in p.Distinct()
                                        where wordsIds.Contains(wId)
                                        select wId
                              where ws1.Count() < wordsIds.Count
                              select p.Key;
                    //var hrs = from hr in session.Query<HealthRecord>()
                    //          where !hr.HrItems.All(x => wordsIds.Contains(x.Word.Id))
                    //          select hr;
                    return hrs.Distinct().ToList();
                }
            };
        }
        /// <summary>
        /// Возвращает записи со всеми словами в области поиска.
        /// Повторы слов должны быть в записи минимум столько раз, сколько передано.
        /// </summary>
        public static Func<IEnumerable<Word>, HealthRecordQueryAndScope, IEnumerable<HealthRecord>> WithAllWordsInScope(ISession session)
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
                        return withAny.Where(hr => words.IsSubmultisetOf(hr.Words)).ToList();
                }
            };
        }

        private static IEnumerable<HealthRecord> GetHrsInScope(IEnumerable<Word> words, IEnumerable<HealthRecord> hrs, Func<HealthRecord, IHrsHolder> holderOf)
        {
            return (from hr in hrs
                    group hr by holderOf(hr) into g
                    where g.Key != null
                    let allWords = g.Key.GetAllWords()
                    where words.IsSubmultisetOf(allWords)
                    select g).SelectMany(x => x).ToList();
        }

        public static Func<IEnumerable<Word>, IEnumerable<Word>, IEnumerable<Word>, IEnumerable<HealthRecord>> WithAllAnyNotWords(ISession session)
        {
            return (all, any, not) =>
            {
                if (any.Any() || all.Any())
                {
                    var withAny = WithAnyWord(session)(any.Any() ? any : all);
                    var withall = withAny.Where(hr => all.IsSubmultisetOf(hr.Words));
                    return withall.Where(hr => !hr.Words.Any(w => not.Contains(w)));
                }
                else
                {
                    return WithoutAnyWord(session)(not);
                }
            };
        }

        public static Func<IEnumerable<Confindencable<Word>>, IEnumerable<Confindencable<Word>>, IEnumerable<Confindencable<Word>>, int, IEnumerable<HealthRecord>> WithAllAnyNotConfWordsMinAny(ISession session)
        {
            return (all, any, not, minAny) =>
            {
                if (any.Any() || all.Any())
                {
                    var withAny = WithAnyConfWords(session)(any.Any() ? any : all, minAny);
                    var withall = withAny.Where(hr => all.IsSubsetOf(hr.GetCWords()));
                    return withall.Where(hr => !hr.GetCWords().Any(w => not.Contains(w)));
                }
                else
                {
                    return WithoutAnyWord(session)(not.Select(x => x.HIO));
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
                    var withall = withAny.Where(hr => all.IsSubmultisetOf(hr.Words));
                    return withall.Where(hr => !hr.Words.Any(w => not.Contains(w)));
                }
                else
                {
                    return WithoutAnyWord(session)(not);
                }
            };
        }
    }
}