using Diagnosis.Common;
using Diagnosis.Tests;
using Diagnosis.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.Models.Tests
{
    [TestClass]
    public class AuthorityControllerTest : InMemoryDatabaseTest
    {
        [TestMethod]
        public void DoctorSeeAllWordsForHisSpeciality()
        {
            AuthorityController.TryLogIn(d2);

            Assert.IsTrue(d2.Speciality != null);
            Assert.IsTrue(d2.Speciality.Vocabularies.SelectMany(y => y.Words).All(x => d2.SpecialityWords.Contains(x)));
        }
    }
}