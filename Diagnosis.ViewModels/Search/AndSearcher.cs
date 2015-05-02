using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Search
{
    /// <summary>
    /// для записей All excluding x =  NotAny x
    /// </summary>
    internal class AndSearcher : Searcher
    {
        public AndSearcher(ISession session)
            : base(session)
        {
        }

        protected override IEnumerable<HealthRecord> GetResultForGroup(SearchOptions qb)
        {
            var anyNormal = qb.Children.Any(x => !x.IsExcluding);
            var anyExcluding = qb.Children.Any(x => x.IsExcluding);
            var beforeExclude = new Dictionary<IHrsHolder, IEnumerable<HealthRecord>>();

            if (anyNormal)
                beforeExclude = GetAllAnyHrsInner(qb);
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
                var hrs = ApplyExcludingInner(qb, anyNormal, beforeExclude);
                if (logOn)
                    logger.DebugFormat("apply ex: {0}", Log(hrs));
                return hrs;
            }
            else // в группе нет исключающих блоков
            {
                return beforeExclude.SelectMany(x => x.Value);
            }
        }

        protected Dictionary<IHrsHolder, IEnumerable<HealthRecord>> GetAllAnyHrsInner(SearchOptions qb)
        {
            Contract.Requires(!qb.IsExcluding);

            // результаты из неисключающих блоков
            var childrenResults = (from q in qb.Children
                                   where !q.IsExcluding
                                   select GetResultInner(q)).ToList();

            switch (qb.SearchScope)
            {
                case SearchScope.HealthRecord:
                    return Intersect(childrenResults);

                // все записи в списка идут выше? если (nor (all, any))
                case SearchScope.Holder:
                    return InHolderScope(childrenResults, hr => hr.Holder);

                case SearchScope.Patient:
                    return InHolderScope(childrenResults, hr => hr.GetPatient());

                default:
                    throw new NotImplementedException();
            }
        }

        protected IEnumerable<HealthRecord> ApplyExcludingInner(SearchOptions qb, bool anyNormal, Dictionary<IHrsHolder, IEnumerable<HealthRecord>> beforeExclude)
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

            if (!anyNormal) // only ex blocks
            {
                var exQbHrsList = ex.Select(x => x.JustNoHrs).ToList();

                if (logOn)
                    logger.DebugFormat("only ex. justNo {0}", Log(exQbHrsList.SelectMany(d => d)));

                switch (qb.SearchScope)
                {
                    case SearchScope.HealthRecord:
                        // пересечение записей с блока
                        // атрибуты проверяются в SearchJustNoWords
                        var result =
                             from holderHrs in Intersect(exQbHrsList)
                             from hr in holderHrs.Value
                             select hr;
                        return result;

                    // только исключающие блоки вычитают из всех записей области, где есть по одной записи без слов
                    case SearchScope.Holder:
                        beforeExclude = InHolderScope(exQbHrsList, (hr) => hr.Holder);
                        break;

                    case SearchScope.Patient:
                        beforeExclude = InHolderScope(exQbHrsList, (hr) => hr.GetPatient());
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
        ///  Все в одной области.
        ///  В каждом блоке должны найтись записи из области, тогда область попадает в результаты,
        ///  где есть все найденные записи из них.
        ///  И не должно быть ни одной исключающей записи.
        ///  </summary>
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

        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> InHolder(IList<IEnumerable<HealthRecord>> results)
        {
            return InHolderScope(results, (hr) => hr.Holder);
        }

        /// <summary>
        ///  Все в одной записи
        ///  В результатах пересечение записей каждого блока.
        ///  </summary>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> InHr(IList<IEnumerable<HealthRecord>> results)
        {
            return Intersect(results);
        }

        /// <summary>
        ///  Все в одной области пациента
        ///  В каждом блоке должны найтись записи из пациента, тогда пациент попадает в результаты,
        ///  где есть все найденные записи из него.
        ///  </summary>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> InPatient(IList<IEnumerable<HealthRecord>> results)
        {
            return InHolderScope(results, (hr) => hr.GetPatient());
        }
    }
}