using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.Tests;
using Diagnosis.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.Models.Tests
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

            var newW = CreateWordAsInEditor("123");

            Assert.IsFalse(newW.IsTransient);
            Assert.IsFalse(d2.CustomVocabulary.IsTransient);
        }

        [TestMethod]
        public void DoctorSeeAllWordsForHisSpeciality()
        {
            AuthorityController.TryLogIn(d2);

            Assert.IsTrue(d2.Speciality != null);
            Assert.IsTrue(d2.Speciality.Vocabularies.SelectMany(y => y.Words).All(x => d2.SpecialityWords.Contains(x)));
        }
    }
}