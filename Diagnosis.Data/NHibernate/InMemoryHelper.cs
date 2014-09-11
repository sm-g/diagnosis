using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Tool.hbm2ddl;
using System.IO;
namespace Diagnosis.Data
{
    internal class InMemoryHelper
    {
        public static void Configure(Configuration cfg)
        {
            cfg.SetProperty(Environment.ReleaseConnections, "on_close")
               .SetProperty(Environment.Dialect, typeof(SQLiteDialect).AssemblyQualifiedName)
               .SetProperty(Environment.ConnectionDriver, typeof(SQLite20Driver).AssemblyQualifiedName)
               .SetProperty(Environment.ConnectionString, "data source=:memory:");
        }

        public static void FillData(Configuration cfg, dynamic session)
        {
            new SchemaExport(cfg).Execute(false, true, false, session.Connection, null);
            using (var s = System.IO.File.OpenText(@"..\..\..\Database\inmem_sqlite.sql"))
            {
                var sql1 = s.ReadToEnd();

                foreach (var line in sql1.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.StartsWith("INSERT"))
                    {
                        session.CreateSQLQuery(line).ExecuteUpdate();
                    }
                }
            }

        }
    }
}