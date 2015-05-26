using Diagnosis.Models;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Data.Queries
{
    public class IcdQuery
    {
        /// <summary>
        /// Возвращает все диагнозы, у которых заголовок содержит строку или код начинается на строку.
        /// </summary>
        public static Func<string, IEnumerable<IcdDisease>> StartingWith(ISession session)
        {
            return (str) =>
            {
                using (var tr = session.BeginTransaction())
                {
                    var q = session.QueryOver<IcdDisease>();

                    var disjunction = new Disjunction();
                    disjunction.Add(Restrictions.On<IcdDisease>(d => d.Title).IsInsensitiveLike(str, MatchMode.Anywhere));
                    disjunction.Add(Restrictions.On<IcdDisease>(d => d.Code).IsInsensitiveLike(str, MatchMode.Start));

                    return q.Where(disjunction).List();
                }
            };
        }

        /// <summary>
        /// Возвращает все блоки, у которых заголовок содержит строку или код начинается на строку.
        /// </summary>
        public static Func<string, IEnumerable<IcdBlock>> BlockStartingWith(ISession session)
        {
            return (str) =>
            {
                using (var tr = session.BeginTransaction())
                {
                    var q = session.QueryOver<IcdBlock>();

                    var disjunction = new Disjunction();
                    disjunction.Add(Restrictions.On<IcdBlock>(d => d.Title).IsInsensitiveLike(str, MatchMode.Anywhere));
                    disjunction.Add(Restrictions.On<IcdBlock>(d => d.Code).IsInsensitiveLike(str, MatchMode.Start));

                    return q.Where(disjunction).List();
                }
            };
        }
    }
}