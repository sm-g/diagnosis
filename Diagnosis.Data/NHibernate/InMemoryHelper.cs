using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Tool.hbm2ddl;

namespace Diagnosis.Data
{
    internal class InMemoryHelper
    {
        public static void Configure(Configuration cfg, bool showSql)
        {
            cfg.SetProperty(Environment.ReleaseConnections, "on_close")
               .SetProperty(Environment.Dialect, typeof(SQLiteDialect).AssemblyQualifiedName)
               .SetProperty(Environment.ConnectionDriver, typeof(SQLite20Driver).AssemblyQualifiedName)
               .SetProperty(Environment.ConnectionString, "data source=:memory:;BinaryGuid=False")
               .SetProperty(Environment.ShowSql, showSql ? "true" : "false");
        }

        public static void FillData(Configuration cfg, dynamic session)
        {
            new SchemaExport(cfg).Execute(false, true, false, session.Connection, null);

            using (var s = System.IO.File.OpenText(@"..\..\..\Database\inmem_sqlite.sql"))
            {
                var sql = s.ReadToEnd();

                using (ITransaction tx = session.BeginTransaction())
                {
                    session.CreateSQLQuery(sql).ExecuteUpdate();
                    tx.Commit();
                }
            }
        }
    }
}