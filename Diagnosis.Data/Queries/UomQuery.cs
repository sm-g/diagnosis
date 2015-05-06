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
                        disjunction.Add(Restrictions.On<Uom>(w => w.Abbr).IsInsensitiveLike(str, MatchMode.Anywhere)); // do not work with sqlite
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

        public static Func<string, string, string, Uom> ByAbbrDescrAndTypeName(ISession session)
        {
            return (abbr, descr, typename) =>
            {
                using (var tr = session.BeginTransaction())
                {
                    if (abbr.IsNullOrEmpty() || descr.IsNullOrEmpty() || typename.IsNullOrEmpty())
                        return null;
                    UomType type = null;
                    var q = session.QueryOver<Uom>()
                        .AndRestrictionOn(w => w.Abbr).IsInsensitiveLike(abbr, MatchMode.Exact)
                        .AndRestrictionOn(w => w.Description).IsInsensitiveLike(descr, MatchMode.Exact)
                        .JoinAlias(x => x.Type, () => type)
                        .AndRestrictionOn(() => type.Title).IsInsensitiveLike(typename, MatchMode.Exact);
                    return q.SingleOrDefault();
                }
            };
        }
    }
}