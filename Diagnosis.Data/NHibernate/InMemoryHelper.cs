using Diagnosis.Models;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Tool.hbm2ddl;
using PasswordHash;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Diagnosis.Data.NHibernate
{
    internal class InMemoryHelper
    {
        public static void FillData(Configuration cfg, dynamic session, bool server = false)
        {
            var isSqlite = cfg.GetProperty(Environment.Dialect) == typeof(SQLiteDialect).AssemblyQualifiedName;

            using (ITransaction tx = session.BeginTransaction())
            {
                foreach (var item in GetScript(!isSqlite, server))
                {
                    session.CreateSQLQuery(item).ExecuteUpdate();
                }

                if (!server)
                    session.CreateSQLQuery(string.Format("INSERT INTO {0} ([Id], [HashAndSalt]) Values ('{1}','{2}')",
                        Names.Passport,
                        Admin.DefaultId,
                        PasswordHashManager.CreateHash(Admin.DefaultPassword + "4"))).ExecuteUpdate();
                tx.Commit();
            }
        }

        public static string[] GetScript(bool forSqlCe, bool forServer)
        {
            var assembly = Assembly.GetAssembly(typeof(InMemoryHelper));
            var resourceName = "Diagnosis.Data.Versions.Sql.inmem_sqlite.sql";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var s = new StreamReader(stream))
            {
                var sql = s.ReadToEnd();

                var x1 = sql.Split(';')
                        .Where(x => !string.IsNullOrWhiteSpace(x));

                if (forSqlCe)
                    return sql.Split(';')
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x.StartsWith("\r\n--SET IDENTITY_INSERT")
                            ? x.Remove(0, 4) // uncomment
                            : x)
                        .TakeWhile(x => !(forServer && x.Contains("-- CLIENT"))) // for server take sql before 
                        .ToArray();

                return new[] { sql };
            }
        }
    }
}