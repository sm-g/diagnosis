using NHibernate;
using System;
using System.Linq;

namespace Diagnosis.Data.Sync
{
    public static class Helpers
    {
        public static void FakeUpdate(this ISession s, Type type, Guid id)
        {
            var table = Names.GetTblByType(type);
            s.CreateSQLQuery(string.Format("UPDATE {0} SET Id = Id WHERE Id = '{1}'", table, id)).ExecuteUpdate();
        }
    }
}