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
                    var voc = session.Query<Vocabulary>()
                        .ToList()
                        .Where(x => !x.IsCustom)
                        .ToList();

                    return voc;
                }
            };
        }
    }
}