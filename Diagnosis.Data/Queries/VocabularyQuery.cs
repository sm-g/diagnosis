using Diagnosis.Common;
using Diagnosis.Models;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Linq;

namespace Diagnosis.Data.Queries
{
    public static class VocabularyQuery
    {
        /// <summary>
        /// Возвращает пользовательский словарь.
        /// </summary>
        public static Func<Vocabulary> Custom(ISession session)
        {
            return () =>
            {
                using (var tr = session.BeginTransaction())
                {
                    var voc = session.Query<Vocabulary>()
                        .Where(x => x.Title == Vocabulary.CustomTitle)
                        .FirstOrDefault() ?? new Vocabulary(Vocabulary.CustomTitle);

                    return voc;
                }
            };
        }
    }
}