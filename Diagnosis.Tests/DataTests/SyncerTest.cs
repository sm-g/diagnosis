using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Sync;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using NHibernate.Linq;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class SyncerTest : SdfDatabaseTest
    {
        [TestMethod]
        public async Task SendFromServer()
        {
            var sCount = sSession.Query<HrCategory>().Count();

            Syncer s = new Syncer(serverCon.ConnectionString, clientCon.ConnectionString, serverCon.ProviderName);
            await s.SendFrom(Side.Server, Scope.Reference.ToEnumerable());

            Assert.AreEqual(sCount, clSession.Query<HrCategory>().Count());
        }
    }
}