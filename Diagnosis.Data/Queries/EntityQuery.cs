using Diagnosis.Common;
using Diagnosis.Models;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace Diagnosis.Data.Queries
{
    public static class EntityQuery<T> where T : IEntity
    {
        public static Func<IEnumerable<T>> All(ISession session)
        {
            return () =>
            {
                Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);
                using (var tr = session.BeginTransaction())
                {
                    return session.Query<T>().ToList();
                }
            };
        }

        public static Func<T> FirstOrDefault(ISession session)
        {
            return () =>
            {
                Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);
                using (var tr = session.BeginTransaction())
                {
                    return session.Query<T>().FirstOrDefault();
                }
            };
        }

        public static Func<Expression<Func<T, bool>>, IEnumerable<T>> Where(ISession session)
        {
            return (expr) =>
            {
                Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);
                using (var tr = session.BeginTransaction())
                {
                    return session.Query<T>()
                        .Where(expr)
                        .ToList();
                }
            };
        }

        public static Func<Expression<Func<T, bool>>, IEnumerable<object>, IEnumerable<T>> WhereAndIdNotIn(ISession session)
        {
            return (expr, ids) =>
            {
                Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);
                using (var tr = session.BeginTransaction())
                {
                    return session.Query<T>()
                        .Where(expr)
                        .Where(x => !ids.Contains(x.Id))
                        .ToList();
                }
            };
        }

        public static Func<bool> Any(ISession session)
        {
            return () =>
            {
                using (var tr = session.BeginTransaction())
                {
                    return session.Query<T>().Any();
                }
            };
        }
    }
}