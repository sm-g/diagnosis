using Diagnosis.Data.Versions;
using Diagnosis.Models;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Tool.hbm2ddl;
using PasswordHash;
using System.IO;
using System.Reflection;

namespace Diagnosis.Data.NHibernate
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

            var assembly = Assembly.GetAssembly(typeof(InMemoryHelper));
            var resourceName = "Diagnosis.Data.Versions.Sql.inmem_sqlite.sql";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var s = new StreamReader(stream))
            {
                var sql = s.ReadToEnd();

                using (ITransaction tx = session.BeginTransaction())
                {
                    session.CreateSQLQuery(sql).ExecuteUpdate();
                    session.CreateSQLQuery(string.Format("INSERT INTO {0} ([Id], [HashAndSalt]) Values ('{1}','{2}')",
                        Names.PassportTbl,
                        Admin.DefaultId,
                        PasswordHashManager.CreateHash(Admin.DefaultPassword + "4"))).ExecuteUpdate();
                    tx.Commit();
                }
            }
        }
    }
}