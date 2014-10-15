using Diagnosis.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Tool.hbm2ddl;

namespace Tests
{
    [TestClass]
    public abstract class InMemoryDatabaseTest
    {
        protected ISession session;

        public InMemoryDatabaseTest()
        {
            NHibernateHelper.InMemory = true;
            NHibernateHelper.ShowSql = true;
        }

        [TestInitialize]
        public void InMemoryDatabaseTestInit()
        {
            session = NHibernateHelper.OpenSession();
        }

        [TestCleanup]
        public void InMemoryDatabaseTestCleanup()
        {
            session.Dispose();
        }
    }
}