using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.Tests.Model
{
    [TestClass]
    public class DoctorTest : InMemoryDatabaseTest
    {


        [TestInitialize]
        public void Init()
        {
            Load<Doctor>();
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