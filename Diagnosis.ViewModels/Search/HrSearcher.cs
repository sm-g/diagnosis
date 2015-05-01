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
        private static bool logOn = false;

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

            if (options.MeasuresAny.Any())
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
                   options.WordsAll.IsSubmultisetOf(x.Words) &&
                   options.MeasuresAny.Any(op => x.Measures.Any(m => op.ResultFor(m)))
                   );

                if (options.WordsAll.Any() || options.WordsAny.Any())
                    hrs = hrs.Union(hrsWithM);
                else // только измерения в all / any
                    hrs = hrsWithM;

                if (options.MinAny > 1 && options.WordsAny.Any())
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
            if (options.MeasuresAll.Any())
            {
                // одно измерение повторно используется для сравнения
                hrs = hrs.Where(x =>
                   options.MeasuresAll.All(op => x.Measures.Any(m => op.ResultFor(m))));
            }

            if (options.Categories.Any())
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

            if (options.Categories.Any())
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

            if (options.WordsAll.Any())
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

            if (options.MeasuresAll.Any())
            {
                hrs = hrs.Where(x =>
                   options.MeasuresAll.All(op => x.Measures.Any(m => op.ResultFor(m))));
            }

            if (options.Categories.Any())
            {
                hrs = hrs.Where(hr =>
                    options.Categories.Any(cat => cat.Equals(hr.Category)));
            }

            return hrs.ToList();
        }
    }
}