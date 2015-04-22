using Diagnosis.Common;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
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
                var exQbHrsList = ex.Select(x => x.JustNoHrs).ToList();

                logger.DebugFormat("all, only ex. justNo {0}", Log(exQbHrsList.SelectMany(d => d)));

                switch (qb.SearchScope)
                {
                    case SearchScope.HealthRecord:
                        // пересечение записей с блока
                        // сделать так же как список, чтобы проверять атрибуты?
                        var result =
                             from holderHrs in Intersect(exQbHrsList)
                             from hr in holderHrs.Value
                             select hr;
                        return result;

                    // только исключающие блоки вычитают из всех записей области, где есть по одной записи без слов
                    case SearchScope.Holder:
                        beforeExclude = InOneHolder(exQbHrsList);
                        break;

                    case SearchScope.Patient:
                        beforeExclude = InOnePatient(exQbHrsList);
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
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> GetAllAnyHrs(ISession session, HrSearchOptions qb)
        {
            Contract.Requires(!qb.IsExcluding);

            // результаты из неисключающих блоков
            var qbResultsList = (from q in qb.Children
                                 where !q.IsExcluding
                                 select GetResult(session, q)).ToList();

            switch (qb.SearchScope)
            {
                case SearchScope.HealthRecord: // группировка по holder не используется
                    if (qb.All)
                        return InOneHr(qbResultsList);
                    else
                        return AnyInOneHr(qbResultsList);

                case SearchScope.Holder:
                    if (qb.All)
                        return InOneHolder(qbResultsList);
                    else
                        return AnyInOneHolder(qbResultsList);

                case SearchScope.Patient:
                    if (qb.All)
                        return InOnePatient(qbResultsList);
                    else
                        return AnyInOnePatient(qbResultsList);

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
        /// <returns>Все подходящие записи из подходящих областей.</returns>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> InOneHolderScope(
            IList<IEnumerable<HealthRecord>> results,
            Func<HealthRecord, IHrsHolder> getScope)
        {
            // плоско все найденные #Блока-Область-Записи
            var qbHolderHrs = results
                .Select((hrs, i) => new { Numer = i, Gr = hrs.GroupBy(hr => getScope(hr)) })
                .SelectMany(x => x.Gr.Select(gr => new { Numer = x.Numer, Scope = gr.Key, Hrs = gr.Cast<HealthRecord>() }));

            var qbCount = results.Count;

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

        /// <summary>
        /// Любое в области
        ///
        /// Область попадет в результаты, если нашлась хотя бы одна запись из нее в любом блоке.
        /// То есть все записи проходят в результаты.
        /// </summary>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> AnyInOneHolderScope(
            IList<IEnumerable<HealthRecord>> results,
            Func<HealthRecord, IHrsHolder> getScope)
        {
            var q = from hrs in results
                    from hr in hrs
                    group hr by getScope(hr) into g
                    select new { Holder = g.Key, Hrs = g.Cast<HealthRecord>() };
            return q.ToDictionary(x => x.Holder as IHrsHolder, x => x.Hrs);
        }

        /// <summary>
        /// Все в одной записи
        ///
        /// В результатах пересечение записей каждого блока.
        /// </summary>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> InOneHr(IList<IEnumerable<HealthRecord>> results)
        {
            Contract.Requires(results.Count > 0);

            return Intersect(results);
        }

        /// <summary>
        /// Любое в одной записи
        ///
        /// В результатах все записи полученные из каждого блока.
        /// </summary>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> AnyInOneHr(IList<IEnumerable<HealthRecord>> results)
        {
            return (from hrs in results
                    from hr in hrs
                    group hr by hr.Holder).ToDictionary(x => x.Key, x => x.Distinct());
        }

        /// <summary>
        /// Все в одной области пациента
        ///
        /// В каждом блоке должны найтись записи из пациента, тогда пациент попадает в результаты,
        /// где есть все найденные записи из него.
        /// </summary>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> InOnePatient(IList<IEnumerable<HealthRecord>> results)
        {
            return InOneHolderScope(results, (hr) => hr.GetPatient());
        }

        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> InOneHolder(IList<IEnumerable<HealthRecord>> results)
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
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> AnyInOneHolder(IList<IEnumerable<HealthRecord>> results)
        {
            return AnyInOneHolderScope(results, (hr) => hr.Holder);
        }

        /// <summary>
        /// Любое в области пациента
        ///
        /// Пациент попадет в результаты, если нашлась хотя бы одна запись из него в любом блоке.
        /// </summary>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> AnyInOnePatient(IList<IEnumerable<HealthRecord>> results)
        {
            return AnyInOneHolderScope(results, (hr) => hr.GetPatient());
        }

        /// <summary>
        /// Пересечение в каждом блоке.
        /// </summary>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> Intersect(IList<IEnumerable<HealthRecord>> results)
        {
            if (results.Count == 0)
                return new Dictionary<IHrsHolder, IEnumerable<HealthRecord>>();

            var hrs = results[0];
            for (int i = 1; i < results.Count; i++)
            {
                hrs = hrs.Intersect(results[i]);
            }

            return hrs
                .GroupBy(x => x.Holder)
                .ToDictionary(x => x.Key, x => x.Distinct());
        }

        private static string Log(IEnumerable<HealthRecord> hrs)
        {
            return string.Join<HealthRecord>("\n", hrs.ToArray());
        }
    }
}