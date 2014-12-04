using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diagnosis.Common;
using NHibernate.Criterion;

namespace Diagnosis.Data.Queries
{
    public static class PatientQuery
    {
        /// <summary>
        /// Возвращает всех пациентов, у которых имя, отчество или фамилия начинаются на строку.
        /// Пустая строка - все пациенты.
        /// </summary>
        public static Func<string, IEnumerable<Patient>> StartingWith(ISession session)
        {
            return (str) =>
            {
                using (var tr = session.BeginTransaction())
                {
                    var q = session.QueryOver<Patient>();

                    if (!str.IsNullOrEmpty())
                    {
                        var disjunction = new Disjunction();
                        disjunction.Add(Restrictions.On<Patient>(w => w.FirstName).IsInsensitiveLike(str, MatchMode.Start)); // do not work with sqlite
                        disjunction.Add(Restrictions.On<Patient>(w => w.MiddleName).IsInsensitiveLike(str, MatchMode.Start));
                        disjunction.Add(Restrictions.On<Patient>(w => w.LastName).IsInsensitiveLike(str, MatchMode.Start));

                        q = q.Where(disjunction);
                    }
                    return q.List().OrderBy(p => p.FullName, new EmptyStringsAreLast());
                }
            };
        }
    }
}
