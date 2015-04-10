using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Sync;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using NHibernate.Linq;
using System.Threading.Tasks;
using Diagnosis.Data.NHibernate;

namespace Diagnosis.Tests.Data
{
    [TestClass]
    public class SyncerTest : SdfDatabaseTest
    {
        static string log;
        [TestMethod]
        public async Task SendFromServer()
        {
            InMemoryHelper.FillData(sCfg, sSession, true);
            var sCatCount = sSession.Query<HrCategory>().Count();

            var s = new Syncer(serverCon.ConnectionString, clientCon.ConnectionString, serverCon.ProviderName);
            Syncer.MessagePosted += Syncer_MessagePosted;
            await s.SendFrom(Side.Server, new[] { Scope.Icd, Scope.Reference });
            Syncer.MessagePosted -= Syncer_MessagePosted;

            // после загрузки проверяем справочные сущности на совпадение
            var checker = new AfterSyncChecker(clSession);
            var scopesToDeprovision = checker.CheckReferenceEntitiesAfterDownload(s.AddedOnServerIdsPerType);

            Assert.AreEqual(sCatCount, clSession.Query<HrCategory>().Count());
        }

        void Syncer_MessagePosted(object sender, StringEventArgs e)
        {
            System.Console.WriteLine(e.str);
            log += e.str + '\n';
        }

    }
}