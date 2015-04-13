using Diagnosis.Models;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Tool.hbm2ddl;
using PasswordHash;
using System.IO;
using System.Reflection;

namespace Diagnosis.Data.NHibernate
{
    internal class InMemoryHelper
    {
        public static void FillData(Configuration cfg, dynamic session, bool server = false)
        {
            new SchemaExport(cfg).Execute(false, true, false, session.Connection, null);

            var assembly = Assembly.GetAssembly(typeof(InMemoryHelper));
            var isSqlite = cfg.GetProperty(Environment.Dialect) == typeof(SQLiteDialect).AssemblyQualifiedName;
            var resourceName = isSqlite
                 ? "Diagnosis.Data.Versions.Sql.inmem_sqlite.sql"
                 : server ? "Diagnosis.Data.Versions.Sql.inmem_sqlce_server.sql"
                          : "Diagnosis.Data.Versions.Sql.inmem_sqlce.sql";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var s = new StreamReader(stream))
            {
                var sql = s.ReadToEnd();

                using (ITransaction tx = session.BeginTransaction())
                {
                    if (!isSqlite) // sqlce
                        foreach (var item in sql.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries))
                        {
                            session.CreateSQLQuery(item).ExecuteUpdate();
                        }
                    else
                        session.CreateSQLQuery(sql).ExecuteUpdate();

                    session.CreateSQLQuery(string.Format("INSERT INTO {0} ([Id], [HashAndSalt]) Values ('{1}','{2}')",
                        Names.Passport,
                        Admin.DefaultId,
                        PasswordHashManager.CreateHash(Admin.DefaultPassword + "4"))).ExecuteUpdate();
                    tx.Commit();
                }
            }
        }
    }
}