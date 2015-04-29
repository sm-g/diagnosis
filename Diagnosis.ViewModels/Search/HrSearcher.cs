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
        static bool logOn = false;
        public static IEnumerable<HealthRecord> Search(ISession session, SearchOptions options)
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
                   options.MeasuresAny.Any(op => x.Measures.Any(m => op.ResultFor(m)))
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
                          let ms = options.MeasuresAny.Where(op => hr.Measures.Any(m => op.ResultFor(m))).Count()
                          where ms + ws >= options.MinAny
                          select hr;
                }
            }

            // все измерения - просто фильтр
            if (options.MeasuresAll.Count() > 0)
            {
                // одно измерение повторно используется для сравнения
                hrs = hrs.Where(x =>
                   options.MeasuresAll.All(op => x.Measures.Any(m => op.ResultFor(m))));
            }

            if (options.Categories.Count() > 0)
            {
                hrs = hrs.Where(hr =>
                    options.Categories.Any(cat => cat.Equals(hr.Category)));
            }

            return hrs.ToList();
        }

        public static IEnumerable<HealthRecord> SearchJustNoWords(ISession session, SearchOptions options)
        {
            Contract.Requires(options != null);
            Contract.Requires(session != null);

            var hrs = HealthRecordQuery.WithoutAnyWord(session)(
                options.WordsNot);

            if (options.Categories.Count() > 0)
            {
                hrs = hrs.Where(hr =>
                    options.Categories.Any(cat => cat.Equals(hr.Category)));
            }

            return hrs.ToList();
        }

        public static IEnumerable<HealthRecord> SearchOld(ISession session, OldHrSearchOptions options)
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
                   options.MeasuresAll.All(op => x.Measures.Any(m => op.ResultFor(m))));
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
        public static IEnumerable<HealthRecord> GetResult(ISession session, SearchOptions qb)
        {
            // по исключающим блокам не ищем
            Contract.Assume(qb.IsRoot || !qb.IsExcluding);
            logger.DebugFormat("{0}", qb);
            if (qb.IsGroup)
            {
                var anyNormal = qb.Children.Any(x => !x.IsExcluding);
                var anyExcluding = qb.Children.Any(x => x.IsExcluding);

                var beforeExclude = new Dictionary<IHrsHolder, IEnumerable<HealthRecord>>();

                if (anyNormal)
                    beforeExclude = GetAllAnyHrs(session, qb);
                else
                {
                    // если в группе только исключающие блоки, в результате
                    // все записи, которые хотели найти (без слов, в категории)
                    // beforeExclude как будто все записи
                }
                if (logOn)
                    logger.DebugFormat("beforeExclude {0}: {1}", beforeExclude.Values.Count(), Log(beforeExclude.SelectMany(d => d.Value)));

                if (anyExcluding)
                {
                    var hrs = ApplyExcluding(session, qb, anyNormal, beforeExclude);
                    if (logOn)
                        logger.DebugFormat("apply ex: {0}", Log(hrs));
                    return hrs;
                }
                else // в группе нет исключающих блоков
                {
                    return beforeExclude.SelectMany(x => x.Value);
                }
            }
            else
            {
                return HrSearcher.Search(session, qb);
            }
        }

        private static IEnumerable<HealthRecord> ApplyExcluding(ISession session,
            SearchOptions qb,
            bool anyNormal,
            Dictionary<IHrsHolder, IEnumerable<HealthRecord>> beforeExclude)
        {
            // исключающие блоки 
            // All убирают записи
            // "в списке записи со словом Х и ни одной со словом У"
            Contract.Ensures(qb.GroupOperator != QueryGroupOperator.All || !anyNormal ||
                Contract.Result<IEnumerable<HealthRecord>>().Count() <= beforeExclude.SelectMany(x => x.Value).Count());
            // Any добавляют
            // "в списке записи со словом Х или без слова У"
            Contract.Ensures(qb.GroupOperator != QueryGroupOperator.Any || !anyNormal ||
               Contract.Result<IEnumerable<HealthRecord>>().Count() >= beforeExclude.SelectMany(x => x.Value).Count());
            // NotAny убирают
            // в записях ни X ни У
            Contract.Ensures(qb.GroupOperator != QueryGroupOperator.NotAny || !anyNormal ||
               Contract.Result<IEnumerable<HealthRecord>>().Count() <= beforeExclude.SelectMany(x => x.Value).Count());

            //если только исключающие, сначала получаем все записи без слов c нужными атрибутами

            var ex = from q in qb.Children
                     where q.IsExcluding
                     select new
                     {
                         Qb = q,
                         ExWords = q.WordsNot,
                         Cats = q.Categories,
                         JustNoHrs = HrSearcher.SearchJustNoWords(session, q)
                     };

            if (qb.GroupOperator == QueryGroupOperator.Any)
            {
                // к записям, полученным из результатов неисключающих блоков +
                // те, где просто нет исключенных слов

                // проходят списки где хоть одна запись без исключенных

                var beforeHrs = beforeExclude.SelectMany(x => x.Value);
                var justNo = ex.SelectMany(x => x.JustNoHrs);

                if (logOn)
                    logger.DebugFormat("any. before\n {0}\n+ justNo\n {1}", Log(beforeHrs), Log(justNo));

                return beforeHrs.Union(justNo).Distinct();
            }
            if (!anyNormal) // only ex blocks
            {
                var exQbHrsList = ex.Select(x => x.JustNoHrs).ToList();

                if (logOn)
                    logger.DebugFormat("only ex. justNo {0}", Log(exQbHrsList.SelectMany(d => d)));

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
                        beforeExclude = InHolder(exQbHrsList);
                        break;

                    case SearchScope.Patient:
                        beforeExclude = InPatient(exQbHrsList);
                        break;
                }
            }

            if (logOn)
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
        /// Записи, полученные из результатов неисключающих блоков группы.
        /// </summary>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> GetAllAnyHrs(ISession session, SearchOptions qb)
        {
            Contract.Requires(!qb.IsExcluding);

            // результаты из неисключающих блоков
            var qbResultsList = (from q in qb.Children
                                 where !q.IsExcluding
                                 select GetResult(session, q)).ToList();

            var table = new Dictionary<SearchScope,
                Dictionary<QueryGroupOperator,
                    Func<IList<IEnumerable<HealthRecord>>,
                        Dictionary<IHrsHolder, IEnumerable<HealthRecord>>>>>();
            table[SearchScope.HealthRecord] = new Dictionary<QueryGroupOperator, Func<IList<IEnumerable<HealthRecord>>, Dictionary<IHrsHolder, IEnumerable<HealthRecord>>>>();
            table[SearchScope.Holder] = new Dictionary<QueryGroupOperator, Func<IList<IEnumerable<HealthRecord>>, Dictionary<IHrsHolder, IEnumerable<HealthRecord>>>>();
            table[SearchScope.Patient] = new Dictionary<QueryGroupOperator, Func<IList<IEnumerable<HealthRecord>>, Dictionary<IHrsHolder, IEnumerable<HealthRecord>>>>();

            // группировка по holder не используется
            table[SearchScope.HealthRecord][QueryGroupOperator.All] = InHr;
            table[SearchScope.HealthRecord][QueryGroupOperator.Any] = AnyInHr;

            // нужен словарь в ключом holder для исключения
            table[SearchScope.Holder][QueryGroupOperator.All] = InHolder;
            table[SearchScope.Holder][QueryGroupOperator.Any] = AnyInHolder;
            table[SearchScope.Patient][QueryGroupOperator.All] = InPatient;
            table[SearchScope.Patient][QueryGroupOperator.Any] = AnyInPatient;

            return table[qb.SearchScope][qb.GroupOperator](qbResultsList);
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
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> InHolderScope(
            IList<IEnumerable<HealthRecord>> results,
            Func<HealthRecord, IHrsHolder> getScope)
        {
            // плоско все найденные #Блока-Область-Записи
            var qbHolderHrs = results
                .Select((hrs, i) => new
                {
                    Numer = i,
                    Gr = hrs.GroupBy(hr => getScope(hr))
                })
                .SelectMany(x => x.Gr.Select(gr => new
                {
                    Numer = x.Numer,
                    Scope = gr.Key,
                    Hrs = gr.Cast<HealthRecord>()
                }));

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
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> AnyInHolderScope(
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
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> InHr(IList<IEnumerable<HealthRecord>> results)
        {
            return Intersect(results);
        }

        /// <summary>
        /// Любое в одной записи
        ///
        /// В результатах все записи полученные из каждого блока.
        /// </summary>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> AnyInHr(IList<IEnumerable<HealthRecord>> results)
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
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> InPatient(IList<IEnumerable<HealthRecord>> results)
        {
            return InHolderScope(results, (hr) => hr.GetPatient());
        }

        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> InHolder(IList<IEnumerable<HealthRecord>> results)
        {
            return InHolderScope(results, (hr) => hr.Holder);
        }

        /// <summary>
        /// Любое в том же списке
        ///
        /// Список попадет в результаты, если нашлась хотя бы одна запись из списка в любом блоке.
        /// То есть все записи проходят в результаты.
        ///
        /// (или в списке нет записей-исключений)
        /// </summary>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> AnyInHolder(IList<IEnumerable<HealthRecord>> results)
        {
            return AnyInHolderScope(results, (hr) => hr.Holder);
        }

        /// <summary>
        /// Любое в области пациента
        ///
        /// Пациент попадет в результаты, если нашлась хотя бы одна запись из него в любом блоке.
        /// </summary>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> AnyInPatient(IList<IEnumerable<HealthRecord>> results)
        {
            return AnyInHolderScope(results, (hr) => hr.GetPatient());
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