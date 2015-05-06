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
        private static ISession session;
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HrSearcher));

        public static IEnumerable<HealthRecord> Search(ISession session, SearchOptions options)
        {
            Contract.Requires(options != null);
            Contract.Requires(session != null);

            IEnumerable<HealthRecord> hrs = Enumerable.Empty<HealthRecord>();

            Func<HealthRecord, IEnumerable<Word>> hrW = hr => hr.Words;
            Func<HealthRecord, IEnumerable<Confindencable<Word>>> hrWordsC = hr => hr.GetCWords();
            var withAanC = HealthRecordQuery.WithAllAnyNotConfWordsMinAny(session);
            var withAan = HealthRecordQuery.WithAllAnyNotWordsMinAny(session);

            if (options.WithConf)
            {
                if (!options.MeasuresAny.Any() || options.CWordsAll.Any() || options.CWordsAny.Any())
                {
                    // нет измерений any / есть слова в all / any
                    hrs = withAanC(
                      options.CWordsAll,
                      options.CWordsAny,
                      options.CWordsNot,
                      options.MinAny - options.MeasuresAny.Count()); // хотя бы 3 из w1,w2,m1,m2 - достаточно 1 слова
                }

                // измерения фильтруем отдельно - сложная логика
                if (options.MeasuresAny.Any())
                {
                    // ищем со словами из измерений с учетом notwords
                    var hrsWithM = FilterWordsAllMeasureAny(options, withAanC, options.CWordsAll, options.CWordsNot, x => x.Word.AsConfidencable(), hrWordsC);
                    // hrs пуст если только измерения в all / any
                    hrs = hrs.Union(hrsWithM);

                    if (options.MinAny > 1 && options.CWordsAny.Any())
                        hrs = FilterMinAny(options, hrs, hrWordsC, options.CWordsAny);
                }
            }
            else
            {
                if (!options.MeasuresAny.Any() || options.CWordsAll.Any() || options.CWordsAny.Any())
                {
                    hrs = withAan(
                       options.WordsAll,
                       options.WordsAny,
                       options.WordsNot,
                       options.MinAny - options.MeasuresAny.Count());
                }

                if (options.MeasuresAny.Any())
                {
                    var hrsWithM = FilterWordsAllMeasureAny(options, withAan, options.WordsAll, options.WordsNot, x => x.Word, hrW);
                    hrs = hrs.Union(hrsWithM);

                    if (options.MinAny > 1 && options.WordsAny.Any())
                        hrs = FilterMinAny(options, hrs, hrW, options.WordsAny);
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

        /// <summary>
        /// Любое из измерений ищем со словами из измерений с учетом notwords.
        ///
        /// Затем фильтр по WordsAll и MeasuresAny.
        /// </summary>
        private static IEnumerable<HealthRecord> FilterWordsAllMeasureAny<T>(SearchOptions options,
            Func<IEnumerable<T>, IEnumerable<T>, IEnumerable<T>, int, IEnumerable<HealthRecord>> query,
            IEnumerable<T> all,
            IEnumerable<T> not,
             Func<Measure, T> measure,
             Func<HealthRecord, IEnumerable<T>> hrWords
            )
        {
            var hrsWithM = query(
                        Enumerable.Empty<T>(),
                        options.MeasuresAny.Select(x => measure(x)),
                        not,
                        options.MinAny - options.CWordsAny.Count()
                        );

            hrsWithM = hrsWithM.Where(x =>
               all.IsSubmultisetOf(hrWords(x)) &&
               options.MeasuresAny.Any(op => x.Measures.Any(m => op.ResultFor(m)))
               );
            return hrsWithM;
        }

        /// <summary>
        /// Проверяем, набрала ли запись общее кол-во элементов из Any
        /// </summary>
        private static IEnumerable<HealthRecord> FilterMinAny<T>(SearchOptions options,
            IEnumerable<HealthRecord> hrs,
            Func<HealthRecord, IEnumerable<T>> hrSelector,
            IEnumerable<T> any)
        {
            hrs = from hr in hrs
                  let ws = hrSelector(hr).Intersect(any).Count()
                  let ms = options.MeasuresAny.Where(op => hr.Measures.Any(m => op.ResultFor(m))).Count()
                  where ms + ws >= options.MinAny
                  select hr;
            return hrs;
        }

        public static IEnumerable<HealthRecord> SearchJustNoWords(ISession session, SearchOptions options)
        {
            Contract.Requires(options != null);
            Contract.Requires(session != null);

            IEnumerable<HealthRecord> hrs;

            if (options.WithConf)
            {
                hrs = HealthRecordQuery.WithoutAnyConfWord(session)(
                    options.CWordsNot);
            }
            else
            {
                hrs = HealthRecordQuery.WithoutAnyWord(session)(
                    options.WordsNot);
            }
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