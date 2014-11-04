using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diag = Diagnosis.Models.Diagnosis;

namespace Diagnosis.Data.Queries
{
    public class DiagnosisQuery
    {

        /// <summary>
        /// Возвращает все диагнозы - детей родителя, у которых заголовок содержит строку или код начинается на строку.
        /// </summary>
        public static Func<Diag, string, IEnumerable<Diag>> ChildrenStartingWith(ISession session)
        {
            return (parent, str) =>
            {
                using (var tr = session.BeginTransaction())
                {
                    var q = session.QueryOver<Diag>();

                    var disjunction = new Disjunction();
                    disjunction.Add(Restrictions.On<Diag>(d => d.Title).IsInsensitiveLike(str, MatchMode.Anywhere));
                    disjunction.Add(Restrictions.On<Diag>(d => d.Code).IsInsensitiveLike(str, MatchMode.Start));

                    return q.Where(disjunction).Where(d => d.Parent == parent).List();
                }
            };
        }
    }
}
