﻿using Diagnosis.Common;
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
        private Doctor d1;
        private Doctor d2;
        private VocabularyListViewModel vl;

        [TestInitialize]
        public void Init()
        {
            d1 = session.Get<Doctor>(IntToGuid<Doctor>(1));
            d2 = session.Get<Doctor>(IntToGuid<Doctor>(1));
            AuthorityController.TryLogIn(d1);

            vl = new VocabularyListViewModel();
        }

        [TestCleanup]
        public void Cleanup()
        {
            vl.Dispose();
        }

        [TestMethod]
        public void DoctorSeeAllVocsForHisSpeciality()
        {
            Assert.AreEqual(null, d1.Speciality);
            Assert.AreEqual(0, vl.Vocs.Count);

            vl.Dispose();
            vl = new VocabularyListViewModel();

            AuthorityController.TryLogIn(d2);

            Assert.IsTrue(d1.Speciality != null);
            Assert.AreEqual(1, vl.Vocs.Count);
            Assert.IsTrue(vl.Vocs.Select(y => y.voc).All(x => d1.Speciality.Vocabularies.Contains(x)));
        }
    }
}