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
        public static Func<Doctor, Vocabulary> Custom(ISession session)
        {
            return (Doctor d) =>
            {
                using (var tr = session.BeginTransaction())
                {
                    var voc = session.Query<Vocabulary>()
                        .Where(x => x.Doctor == d)
                        .FirstOrDefault() ?? new Vocabulary(Vocabulary.CustomTitle, d);

                    return voc;
                }
            };
        }
    }
}