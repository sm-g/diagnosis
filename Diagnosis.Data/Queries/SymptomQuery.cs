using Diagnosis.Data.Repositories;
using Diagnosis.Data.Specs;
using Diagnosis.Models;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Data.Queries
{
    public static class SymptomQuery
    {
        /// <summary>
        /// Возвращает симптомы, в которых есть хотя бы одно слово.
        /// </summary>
        public static Func<IEnumerable<Word>, IEnumerable<Symptom>> WithAnyWord(ISession session)
        {
            return (words) =>
            {
                using (var tr = session.BeginTransaction())
                {
                    var wordsIds = words.Select(w => w.Id).ToList();

                    Symptom symptom = null;
                    Word word = null;
                    SymptomWords sw = null;
                    return session.QueryOver<Symptom>(() => symptom)
                       .JoinAlias(() => symptom.Words, () => word)
                       .WhereRestrictionOn(() => word.Id).IsIn(wordsIds)
                       .TransformUsing(Transformers.DistinctRootEntity)
                       .List();
                }
            };
        }
        /// <summary>
        /// Возвращает симптом со словами.
        /// </summary>
        public static Func<IEnumerable<Word>, Symptom> ByWords(ISession session)
        {
            return (words) =>
            {
                var comparator = new CompareWord();
                var symptoms = WithAnyWord(session).Invoke(words);

                return symptoms.FirstOrDefault(
                 s => s.Words.OrderBy(w => w, comparator).SequenceEqual(
                        words.OrderBy(w => w, comparator)));
            };
        }
    }
}