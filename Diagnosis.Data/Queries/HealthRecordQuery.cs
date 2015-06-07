using Diagnosis.Common;
using Diagnosis.Models;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
                Contract.Ensures(Contract.Result<IEnumerable<HealthRecord>>() != null);
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
                Contract.Ensures(Contract.Result<IEnumerable<HealthRecord>>() != null);

                mincount = Math.Min(mincount, words.Count()); // хотя бы 2 слова из одного - подходят записи, где это единственное слово

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

#pragma warning disable 0162 // unreachable

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
#pragma warning restore 0162
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
                Contract.Ensures(Contract.Result<IEnumerable<HealthRecord>>() != null);
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
                Contract.Ensures(Contract.Result<IEnumerable<HealthRecord>>() != null);

                mincount = Math.Min(mincount, cwords.Count());

                using (var tr = session.BeginTransaction())
                {
                    var wordsIds = cwords.Select(w => w.HIO.Id).ToList();

                    var hriWithAnyWord = (from hr in session.Query<HealthRecord>()
                                          join hri in session.Query<HrItem>() on hr.Id equals hri.HealthRecord.Id
                                          join w in session.Query<Word>() on hri.Word.Id equals w.Id
                                          where wordsIds.Contains(w.Id)
                                          select hri).ToList();

                    var hrs = from hri in hriWithAnyWord.Where(x => cwords.Any(p => x.Confidence == p.Confidence && x.Word == p.HIO)) // hri где есть любой cword
                              group hri by hri.HealthRecord into g
                              where g.Count() >= mincount
                              select g.Key;

                    return hrs.Distinct().ToList();
                }
            };
        }

        /// <summary>
        /// Возвращает записи со всеми из слов (с точным числом слов).
        /// </summary>
        public static Func<IEnumerable<Word>, IEnumerable<HealthRecord>> WithAllWords(ISession session)
        {
            return (words) =>
            {
                Contract.Ensures(Contract.Result<IEnumerable<HealthRecord>>() != null);

                using (var tr = session.BeginTransaction())
                {
                    // with help of http://stackoverflow.com/questions/8425232/sql-select-all-rows-where-subset-exists
                    // and http://stackoverflow.com/questions/448203/linq-to-sql-using-group-by-and-countdistinct
                    // also http://sqlfiddle.com/#!2/327514/1 and http://johnreilly.me/2009/12/sql-contains-all-query/
                    var wordsIds = words.Select(w => w.Id).ToList();

                    var hrWithAnyWord = (from hr in session.Query<HealthRecord>()
                                         join hri in session.Query<HrItem>() on hr.Id equals hri.HealthRecord.Id
                                         join w in session.Query<Word>() on hri.Word.Id equals w.Id
                                         where wordsIds.Contains(w.Id)
                                         select hr).ToList();

                    var h = from hr in hrWithAnyWord
                            where words.IsSubmultisetOf(hr.Words)
                            select hr;

                    // one query - nhib Query Source could not be identified.
                    //var hrIds = (from hri in hriWithAnyWords
                    //             group hri by hri.HealthRecord.Id into g
                    //             where g.Select(x => x.Word.Id).Count() == wordsIds.Count // те hr, где кол-во слов в hri == переданному - неверно
                    //             select g.Key).ToList();

                    //var qq = from hr in session.Query<HealthRecord>()
                    //         where hrIds.Contains(hr.Id)
                    //         select hr;
                    return h.Distinct().ToList();
                }
            };
        }

        /// <summary>
        /// Возвращает записи со всеми из слов c учетом уверенности.
        /// </summary>
        public static Func<IEnumerable<Confindencable<Word>>, IEnumerable<HealthRecord>> WithAllConfWords(ISession session)
        {
            return (cwords) =>
            {
                Contract.Ensures(Contract.Result<IEnumerable<HealthRecord>>() != null);

                using (var tr = session.BeginTransaction())
                {
                    var wordsIds = cwords.Select(w => w.HIO.Id).ToList();

                    var hriWithAnyWord = (from hr in session.Query<HealthRecord>()
                                          join hri in session.Query<HrItem>() on hr.Id equals hri.HealthRecord.Id
                                          join w in session.Query<Word>() on hri.Word.Id equals w.Id
                                          where wordsIds.Contains(w.Id)
                                          select hr).ToList();
                    var h = from hr in hriWithAnyWord
                            where cwords.IsSubmultisetOf(hr.GetCWords())
                            select hr;
                    return h.Distinct().ToList();
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
                Contract.Ensures(Contract.Result<IEnumerable<HealthRecord>>() != null);

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

        /// <summary>
        /// Возвращает записи где нет ни одного из слов учитывая уверенность.
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public static Func<IEnumerable<Confindencable<Word>>, IEnumerable<HealthRecord>> WithoutAnyConfWord(ISession session)
        {
            return (cwords) =>
            {
                Contract.Ensures(Contract.Result<IEnumerable<HealthRecord>>() != null);

                using (var tr = session.BeginTransaction())
                {
                    // записи без слов + со словами, но без этой уверенности
                    var wordsIds = cwords.Select(w => w.HIO.Id).ToList();

                    var noWordshrs = (from hr in session.Query<HealthRecord>()
                                      where !hr.HrItems.Any(x => wordsIds.Contains(x.Word.Id))
                                      select hr).ToList();

                    var hrWithAnyWord = (from hr in session.Query<HealthRecord>()
                                         where hr.HrItems.Any(x => wordsIds.Contains(x.Word.Id))
                                         select hr).ToList();

                    var otherConfHrs = from hr in hrWithAnyWord
                                       from hri in hr.HrItems
                                       where !cwords.Any(p => hri.Confidence == p.Confidence && hri.Word == p.HIO)
                                       group hri by hri.HealthRecord into g
                                       where g.Count() == g.Key.HrItems.Count() // только записи где все hri прошли
                                       select g.Key;

                    return noWordshrs.Union(otherConfHrs).Distinct().ToList();
                }
            };
        }

        /// <summary>
        /// Возвращает записи кроме тех, где есть все слова.
        /// </summary>
        public static Func<IEnumerable<Word>, IEnumerable<HealthRecord>> WithoutAllWords(ISession session)
        {
            return (words) =>
            {
                throw new NotImplementedException();

#pragma warning disable 0162 // unreachable

                using (var tr = session.BeginTransaction())
                {
                    var wordsIds = words.Select(w => w.Id).ToList();
                    var hrsw = (from hr in session.Query<HealthRecord>()
                                join hri in session.Query<HrItem>() on hr.Id equals hri.HealthRecord.Id
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
#pragma warning restore 0162
            };
        }

        public static Func<IEnumerable<Word>, IEnumerable<Word>, IEnumerable<Word>, IEnumerable<HealthRecord>> WithAllAnyNotWords(ISession session)
        {
            return (all, any, not) =>
            {
                Contract.Ensures(Contract.Result<IEnumerable<HealthRecord>>() != null);

                // записи где слова - надмножество all
                // all.IsSubmultisetOf(hr.Words)
                // var withAny = withall.Where(hr => any.Any(w => hr.Words.DifferenceWith(all).Contains(w))); // TODO minany

                // записи где set(all) - подмножество слов записи
                //all.IsSubsetOf(hr.Words)

                if (any.Any() || all.Any())
                {
                    var withAny = WithAnyWord(session)(any.Any() ? any : all);
                    var withall = withAny.Where(hr => all.IsSubsetOf(hr.Words)); // записи где слова - надмножество all
                    return withall.Where(hr => !hr.Words.Any(w => not.Contains(w)));

                    //IEnumerable<HealthRecord> allany;
                    //if (any.Any() && all.Any())
                    //{
                    //    var withall = WithAllWords(session)(all);
                    //    allany = withall.Where(hr => any.Any(w => hr.Words.Contains(w)));
                    //}
                    //else if (any.Any())
                    //    allany = WithAnyWord(session)(any);
                    //else // all.Any()
                    //    allany = WithAllWords(session)(all);

                    //return allany.Where(hr => !hr.Words.Any(w => not.Contains(w)));
                }
                else
                {
                    return WithoutAnyWord(session)(not);
                }
            };
        }

        // test only
        public static Func<IEnumerable<Word>, IEnumerable<Word>, IEnumerable<Word>, int, IEnumerable<HealthRecord>> WithAllAnyNotWordsMinAny(ISession session)
        {
            return (all, any, not, minAny) =>
            {
                Contract.Ensures(Contract.Result<IEnumerable<HealthRecord>>() != null);

                if (any.Any() || all.Any())
                {
                    var withAny = WithAnyWords(session)(any.Any() ? any : all, minAny);
                    var withall = withAny.Where(hr => all.IsSubsetOf(hr.Words)); // записи где слова - надмножество all
                    return withall.Where(hr => !hr.Words.Any(w => not.Contains(w)));

                }
                else
                {
                    return WithoutAnyWord(session)(not);
                }
            };
        }

        public static Func<IEnumerable<Confindencable<Word>>, IEnumerable<Confindencable<Word>>, IEnumerable<Confindencable<Word>>, IEnumerable<HealthRecord>> WithAllAnyNotConfWords(ISession session)
        {
            return (all, any, not) =>
            {
                Contract.Ensures(Contract.Result<IEnumerable<HealthRecord>>() != null);
                if (any.Any() || all.Any())
                {
                    var withAny = WithAnyConfWord(session)(any.Any() ? any : all);
                    var withall = withAny.Where(hr => all.IsSubsetOf(hr.GetCWords())); // записи где слова - надмножество all
                    return withall.Where(hr => !hr.GetCWords().Any(w => not.Contains(w)));
                }
                else
                {
                    return WithoutAnyConfWord(session)(not);
                }
            };
        }

        // test only
        public static Func<IEnumerable<Confindencable<Word>>, IEnumerable<Confindencable<Word>>, IEnumerable<Confindencable<Word>>, int, IEnumerable<HealthRecord>> WithAllAnyNotConfWordsMinAny(ISession session)
        {
            return (all, any, not, minAny) =>
            {
                Contract.Ensures(Contract.Result<IEnumerable<HealthRecord>>() != null);
                if (any.Any() || all.Any())
                {
                    var withAny = WithAnyConfWords(session)(any.Any() ? any : all, minAny);
                    var withall = withAny.Where(hr => all.IsSubsetOf(hr.GetCWords())); // записи где слова - надмножество all
                    return withall.Where(hr => !hr.GetCWords().Any(w => not.Contains(w)));
                }
                else
                {
                    return WithoutAnyConfWord(session)(not);
                }
            };
        }

        #region QueryScope

        /// <summary>
        /// Возвращает записи со всеми словами в области поиска.
        /// Повторы слов должны быть в записи минимум столько раз, сколько передано.
        /// </summary>
        public static Func<IEnumerable<Word>, HealthRecordQueryAndScope, IEnumerable<HealthRecord>> WithAllWordsInScope(ISession session)
        {
            return (words, scope) =>
            {
                Contract.Ensures(Contract.Result<IEnumerable<HealthRecord>>() != null);
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

        #endregion QueryScope
    }
}