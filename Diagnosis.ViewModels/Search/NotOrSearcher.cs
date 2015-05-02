using Diagnosis.Data.Queries;
using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.ViewModels.Search
{
    /// <summary>
    /// NotAny x = All excluding x
    /// </summary>
    internal class NotOrSearcher : OrSearcher
    {
        public NotOrSearcher(ISession session)
            : base(session)
        {
        }

        protected override IEnumerable<HealthRecord> GetResultForGroup(SearchOptions qb)
        {
            var or = base.GetResultForGroup(qb);
            var allHrs = allHrsCache ?? (allHrsCache = HealthRecordQuery.All(session)()); // минимизировать блоки Nor/выбор всех записей
            switch (qb.SearchScope)
            {
                case SearchScope.HealthRecord:
                    return allHrs.Except(or);

                case SearchScope.Holder:
                    return NotAnyInHolderScope(or, allHrs, h => h.Holder);

                case SearchScope.Patient:
                    return NotAnyInHolderScope(or, allHrs, h => h.GetPatient());

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        ///  Все записи списков, у кот. нет ни одной записи из Or
        ///  </summary>
        private static IEnumerable<HealthRecord> NotAnyInHolderScope(
                   IEnumerable<HealthRecord> orResults,
                   IEnumerable<HealthRecord> allHrs,
                   Func<HealthRecord, IHrsHolder> getScope)
        {
            var orHolders = orResults.Select(x => getScope(x)).ToList(); // с повторами
            var hrs = allHrs
                .Select(hr => getScope(hr))
                .Where(scope => !orHolders.Contains(scope))
                .SelectMany(scope => scope.HealthRecords);
            return hrs.Distinct();
        }
    }
}