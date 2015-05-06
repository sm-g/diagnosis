using Diagnosis.Common;
using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Search
{
    public abstract class Searcher
    {
        protected static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Searcher));
        protected static bool logOn;
        protected static IEnumerable<HealthRecord> allHrsCache;
        protected ISession session;

        public Searcher(ISession session)
        {
            this.session = session;
        }

        private Searcher()
        {
        }

        /// <summary>
        /// Записи, возвращаемые корнем.
        /// </summary>
        public static IEnumerable<HealthRecord> GetResult(ISession session, SearchOptions qb)
        {
            Contract.Requires(qb.IsRoot);

            IEnumerable<HealthRecord> hrs;

            if (qb.IsGroup)
                hrs = Searcher.Create(session, qb).GetResultForGroup(qb);
            else
                hrs = HrSearcher.Search(session, qb);

            allHrsCache = null; // следующий поиск снова выбирает все записи, если надо
            return hrs;
        }

        public static Searcher Create(ISession session, SearchOptions qb)
        {
            Searcher searcher;
            switch (qb.GroupOperator)
            {
                case QueryGroupOperator.All:
                    searcher = new AndSearcher(session);
                    break;

                case QueryGroupOperator.Any:
                    searcher = new OrSearcher(session);
                    break;

                case QueryGroupOperator.NotAny:
                    searcher = new NotOrSearcher(session);
                    break;

                default:
                    throw new NotImplementedException();
            }
            return searcher;
        }

        /// <summary>
        /// Пересечение в каждом блоке.
        /// </summary>
        protected static Dictionary<IHrsHolder, IEnumerable<HealthRecord>> Intersect(IList<IEnumerable<HealthRecord>> results)
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

        /// <summary>
        /// Записи, возвращаемые блоком.
        /// </summary>
        protected IEnumerable<HealthRecord> GetResultInner(SearchOptions qb)
        {
            // по исключающим блокам не ищем
            Contract.Assume(qb.IsRoot || !qb.IsExcluding);
            logger.DebugFormat("{0}", qb);
            if (qb.IsGroup)
            {
                var searcher = Searcher.Create(session, qb);
                return searcher.GetResultForGroup(qb);
            }
            else
            {
                return HrSearcher.Search(session, qb);
            }
        }

        /// <summary>
        /// Записи, возвращаемые группой.
        /// </summary>
        protected abstract IEnumerable<HealthRecord> GetResultForGroup(SearchOptions qb);

        protected static string Log(IEnumerable<HealthRecord> hrs)
        {
            return string.Join<HealthRecord>("\n", hrs.ToArray());
        }
    }
}