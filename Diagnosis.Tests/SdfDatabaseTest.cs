using Diagnosis.Common;
using Diagnosis.Common.Types;
using Diagnosis.Data;
using Diagnosis.Data.NHibernate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using System;
using System.IO;

namespace Diagnosis.Tests
{
    [TestClass]
    public abstract class SdfDatabaseTest : DbTest
    {
        protected static readonly ConnectionInfo serverCon = new ConnectionInfo("Data Source=" + serverSdf, Constants.SqlCeProvider);
        protected static readonly ConnectionInfo clientCon = new ConnectionInfo("Data Source=" + clientSdf, Constants.SqlCeProvider);
        protected ISession clSession;
        protected ISession sSession;
        private const string serverSdf = "server.sdf";
        private const string clientSdf = "client.sdf";
        private static ISessionFactory clFactory;
        private static ISessionFactory sFactory;
        private static Configuration sCfg;
        private static Configuration clCfg;

        static SdfDatabaseTest()
        {
            Constants.IsClient = true;

            SqlHelper.CreateSqlCeByPath(clientSdf);
            File.Copy("db.sdf", serverSdf, true);

            clCfg = NHibernateHelper.CreateConfiguration(clientCon, NHibernateHelper.CreateMapping(), true);
            sCfg = NHibernateHelper.CreateConfiguration(serverCon, NHibernateHelper.CreateMapping(), true);
            clFactory = clCfg.BuildSessionFactory();
            sFactory = sCfg.BuildSessionFactory();
        }

        [TestInitialize]
        public void InMemoryDatabaseTestInit()
        {
            SqlHelper.CreateSqlCeByPath(clientSdf);
            File.Copy("db.sdf", serverSdf, true);

            //new SchemaExport(clCfg).Execute(false, true, false);
            clSession = clFactory.OpenSession();
            InMemoryHelper.FillData(clCfg, clSession);
            sSession = sFactory.OpenSession();

            session = clSession;
        }

        [TestCleanup]
        public void InMemoryDatabaseTestCleanup()
        {
            if (sSession != null)
                sSession.Dispose();
            if (clSession != null)
                clSession.Dispose();
            File.Delete(clientSdf);
            File.Delete(serverSdf);
        }
    }
}