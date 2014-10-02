using Diagnosis.Data.Repositories;
using Diagnosis.Data.Specs;
using Diagnosis.Models;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using Diagnosis.Core;
using System.Linq;

namespace Diagnosis.Data.Queries
{
    public static class HealthRecordQuery
    {
        /// <summary>
        /// Возвращает записи с любым из слов в симптоме.
        /// </summary>
        public static Func<IEnumerable<Word>, IEnumerable<HealthRecord>> WithAnyWord(ISession session)
        {
            return (words) =>
            {
                using (var tr = session.BeginTransaction())
                {
                    var wordsIds = words.Select(w => w.Id).ToList();
                    Word word = null;
                    HealthRecord hr = null;
                    HrItem item = null;

                    return session.QueryOver<HealthRecord>(() => hr)
                        .JoinAlias(() => hr.HrItems, () => item)
                        .WhereRestrictionOn(() => item.Word).IsNotNull
                        .JoinAlias(() => item.Word, () => word)
                        .WhereRestrictionOn(() => word.Id).IsIn(wordsIds)
                        .TransformUsing(Transformers.DistinctRootEntity)
                        .List();
                }
            };
        }
        /// <summary>
        /// Возвращает записи со всеми словами в симптоме, могут быть и другие.
        /// </summary>
        public static Func<IEnumerable<Word>, IEnumerable<HealthRecord>> WithAllWords(ISession session)
        {
            return (words) =>
            {
                var withAny = WithAnyWord(session)(words);

                return withAny.Where(hr => words.IsSubsetOf(hr.Words));
            };
        }

        /// <summary>
        /// Возвращает записи с указанными словами в симптоме. То есть записи с симптомом.
        /// </summary>
        //public static Func<IEnumerable<Word>, IEnumerable<HealthRecord>> WithOnlyWords(ISession session)
        //{
        //    return (words) =>
        //    {
        //        using (var tr = session.BeginTransaction())
        //        {

        //        }
        //    };
        //}
    }
}