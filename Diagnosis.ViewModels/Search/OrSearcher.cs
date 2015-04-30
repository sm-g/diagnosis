using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;


namespace Diagnosis.ViewModels.Search
{

    internal class OrSearcher : Diagnosis.ViewModels.Searcher
    {
        public OrSearcher(ISession session)
            : base(session)
        {

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
                    return AnyInHr(childrenResults);


                case SearchScope.Holder:
                    return AnyInHolderScope(childrenResults, (hr) => hr.Holder);


                case SearchScope.Patient:
                    return AnyInHolderScope(childrenResults, (hr) => hr.GetPatient());


                default:
                    throw new NotImplementedException();
            }
        }

        protected override IEnumerable<HealthRecord> GetResultForGroup(SearchOptions qb)
        {
            var beforeExclude = GetAllAnyHrsInner(qb);

            // если в группе только исключающие блоки, в результате
            // все записи, которые хотели найти (без слов, в категории)
            // beforeExclude как будто все записи


            // не нужен холдер
            var beforeHrs = beforeExclude.SelectMany(x => x.Value);

            var justNoHrs = (from q in qb.Children
                             where q.IsExcluding
                             from hr in HrSearcher.SearchJustNoWords(session, q)
                             select hr).ToList();

            // к записям, полученным из результатов неисключающих блоков +
            // те, где просто нет исключенных слов

            // проходят списки где хоть одна запись без исключенных

            if (logOn)
                logger.DebugFormat("any. before\n {0}\n+ justNoHrs\n {1}", Log(beforeHrs), Log(justNoHrs));
            Console.WriteLine("any. before\n {0}\n+ justNoHrs\n {1}", Log(beforeHrs), Log(justNoHrs));

            var hrs = beforeHrs.Union(justNoHrs).Distinct();

            return hrs;
        }

        /// <summary>
        ///  Любое в том же списке
        ///  Список попадет в результаты, если нашлась хотя бы одна запись из списка в любом блоке.
        ///  То есть все записи проходят в результаты.
        ///  (или в списке нет записей-исключений)
        ///  </summary>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> AnyInHolder(IList<IEnumerable<HealthRecord>> results)
        {
            return AnyInHolderScope(results, (hr) => hr.Holder);
        }

        /// <summary>
        ///  Любое в области
        ///  Область попадет в результаты, если нашлась хотя бы одна запись из нее в любом блоке.
        ///  То есть все записи проходят в результаты.
        ///  </summary>
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
        ///  Любое в одной записи
        ///  В результатах все записи полученные из каждого блока.
        ///  </summary>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> AnyInHr(IList<IEnumerable<HealthRecord>> results)
        {
            return (from hrs in results
                    from hr in hrs
                    group hr by hr.Holder).ToDictionary(x => x.Key, x => x.Distinct());
        }

        /// <summary>
        ///  Любое в области пациента
        ///  Пациент попадет в результаты, если нашлась хотя бы одна запись из него в любом блоке.
        ///  </summary>
        private static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> AnyInPatient(IList<IEnumerable<HealthRecord>> results)
        {
            return AnyInHolderScope(results, (hr) => hr.GetPatient());
        }
    }

}
