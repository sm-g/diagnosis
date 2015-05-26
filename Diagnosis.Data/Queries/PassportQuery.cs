using Diagnosis.Models;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Data.Queries
{
    public class PassportQuery
    {
        /// <summary>
        /// Возвращает пасспорт по Id юзера.
        /// </summary>
        public static Func<Guid, Passport> WithId(ISession session)
        {
            return (guid) =>
            {
                using (var tr = session.BeginTransaction())
                {
                    return session.Query<Passport>()
                         .Where(u => u.Id == guid)
                         .FirstOrDefault();
                }
            };
        }

    }
}