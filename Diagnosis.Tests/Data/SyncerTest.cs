using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Sync;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using NHibernate.Linq;
using System.Threading.Tasks;
using Diagnosis.Data.NHibernate;
using System.Collections.Generic;
using System;
using System.Data;

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

        [TestMethod]
        public async Task SendVocScopeFromServerDoNotAddVoc()
        {
            InMemoryHelper.FillData(sCfg, sSession, true);

            var sWtCount = sSession.Query<WordTemplate>().Count();
            var cVocCount = clSession.Query<Vocabulary>().Count();

            var s = new Syncer(serverCon.ConnectionString, clientCon.ConnectionString, serverCon.ProviderName);

            // не синхронизируем новые словари
            var installedVocsIds = sSession.Query<Vocabulary>().Select(x => x.Id).ToList().Cast<object>();
            await s.SendFrom(Side.Server, Scope.Voc.ToEnumerable(), installedVocsIds);

            Assert.AreEqual(cVocCount, clSession.Query<Vocabulary>().Count());
            Assert.AreEqual(2, clSession.Query<Speciality>().Count());
            Assert.AreEqual(0, clSession.Query<WordTemplate>().Count());
        }

    }
}