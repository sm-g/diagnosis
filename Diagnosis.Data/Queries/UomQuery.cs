using Diagnosis.Common;
using Diagnosis.Models;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Data.Queries
{
    public static class UomQuery
    {
        /// <summary>
        /// Возвращает все единицы измерения, чьи обозначения или названия содержат строку.
        /// </summary>
        public static Func<string, IEnumerable<Uom>> Contains(ISession session)
        {
            return (str) =>
            {
                using (var tr = session.BeginTransaction())
                {
                    var q = session.QueryOver<Uom>();

                    if (!str.IsNullOrEmpty())
                    {
                        var disjunction = new Disjunction();
                        disjunction.Add(Restrictions.On<Uom>(w => w.Abbr).IsLike(str, MatchMode.Anywhere)); // do not work with sqlite
                        disjunction.Add(Restrictions.On<Uom>(w => w.Description).IsInsensitiveLike(str, MatchMode.Anywhere));

                        q = q.Where(disjunction);
                    }
                    return q.List()
                            .OrderBy(s => s.Type.Ord)
                            .ThenBy(s => s.Factor)
                            .ThenBy(s => s.Abbr)
                            .ToList();
                }
            };
        }
    }
}