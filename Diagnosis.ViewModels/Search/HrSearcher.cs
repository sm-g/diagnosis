using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Diagnosis.Models;
using Diagnosis.Common;
using Diagnosis.Data.Queries;
using NHibernate;
using NHibernate.Linq;
using Diagnosis.Common.Types;

namespace Diagnosis.ViewModels.Search
{
    public class HrSearcher
    {
        public IEnumerable<HealthRecord> Search(ISession session, HrSearchOptions options)
        {
            Contract.Requires(options != null);
            Contract.Requires(session != null);

            IEnumerable<HealthRecord> hrs;

            //if (options.WordsAll.Count() > 0)
            //    if (options.AllWords)
            //    {
            //        hrs = HealthRecordQuery.WithAllWords(session)(options.WordsAll, options.QueryScope);
            //    }
            //    else
            //    {
            //        hrs = HealthRecordQuery.WithAnyWord(session)(options.WordsAll);
            //    }
            //else
            //{
            //    hrs = session.Query<HealthRecord>();
            //}

            hrs = HealthRecordQuery.WithAllAnyNotWords(session)(
                options.WordsAll,
                options.WordsAny,
                options.WordsNot);

            // все измерения - просто фильтр
            if (options.MeasuresAll.Count() > 0)
            {
                hrs = hrs.Where(x =>
                   options.MeasuresAll.All(m => x.Measures.Contains(m, new ValueComparer(m.Operator))));
            }
            // любое из измерений - ищем со словами из измерений с учетом ограничений all, not, 
            // добавляем к записям
            if (options.MeasuresAny.Count() > 0)
            {
                var hrsWithM = HealthRecordQuery.WithAllAnyNotWords(session)(
                    Enumerable.Empty<Word>(),
                    options.MeasuresAny.Select(x => x.Word),
                    options.WordsNot);

                hrsWithM = hrsWithM.Where(x =>
                   options.WordsAll.IsSubsetOf(x.Words) &&
                   options.MeasuresAll.All(m => x.Measures.Contains(m, new ValueComparer(m.Operator))) &&

                   options.MeasuresAny.Any(m => x.Measures.Contains(m, new ValueComparer(m.Operator))));

                if (options.WordsAll.Any() || options.WordsAny.Any())
                    hrs = hrs.Union(hrsWithM);
                else // только измерения в all / any
                    hrs = hrsWithM;
            }

            if (options.Categories.Count() > 0)
            {
                hrs = hrs.Where(hr =>
                    options.Categories.Any(cat => cat.Equals(hr.Category)));
            }

            return hrs.ToList();
        }
    }
}
