using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diag = Diagnosis.Models.IcdDisease;

namespace Diagnosis.Data.Queries
{
    public class DiagnosisQuery
    {

        /// <summary>
        /// Возвращает все диагнозы, у которых заголовок содержит строку или код начинается на строку.
        /// </summary>
        public static Func<string, IEnumerable<Diag>> StartingWith(ISession session)
        {
            return (str) =>
            {
                using (var tr = session.BeginTransaction())
                {
                    var q = session.QueryOver<Diag>();

                    var disjunction = new Disjunction();
                    disjunction.Add(Restrictions.On<Diag>(d => d.Title).IsInsensitiveLike(str, MatchMode.Anywhere));
                    disjunction.Add(Restrictions.On<Diag>(d => d.Code).IsInsensitiveLike(str, MatchMode.Start));

                    return q.Where(disjunction).List();
                }
            };
        }
    }
}
