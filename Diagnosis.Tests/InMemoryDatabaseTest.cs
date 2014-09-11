using Diagnosis.Data;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Tool.hbm2ddl;

namespace Tests
{
    public class InMemoryDatabaseTest : System.IDisposable
    {
        private static Configuration Configuration;
        private static ISessionFactory SessionFactory;
        protected ISession session;

        public InMemoryDatabaseTest()
        {
            if (Configuration == null)
            {
                Configuration = new Configuration()
                    .SetProperty(Environment.ReleaseConnections, "on_close")
                    .SetProperty(Environment.Dialect, typeof(SQLiteDialect).AssemblyQualifiedName)
                    .SetProperty(Environment.ConnectionDriver, typeof(SQLite20Driver).AssemblyQualifiedName)
                    .SetProperty(Environment.ConnectionString, "data source=:memory:")
                    .SetProperty(Environment.CollectionTypeFactoryClass, typeof(Net4CollectionTypeFactory).AssemblyQualifiedName);

                Configuration
                    .AddMapping(NHibernateHelper.CreateMapping());

                SessionFactory = Configuration.BuildSessionFactory();
            }

            session = SessionFactory.OpenSession();

            new SchemaExport(Configuration).Execute(true, true, false, session.Connection, System.Console.Out);
        }

        public void Dispose()
        {
            session.Dispose();
        }
    }
}