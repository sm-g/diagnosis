using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class DoctorTest : InMemoryDatabaseTest
    {
        private Doctor d2;

        [TestInitialize]
        public void Init()
        {
            d2 = session.Get<Doctor>(IntToGuid<Doctor>(2));
        }

        [TestMethod]
        public void CustomVocCreatedAfterSaveNewWord()
        {
            AuthorityController.TryLogIn(d2);
            Assert.IsTrue(d2.CustomVocabulary.IsTransient);

            var newW = CreateWordInEditor("123");

            Assert.IsFalse(newW.IsTransient);
            Assert.IsFalse(d2.CustomVocabulary.IsTransient);
        }
    }
}