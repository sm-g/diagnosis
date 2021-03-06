﻿using Diagnosis.Models;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Data.Queries
{
    public static class CategoryQuery
    {
        /// <summary>
        /// Возвращает все категории, которые начинаются на строку.
        /// </summary>
        public static Func<string, IEnumerable<HrCategory>> StartingWith(ISession session)
        {
            return (str) =>
            {
                using (var tr = session.BeginTransaction())
                {
                    return session.QueryOver<HrCategory>()
                        .WhereRestrictionOn(w => w.Title)
                        .IsInsensitiveLike(str, MatchMode.Start)
                        .List();
                }
            };
        }

        /// <summary>
        /// Возвращает категорию по заголовку.
        /// </summary>
        public static Func<string, HrCategory> ByTitle(ISession session)
        {
            return (str) =>
            {
                using (var tr = session.BeginTransaction())
                {
                    return session.QueryOver<HrCategory>()
                        .WhereRestrictionOn(w => w.Title)
                        .IsInsensitiveLike(str, MatchMode.Exact)
                        .SingleOrDefault();
                }
            };
        }
        /// <summary>
        /// Возвращает категории по заголовку.
        /// </summary>
        public static Func<IEnumerable<string>, IEnumerable<HrCategory>> ByTitles(ISession session)
        {
            return (strs) =>
            {
                using (var tr = session.BeginTransaction())
                {
                    var upper = strs.Select(x => x.ToUpperInvariant()).ToArray();
                    return session.QueryOver<HrCategory>()
                        .Where(Restrictions.In(Projections.SqlFunction(
                                "upper", NHibernateUtil.String,
                                Projections.Property<HrCategory>(x => x.Title)),
                            upper))
                        .List<HrCategory>();
                }
            };
        }
    }
}