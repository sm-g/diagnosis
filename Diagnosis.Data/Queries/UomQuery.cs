using Diagnosis.Models;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;

namespace Diagnosis.Data.Queries
{
    public static class UomQuery
    {
        /// <summary>
        /// Возвращает все единицы измерения, чьи обозначения начинаются на строку.
        /// </summary>
        public static Func<string, IEnumerable<Uom>> StartingWith(ISession session)
        {
            return (str) =>
            {
                using (var tr = session.BeginTransaction())
                {
                    return session.QueryOver<Uom>().WhereRestrictionOn(w => w.Abbr).IsLike(str, MatchMode.Start).List();
                }
            };
        }
    }
}