using Diagnosis.Common;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Screens;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Search
{
    public class HrSearcher
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HrSearcher));

        public IEnumerable<HealthRecord> Search(ISession session, HrSearchOptions options)
        {
            Contract.Requires(options != null);
            Contract.Requires(session != null);

            var hrs = HealthRecordQuery.WithAllAnyNotWordsMinAny(session)(
                options.WordsAll,
                options.WordsAny,
                options.WordsNot,
                options.MinAny - options.MeasuresAny.Count()); // хотя бы 3 из w1,w2,m1,m2 - достаточно 1 слова

            // измерения фильтруем отдельно - сложная логика

            if (options.MeasuresAny.Count() > 0)
            {
                // любое из измерений
                // ищем со словами из измерений с учетом notwords
                var hrsWithM = HealthRecordQuery.WithAllAnyNotWordsMinAny(session)(
                    Enumerable.Empty<Word>(),
                    options.MeasuresAny.Select(x => x.Word),
                    options.WordsNot,
                    options.MinAny - options.WordsAny.Count()
                    );

                // фильтр по WordsAll и MeasuresAny
                hrsWithM = hrsWithM.Where(x =>
                   options.WordsAll.IsSubsetOf(x.Words) &&
                   options.MeasuresAny.Any(m => x.Measures.Contains(m, new ValueComparer(m.Operator)))
                   );

                if (options.WordsAll.Any() || options.WordsAny.Any())
                    hrs = hrs.Union(hrsWithM);
                else // только измерения в all / any
                    hrs = hrsWithM;

                if (options.MinAny > 1 && options.WordsAny.Count() > 0)
                {
                    // проверяем, набрала ли запись общее кол-во элементов из Any
                    hrs = from hr in hrs
                          let ws = hr.Words.Intersect(options.WordsAny).Count()
                          let ms = options.MeasuresAny.Where(m => hr.Measures.Contains(m, new ValueComparer(m.Operator))).Count()
                          where ms + ws >= options.MinAny
                          select hr;
                }
            }

            // все измерения - просто фильтр
            if (options.MeasuresAll.Count() > 0)
            {
                hrs = hrs.Where(x =>
                   options.MeasuresAll.All(m => x.Measures.Contains(m, new ValueComparer(m.Operator))));
            }

            if (options.Categories.Count() > 0)
            {
                hrs = hrs.Where(hr =>
                    options.Categories.Any(cat => cat.Equals(hr.Category)));
            }

            return hrs.ToList();
        }

        public IEnumerable<HealthRecord> SearchJustNoWords(ISession session, HrSearchOptions options)
        {
            Contract.Requires(options != null);
            Contract.Requires(session != null);

            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(
                Enumerable.Empty<Word>(),
                Enumerable.Empty<Word>(),
                options.WordsNot);

            if (options.Categories.Count() > 0)
            {
                hrs = hrs.Where(hr =>
                    options.Categories.Any(cat => cat.Equals(hr.Category)));
            }

            return hrs.ToList();
        }

        public IEnumerable<HealthRecord> SearchOld(ISession session, OldHrSearchOptions options)
        {
            Contract.Requires(options != null);
            Contract.Requires(session != null);

            IEnumerable<HealthRecord> hrs;

            if (options.WordsAll.Count() > 0)
                if (options.AllWords)
                {
                    hrs = HealthRecordQuery.WithAllWordsInScope(session)(options.WordsAll, options.QueryScope);
                }
                else
                {
                    hrs = HealthRecordQuery.WithAnyWord(session)(options.WordsAll);
                }
            else
            {
                hrs = session.Query<HealthRecord>();
            }

            if (options.MeasuresAll.Count() > 0)
            {
                hrs = hrs.Where(x =>
                   options.MeasuresAll.All(m => x.Measures.Contains(m, new ValueComparer(m.Operator))));
            }

            if (options.Categories.Count() > 0)
            {
                hrs = hrs.Where(hr =>
                    options.Categories.Any(cat => cat.Equals(hr.Category)));
            }

            return hrs.ToList();
        }

        /// <summary>
        /// Записи, возвращаемые блоком.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="qb"></param>
        /// <returns></returns>
        public static IEnumerable<HealthRecord> GetResult(ISession session, HrSearchOptions qb)
        {
            // по исключающим блокам не ищем
            Contract.Assume(qb.IsRoot || !qb.IsExcluding);
            logger.DebugFormat("{0}", qb);
            if (qb.IsGroup)
            {
                var anyNonExQbInGroup = qb.Children.Any(x => !x.IsExcluding);
                var anyExQbInGroup = qb.Children.Any(x => x.IsExcluding);

                var beforeExclude = new Dictionary<IHrsHolder, IEnumerable<HealthRecord>>();

                if (anyNonExQbInGroup)
                    beforeExclude = GetAllAnyHrs(session, qb);
                else
                {
                    // если в группе только исключающие блоки, в результате
                    // (нет записей - чтобы только не видеть записи с тем словами)
                    // все записи списка/вообще все - их и хотели найти
                    //    ведь надо что-то показывать
                }

                logger.DebugFormat("beforeExclude {0}: {1}", beforeExclude.Values.Count(), Log(beforeExclude.SelectMany(d => d.Value)));

                if (anyExQbInGroup)
                {
                    var hrs = ApplyExcluding(session, qb, anyNonExQbInGroup, beforeExclude);
                    logger.DebugFormat("apply ex: {0}", Log(hrs));
                    return hrs;
                }
                else
                {
                    // в группе нет исключающих блоков
                    return from holderHrs in beforeExclude
                           from hr in holderHrs.Value
                           select hr;
                }
            }
            else
            {
                return new HrSearcher().Search(session, qb);
            }
        }

        private static IEnumerable<HealthRecord> ApplyExcluding(ISession session,
            HrSearchOptions qb,
            bool anyNonExQbInGroup,
            Dictionary<IHrsHolder, IEnumerable<HealthRecord>> beforeExclude)
        {
            // исключающие блоки в режиме All убирают записи
            // "в списке записи со словом Х и ни одной со словом У"
            // в режиме Any добавляют
            // "в списке записи со словом Х или без слова У"
            //
            var ex = from q in qb.Children
                     where q.IsExcluding
                     select new
                     {
                         Qb = q,
                         ExWords = q.WordsNot,
                         Cats = q.Categories,
                         JustNoHrs = new HrSearcher().SearchJustNoWords(session, q)
                     };

            if (!qb.All)
            {
                // к записям, полученным из результатов неисключающих блоков +
                // те, где просто нет исключенных слов

                // проходят списки где хоть одна запись без исключенных

                var beforeHrs = beforeExclude.SelectMany(x => x.Value);
                var justNo = ex.SelectMany(x => x.JustNoHrs);

                logger.DebugFormat("any. before\n {0}\n+ justNo\n {1}", Log(beforeHrs), Log(justNo));

                return beforeHrs.Union(justNo).Distinct();
            }
            if (!anyNonExQbInGroup) // all, only ex blocks
            {
                var exQbHrsDict = ex.ToDictionary(x => x.Qb, x => x.JustNoHrs);

                logger.DebugFormat("all, only ex. justNo {0}", Log(exQbHrsDict.SelectMany(d => d.Value)));

                switch (qb.SearchScope)
                {
                    case SearchScope.HealthRecord:
                        // пересечение записей с блока
                        // сделать так же как список, чтобы проверять атрибуты?
                        var result =
                             from holderHrs in Intersect(exQbHrsDict)
                             from hr in holderHrs.Value
                             select hr;
                        return result;

                    // только исключающие блоки вычитают из всех записей области, где есть по одной записи без слов
                    case SearchScope.Holder:
                        beforeExclude = InOneHolder(exQbHrsDict);
                        break;

                    case SearchScope.Patient:
                        beforeExclude = InOnePatient(exQbHrsDict);
                        break;
                }
            }

            logger.DebugFormat("all, with non ex, scope {0}", qb.SearchScope);

            switch (qb.SearchScope)
            {
                case SearchScope.HealthRecord:
                    // не проходят отдельные записи, у которых совпадает атрибут и есть исключенные слова
                    return from hrs in beforeExclude.Values
                           from hr in hrs
                           from e in ex
                           where !(
                                    (!e.Cats.Any() || e.Cats.Contains(hr.Category)) &&
                                    hr.Words.Intersect(e.ExWords).Any()
                                   )
                           select hr;

                case SearchScope.Holder:
                    // хоть одна запись, у которых совпадает атрибут и есть исключенные слова - весь список не проходит
                    return from holderHrs in beforeExclude
                           let hrs = holderHrs.Key.HealthRecords // все записи списка
                           where ex.All(e => !hrs.Any(hr => // ни одной записи из исключающих блоков
                                           (!e.Cats.Any() || e.Cats.Contains(hr.Category)) && // категории
                                           hr.Words.Intersect(e.ExWords).Any()) // со словами
                               )
                           from hr in holderHrs.Value // записи из beforeExclude
                           select hr;

                case SearchScope.Patient:
                    return from holderHrs in beforeExclude
                           let hrs = holderHrs.Key.GetAllHrs() // все записи пациента
                           where ex.All(e => !hrs.Any(hr => // ни одной записи из исключающих блоков
                                           (!e.Cats.Any() || e.Cats.Contains(hr.Category)) && // категории
                                           hr.Words.Intersect(e.ExWords).Any()) // со словами
                               )
                           from hr in holderHrs.Value // записи из beforeExclude
                           select hr;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Записи, полученные из результатов неисключающих блоков
        /// </summary>
        /// <param name="qb"></param>
        /// <param name="qbResults"></param>
        /// <returns></returns>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> GetAllAnyHrs(ISession session, HrSearchOptions qb)
        {
            Contract.Requires(!qb.IsExcluding);

            // результаты из неисключающих блоков
            var qbResults = (from q in qb.Children
                             where !q.IsExcluding
                             select new
                             {
                                 Qb = q,
                                 Hrs = GetResult(session, q)
                             }).ToDictionary(x => x.Qb, x => x.Hrs);

            switch (qb.SearchScope)
            {
                case SearchScope.HealthRecord: // группировка по holder не используется
                    if (qb.All)
                        return InOneHr(qbResults);
                    else
                        return AnyInOneHr(qbResults);

                case SearchScope.Holder:
                    if (qb.All)
                        return InOneHolder(qbResults);
                    else
                        return AnyInOneHolder(qbResults);

                case SearchScope.Patient:
                    if (qb.All)
                        return InOnePatient(qbResults);
                    else
                        return AnyInOnePatient(qbResults);

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Все в одной области.
        ///
        /// В каждом блоке должны найтись записи из области, тогда область попадает в результаты,
        /// где есть все найденные записи из них.
        ///
        /// И не должно быть ни одной исключающей записи.
        /// </summary>
        /// <param name="results"></param>
        /// <returns>Все подходящие записи из подходящих областей.</returns>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> InOneHolderScope(
            Dictionary<HrSearchOptions, IEnumerable<HealthRecord>> results,
            Func<HealthRecord, IHrsHolder> getScope)
        {
            // HrSearchOptions только разные значения для группировки

            // плоско все найденные Блок-Область-Записи
            var qbHolderHrs = from qbHrs in results
                              let middle =
                                  from hr in qbHrs.Value
                                  group hr by getScope(hr) into g
                                  select new { Hrs = g.Cast<HealthRecord>(), Scope = g.Key }
                              from q in middle
                              select new { Qb = qbHrs.Key, Scope = q.Scope, Hrs = q.Hrs };

            var qbCount = results.Keys.Count;

            // подходящие области и подходящие записи из них
            var holderHrs = from a in qbHolderHrs
                            group a by a.Scope into g
                            where g.Count() == qbCount // область во всех блоках (g.Count() - кол-во блоков, связанных с областью)
                            select new
                            {
                                Scope = g.Key,
                                Hrs = (from b in qbHolderHrs
                                       where b.Scope == g.Key
                                       from hr in b.Hrs
                                       select hr).Distinct()
                            };

            return holderHrs.ToDictionary(x => x.Scope, x => x.Hrs);
        }

        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> InOneHolderScope2(
           IEnumerable<HealthRecord> results,
           Func<HealthRecord, IHrsHolder> holder)
        {
            var qbHolderHrs = from hr in results
                              group hr by holder(hr) into g
                              where g.Key.HealthRecords.Count() == g.Count() // все записи списка переданы
                              select new { Hrs = g.Cast<HealthRecord>(), Holder = g.Key };

            return qbHolderHrs.ToDictionary(x => x.Holder, x => x.Hrs);
        }

        /// <summary>
        /// Все в одной записи
        ///
        /// В результатах пересечение записей каждого блока.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> InOneHr(Dictionary<HrSearchOptions, IEnumerable<HealthRecord>> results)
        {
            Contract.Requires(results.Count > 0);

            return Intersect(results);
        }

        /// <summary>
        /// Пересечение в каждом блоке.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> Intersect(Dictionary<HrSearchOptions, IEnumerable<HealthRecord>> results)
        {
            // HrSearchOptions только разные значения для группировки

            if (results.Count == 0)
                return new Dictionary<IHrsHolder, IEnumerable<HealthRecord>>();

            IEnumerable<HealthRecord> hrs = results[results.Keys.First()];
            for (int i = 1; i < results.Count; i++)
            {
                var qb = results.Keys.ElementAt(i);
                hrs = hrs.Intersect(results[qb]);
            }

            return hrs
                .GroupBy(x => x.Holder)
                .ToDictionary(x => x.Key, x => x.Distinct());
        }

        /// <summary>
        /// Любое в одной записи
        ///
        /// В результатах все записи полученные из каждого блока.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> AnyInOneHr(Dictionary<HrSearchOptions, IEnumerable<HealthRecord>> results)
        {
            return (from hrs in results.Values
                    from hr in hrs
                    group hr by hr.Holder).ToDictionary(x => x.Key, x => x.Distinct());
        }

        /// <summary>
        /// Все в одной области пациента
        ///
        /// В каждом блоке должны найтись записи из пациента, тогда пациент попадает в результаты,
        /// где есть все найденные записи из него.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> InOnePatient(Dictionary<HrSearchOptions, IEnumerable<HealthRecord>> results)
        {
            return InOneHolderScope(results, (hr) => hr.GetPatient());
        }

        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> InOneHolder(Dictionary<HrSearchOptions, IEnumerable<HealthRecord>> results)
        {
            return InOneHolderScope(results, (hr) => hr.Holder);
        }

        /// <summary>
        /// Любое в том же списке
        ///
        /// Список попадет в результаты, если нашлась хотя бы одна запись из списка в любом блоке.
        /// То есть все записи проходят в результаты.
        ///
        /// (или в списке нет записей-исключений)
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> AnyInOneHolder(Dictionary<HrSearchOptions, IEnumerable<HealthRecord>> results)
        {
            // список и записи из него
            var q = from hrs in results.Values
                    from hr in hrs
                    group hr by hr.Holder into g
                    select new { Holder = g.Key, Hrs = g.Cast<HealthRecord>() };
            return q.ToDictionary(x => x.Holder, x => x.Hrs);
        }

        /// <summary>
        /// Любое в области пациента
        ///
        /// пациент попадет в результаты, если нашлась хотя бы одна запись из него в любом блоке или в списке нет записей-исключений
        /// То есть все записи проходят в результаты.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> AnyInOnePatient(Dictionary<HrSearchOptions, IEnumerable<HealthRecord>> results)
        {
            var q = from hrs in results.Values
                    from hr in hrs
                    group hr by hr.GetPatient() into g
                    select new { Holder = g.Key, Hrs = g.Cast<HealthRecord>() };
            return q.ToDictionary(x => x.Holder as IHrsHolder, x => x.Hrs);
        }

        private static string Log(IEnumerable<HealthRecord> hrs)
        {
            return string.Join<HealthRecord>("\n", hrs.ToArray());
        }
    }
}