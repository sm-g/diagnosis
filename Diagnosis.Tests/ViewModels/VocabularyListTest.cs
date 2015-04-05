using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Tests.ViewModels
{
    [TestClass]
    public class VocabularyListTest : InMemoryDatabaseTest
    {


        private VocabularySyncViewModel vl;

        [TestInitialize]
        public void Init()
        {
            Load<Doctor>();

            AuthorityController.TryLogIn(d1);

            vl = new VocabularySyncViewModel();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (vl != null)
                vl.Dispose();
            vl = null;
        }

        [TestMethod]
        public void DontShowCustomVoc()
        {
            // есть пльзовательский словарь в бд
            Assert.IsTrue(d1.CustomVocabulary != null);
            // но еего нет в списке установленных
            Assert.IsFalse(vl.Vocs.Any(x => x.voc == d1.CustomVocabulary));
        }

        [TestMethod]
        public void DoctorSeeAllVocsForHisSpeciality() // если бы он видел список словарей
        {
            //vl.Dispose();

            //AuthorityController.TryLogIn(d2);
            //vl = new VocabularyListViewModel();

            //Assert.IsTrue(d2.Speciality != null);
            //Assert.AreEqual(1, vl.Vocs.Count);
            //Assert.IsTrue(vl.Vocs.Select(y => y.voc).All(x => d2.Speciality.Vocabularies.Contains(x)));
        }
    }
}