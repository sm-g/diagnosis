using Diagnosis.Common;
using Diagnosis.Common.Types;
using Diagnosis.Data.NHibernate;
using Diagnosis.Data.Sync;
using Diagnosis.Models;
using Diagnosis.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diagnosis.Data.Tests
{
    [TestClass]
    public class SyncerTest : SdfDatabaseTest
    {
        private Syncer s;

        [TestInitialize]
        public void Init()
        {
            InMemoryHelper.FillData(sCfg, sSession, true);
            s = new Syncer(serverCon.ConnectionString, clientCon.ConnectionString, serverCon.ProviderName);

            Poster.MessagePosted += Syncer_MessagePosted;
        }

        [TestCleanup]
        public void Cleanup()
        {
            Poster.MessagePosted -= Syncer_MessagePosted;
        }

        [TestMethod]
        public void DataConditions()
        {
            // есть спец. для 2 словаря
            var voc2 = sSession.Get<Vocabulary>(IntToGuid<Vocabulary>(2));
            Assert.IsTrue(voc2.Specialities.Any());
        }

        [TestMethod]
        public async Task SendFromServerToEmptyClient()
        {
            var scopes = new[] { Scope.Icd, Scope.Reference };
            var types = scopes.SelectMany(x => x.GetTypes());

            var entitiesCount = new Dictionary<Type, int>();
            object al = null;
            foreach (var type in types)
            {
                entitiesCount[type] = sSession.QueryOver(type.Name, () => al).RowCount();
            }

            await s.SendFrom(Side.Server, scopes);

            // после загрузки проверяем справочные сущности на совпадение
            var checker = new AfterSyncChecker(clSession);
            checker.CheckReferenceEntitiesAfterDownload(s.AddedOnServerIdsPerType);
            foreach (var type in types)
            {
                Assert.AreEqual(entitiesCount[type], clSession.QueryOver(type.Name, () => al).RowCount());
            }
        }

        [TestMethod]
        public async Task SendFromClient()
        {
            InMemoryHelper.FillData(clCfg, clSession, false);

            var scopes = Scopes.GetOrderedUploadScopes();
            var types = scopes.SelectMany(x => x.GetTypes());

            var entitiesCount = new Dictionary<Type, int>();
            object al = null;
            foreach (var type in types)
            {
                entitiesCount[type] = clSession.QueryOver(type.Name, () => al).RowCount();
            }

            await s.WithoutCustomVocsInDoc().SendFrom(Side.Client, scopes);

            foreach (var type in types)
            {
                Assert.AreEqual(entitiesCount[type], sSession.QueryOver(type.Name, () => al).RowCount());
            }
        }


        [TestMethod]
        public async Task SendFromServerToFilledClient()
        {
            InMemoryHelper.FillData(clCfg, clSession, false, alterIds: true);

            var scopes = new[] { Scope.Icd, Scope.Reference };
            var types = scopes.SelectMany(x => x.GetTypes());

            var entitiesCount = new Dictionary<Type, int>();
            object al = null;
            foreach (var type in types)
            {
                entitiesCount[type] = sSession.QueryOver(type.Name, () => al).RowCount();
            }

            await s.SendFrom(Side.Server, scopes);

            // после загрузки проверяем справочные сущности на совпадение
            var checker = new AfterSyncChecker(clSession);
            checker.CheckReferenceEntitiesAfterDownload(s.AddedOnServerIdsPerType);
            foreach (var type in types)
            {
                Assert.AreEqual(entitiesCount[type], clSession.QueryOver(type.Name, () => al).RowCount());
            }
        }

        [TestMethod]
        public async Task SendAddedPatientFromClient()
        {
            InMemoryHelper.FillData(clCfg, clSession, false);
            var clPatCount = clSession.Query<Patient>().Count();

            await s.WithoutCustomVocsInDoc().SendFrom(Side.Client, Scope.Holder);

            // новый пациент на клиенте
            var p = new Patient("x");
            session.DoSave(p);


            s = new Syncer(serverCon.ConnectionString, clientCon.ConnectionString, serverCon.ProviderName);
            await s.WithoutCustomVocsInDoc().SendFrom(Side.Client, Scope.Holder);

            Assert.AreEqual(clPatCount + 1, sSession.Query<Patient>().Count());
        }

        #region Vocs

        [TestMethod]
        public async Task SendVocScopeFromServerWithInstalledVocs()
        {
            var cVocCount = clSession.Query<Vocabulary>().Count();

            // не синхронизируем новые словари
            var installedVocs = sSession.Query<Vocabulary>();
            await s.WithInstalledVocs(installedVocs)
                .SendFrom(Side.Server, Scope.Voc);

            new VocLoader(clSession).AfterSyncVocs(s.DeletedOnServerIdsPerType);

            Assert.AreEqual(cVocCount, clSession.Query<Vocabulary>().Count());
            // специальности все равно загружаются
            Assert.AreEqual(2, clSession.Query<Speciality>().Count());
            Assert.AreEqual(0, clSession.Query<WordTemplate>().Count());
        }

        [TestMethod]
        public async Task SendVocScopeFromServer()
        {
            // без указания установленных словарей добавлены все сущности области словарей
            await s.SendFrom(Side.Server, Scope.Voc);
            new VocLoader(clSession).AfterSyncVocs(s.DeletedOnServerIdsPerType);

            Assert.AreEqual(sSession.Query<Vocabulary>().Count(), clSession.Query<Vocabulary>().Count());
            Assert.AreEqual(sSession.Query<WordTemplate>().Count(), clSession.Query<WordTemplate>().Count());
            Assert.AreEqual(sSession.Query<Speciality>().Count(), clSession.Query<Speciality>().Count());
        }

        [TestMethod]
        public async Task LoadSelectedVocWithNewSpec()
        {
            // load voc from server
            var voc = sSession.Get<Vocabulary>(IntToGuid<Vocabulary>(2));
            await s.OnlySelectedVocs(sSession, voc.ToEnumerable())
                .SendFrom(Side.Server, Scope.Voc);

            // добавлен словарь, шаблоны
            Assert.IsTrue(clSession.Query<Vocabulary>().Any(x => x.Id == voc.Id));
            var clientWtIds = clSession.Query<WordTemplate>().Select(x => x.Id).ToList();
            Assert.IsTrue(voc.WordTemplates.Select(x => x.Id).IsSubsetOf(clientWtIds));

            var vocOnCLient = clSession.Query<Vocabulary>()
                            .Where(v => v.Id == voc.Id)
                            .ToList();
            var l = new VocLoader(clSession);
            l.LoadOrUpdateVocs(vocOnCLient);

            // по каждому шаблону есть слово
            var clientWordTitles = GetWordTitles();
            Assert.IsTrue(voc.WordTemplates.Select(x => x.Title).All(x => clientWordTitles.Contains(x)));

            // загружена специальность выбранного словаря
            var clientSpecIds = clSession.Query<Speciality>().Select(x => x.Id).ToList();
            Assert.IsTrue(voc.Specialities.Select(x => x.Id).All(x => clientSpecIds.Contains(x)));
        }

        [TestMethod]
        public async Task LoadSelectedVocWithOldSpec()
        {
            var voc = sSession.Get<Vocabulary>(IntToGuid<Vocabulary>(2));
            // на клиенте уже есть специальности словаря
            var specTitles = voc.Specialities.Select(x => x.Title);
            specTitles.ForAll(t =>
            {
                clSession.DoSave(new Speciality(t));
            });

            // load voc from server
            await s.OnlySelectedVocs(sSession, voc.ToEnumerable())
                .SendFrom(Side.Server, Scope.Voc);

            // после загрузки проверяем справочные сущности на совпадение
            var checker = new AfterSyncChecker(clSession);
            checker.CheckReferenceEntitiesAfterDownload(s.AddedOnServerIdsPerType);

            // добавлен словарь, шаблоны
            Assert.IsTrue(clSession.Query<Vocabulary>().Any(x => x.Id == voc.Id));
            var clientWtIds = clSession.Query<WordTemplate>().Select(x => x.Id).ToList();
            Assert.IsTrue(voc.WordTemplates.Select(x => x.Id).IsSubsetOf(clientWtIds));

            var selectedSynced = clSession.Query<Vocabulary>()
                            .Where(v => v.Id == voc.Id)
                            .ToList();
            var l = new VocLoader(clSession);
            l.LoadOrUpdateVocs(selectedSynced);

            // по каждому шаблону есть слово
            var clientWordTitles = GetWordTitles();
            Assert.IsTrue(voc.WordTemplates.Select(x => x.Title).All(x => clientWordTitles.Contains(x)));

            // специальность выбранного словаря уже была с другим Ид, остается одна
            var clientSpecIds = clSession.Query<Speciality>().Select(x => x.Id).ToList();
            Assert.IsTrue(voc.Specialities.Select(x => x.Id).All(x => clientSpecIds.Contains(x)));
            Assert.AreEqual(voc.Specialities.Count(), clientSpecIds.Count());
        }

        [TestMethod]
        public async Task LoadVoc_AddNewWt_SendVocScopeFromServer()
        {
            var l = new VocLoader(clSession);

            // load voc from server
            var voc = sSession.Get<Vocabulary>(IntToGuid<Vocabulary>(1));
            await s.OnlySelectedVocs(sSession, voc.ToEnumerable())
                .SendFrom(Side.Server, Scope.Voc);

            // добавляем шаблон в словарь на сервере
            voc.AddTemplates(new[] { "qwe" });
            sSession.DoSave(voc);

            // загружаем новые шаблоны для установленных словарей
            var cWtCount = clSession.Query<WordTemplate>().Count();
            var installedVocs = sSession.Query<Vocabulary>();

            s = new Syncer(serverCon.ConnectionString, clientCon.ConnectionString, serverCon.ProviderName);
            await s.WithInstalledVocs(installedVocs)
                .SendFrom(Side.Server, Scope.Voc);
            new VocLoader(clSession).AfterSyncVocs(s.DeletedOnServerIdsPerType);

            Assert.AreEqual(cWtCount + 1, clSession.Query<WordTemplate>().Count());
        }

        [TestMethod]
        public async Task SaveVocToExchangeCreatesDoctorTable()
        {
            // выгружаем словари с сервера
            var sdfFileCon = new ConnectionInfo("exch.sdf", Constants.SqlCeProvider);
            var syncer = new Syncer(
                             serverConStr: serverCon.ConnectionString,
                             clientConStr: sdfFileCon.ConnectionString,
                             serverProviderName: serverCon.ProviderName);

            SqlHelper.CreateSqlCeByConStr(sdfFileCon.ConnectionString);

            var scopes = Scopes.GetOrderedDownloadScopes();
            await syncer.WithoutDoctors().SendFrom(Side.Server, scopes);

            // пробуем достать словари
            var nhib = NHibernateHelper.FromConnectionInfo(sdfFileCon, Side.Server);

            int exVocsCount;
            using (var s = nhib.OpenSession())
            using (var tr = s.BeginTransaction())
            {
                exVocsCount = s.Query<Vocabulary>().Count();
                // значит создана Doctors
                var docsCount = s.Query<Doctor>().Count();
            }

            var serverVocsCount = sSession.Query<Vocabulary>().Count(); // даже пользовательские, их не должно быть на сервере
            Assert.AreEqual(serverVocsCount, exVocsCount);
        }

        [TestMethod]
        public async Task SendFromClient_NoCustomVocsOnServer()
        {
            InMemoryHelper.FillData(clCfg, clSession, false);

            await s.WithoutCustomVocsInDoc().SendFrom(Side.Client);

            var sCustoms = sSession.Query<Vocabulary>().ToList();
            var clVocs = clSession.Query<Vocabulary>().ToList();
            var common = sCustoms.Intersect(clVocs);

            Assert.AreEqual(0, common.Where(x => x.IsCustom).Count());
        }

        [TestMethod]
        public async Task LoadVocs_NewSession_SendFromClient_DoctorOk()
        {
            // sync voc
            var voc = sSession.Get<Vocabulary>(IntToGuid<Vocabulary>(2));
            await s.OnlySelectedVocs(sSession, voc.ToEnumerable())
                .SendFrom(Side.Server, Scope.Voc);

            // close app
            SyncUtil.Clear();

            // sync user
            bool errorWas = false;
            EventHandler<StringEventArgs> h = (s1, e) =>
            {
                errorWas = e.str.Contains("Error");
            };
            Poster.MessagePosted += h;

            await s.WithoutCustomVocsInDoc().SendFrom(Side.Client, Scope.User);
            Poster.MessagePosted -= h;

            // doctor table already provisioned in user scope
            Assert.IsTrue(!errorWas);

        }

        #endregion Vocs

        private void Syncer_MessagePosted(object sender, StringEventArgs e)
        {
            System.Console.WriteLine(e.str);
        }
    }
}