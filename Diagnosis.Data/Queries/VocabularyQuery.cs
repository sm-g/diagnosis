using Diagnosis.Common;
using Diagnosis.Models;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Data.Queries
{
    public static class VocabularyQuery
    {
        /// <summary>
        /// Возвращает непользовательские словари.
        /// </summary>
        public static Func<IEnumerable<Vocabulary>> NonCustom(ISession session)
        {
            return () =>
            {
                using (var tr = session.BeginTransaction())
                {
                    return session.Query<Vocabulary>()
                        .ToList()
                        .Where(x => !x.IsCustom)
                        .OrderBy(x => x.Title)
                        .ToList();
                }
            };
        }
        /// <summary>
        /// Возвращает словари c указанными Id.
        /// </summary>
        public static Func<IEnumerable<Guid>, IEnumerable<Vocabulary>> ByIds(ISession session)
        {
            return (ids) =>
            {
                using (var tr = session.BeginTransaction())
                {
                    return session.Query<Vocabulary>()
                        .Where(v => ids.Contains(v.Id))
                        .ToList();
                }
            };
        }
    }
}