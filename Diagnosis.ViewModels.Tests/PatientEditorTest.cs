using Diagnosis.Models;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class PatientEditorTest : ViewModelTest
    {
        private PatientEditorViewModel e;

        [TestInitialize]
        public void PatientEditorTestInit()
        {
            Load<Patient>();
        }

        [TestCleanup]
        public void PatientEditorTestCleanup()
        {
            if (e != null)
                e.Dispose();
        }

        [TestMethod]
        public void DataConditions()
        {
        }

        [TestMethod]
        public void SavePatient()
        {
            var p = new Patient();
            e = new PatientEditorViewModel(p);

            Assert.IsTrue(e.IsUnsaved);

            var name = "  qwe asd zxС hj";
            e.Patient.FullName = name;
            e.Patient.Age = 10;
            e.Patient.IsFemale = true;

            Assert.AreEqual(10, p.Age);
            Assert.AreEqual(name.Trim(), p.FullNameOrCreatedAt);
            Assert.AreEqual(false, p.IsMale);
        }

        [TestMethod]
        public void InvalidYear()
        {
            var p = new Patient();
            e = new PatientEditorViewModel(p);

            e.Patient.BirthYear = DateTime.Today.Year + 1;

            Assert.IsTrue(!e.CanOk);
        }
    }
}