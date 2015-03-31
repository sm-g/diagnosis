using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class VocabularyListTest : InMemoryDatabaseTest
    {


        private VocabularyListViewModel vl;

        [TestInitialize]
        public void Init()
        {
            Load<Doctor>();

            AuthorityController.TryLogIn(d1);

            vl = new VocabularyListViewModel();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (vl != null)
                vl.Dispose();
            vl = null;
        }

        [TestMethod]
        public void DoctorDontSeeCustomVoc()
        {
            Assert.AreEqual(null, d1.Speciality);
            Assert.IsTrue(d1.CustomVocabulary != null);

            Assert.AreEqual(0, vl.Vocs.Count);
        }

        [TestMethod]
        public void DoctorSeeAllVocsForHisSpeciality()
        {
            vl.Dispose();

            AuthorityController.TryLogIn(d2);
            vl = new VocabularyListViewModel();

            Assert.IsTrue(d2.Speciality != null);
            Assert.AreEqual(1, vl.Vocs.Count);
            Assert.IsTrue(vl.Vocs.Select(y => y.voc).All(x => d2.Speciality.Vocabularies.Contains(x)));
        }
    }
}