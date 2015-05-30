using Diagnosis.Common;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Data.Search
{
    public class HrSearcher
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HrSearcher));

        public static IEnumerable<HealthRecord> Search(ISession session, SearchOptions options)
        {
            Contract.Requires(options != null);
            Contract.Requires(session != null);

            IEnumerable<HealthRecord> hrs = Enumerable.Empty<HealthRecord>();

            Func<HealthRecord, IEnumerable<Word>> hrW = hr => hr.Words;
            Func<HealthRecord, IEnumerable<Confindencable<Word>>> hrWordsC = hr => hr.GetCWords();
            var withAanC = HealthRecordQuery.WithAllAnyNotConfWords(session);
            var withAan = HealthRecordQuery.WithAllAnyNotWords(session);

            if (options.WithConf)
            {
                // проверка, чтобы не добавлять лишнее, когда есть измерения any, но нет слов all / any
                if (!options.MeasuresAny.Any() || options.CWordsAll.Any() || options.CWordsAny.Any())
                {
                    // нет измерений any / есть слова в all / any
                    // могут быть только слова в not
                    // или только измерение в all
                    hrs = withAanC(
                      options.CWordsAll,
                      options.CWordsAny,
                      options.CWordsNot);
                }

                if (options.MeasuresAny.Any())
                {
                    var hrsWithM = FilterWordsAllMeasureAny(options,
                        withAanC, options.CWordsAll, options.CWordsNot,
                        x => x.Word.AsConfidencable(), hrWordsC);

                    Contract.Assume(hrs.Any() || !(options.CWordsAll.Any() || options.CWordsAny.Any())); // !hrs.Any() → only measures in all/any

                    hrs = hrs.Union(hrsWithM);
                }

                hrs = FilterMinAny(options, hrs, hrWordsC, options.CWordsAny);
            }
            else
            {
                if (!options.MeasuresAny.Any() || options.CWordsAll.Any() || options.CWordsAny.Any())
                {
                    hrs = withAan(
                       options.WordsAll,
                       options.WordsAny,
                       options.WordsNot);
                }

                if (options.MeasuresAny.Any())
                {
                    var hrsWithM = FilterWordsAllMeasureAny(options,
                        withAan, options.WordsAll, options.WordsNot,
                        x => x.Word, hrW);

                    hrs = hrs.Union(hrsWithM);
                }

                hrs = FilterMinAny(options, hrs, hrW, options.WordsAny);
            }

            Contract.Assume(hrs != null);

            // все измерения - просто фильтр
            if (options.MeasuresAll.Any())
            {
                // для каждого измерения опций в записи находится подходящее
                // одно измерение повторно используется для сравнения. t>2 и t>0 - достаточно t>2 в записи
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
        private static IEnumerable<HealthRecord> FilterWordsAllMeasureAny<T>(SearchOptions options, Func<IEnumerable<T>, IEnumerable<T>, IEnumerable<T>, IEnumerable<HealthRecord>> query, IEnumerable<T> all, IEnumerable<T> not, Func<Measure, T> measureWord, Func<HealthRecord, IEnumerable<T>> hrWords)
        {
            var hrsWithM = query(
                        Enumerable.Empty<T>(),
                        options.MeasuresAny.Select(x => measureWord(x)),
                        not
                        );

            hrsWithM = hrsWithM.Where(x =>
               all.IsSubsetOf(hrWords(x)) &&
               options.MeasuresAny.Any(op => x.Measures.Any(m => op.ResultFor(m))));
            return hrsWithM;
        }

        /// <summary>
        /// Проверяем, есть ли в записи общее кол-во элементов из Any
        /// </summary>
        private static IEnumerable<HealthRecord> FilterMinAny<T>(SearchOptions options,
            IEnumerable<HealthRecord> hrs,
            Func<HealthRecord, IEnumerable<T>> hrWords,
            IEnumerable<T> any)
        {
            if (options.MinAny <= 1) return hrs;

            return from hr in hrs
                   let hrWs = hrWords(hr).ToList()
                   let ws = any.Where(w => hrWs.Contains(w)).Count() // сколько слов из any есть в записи
                   let ms = options.MeasuresAny.Where(op => hr.Measures.Any(m => op.ResultFor(m))).Count() // подходящих измерений из any
                   where ms + ws >= options.MinAny
                   select hr;
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