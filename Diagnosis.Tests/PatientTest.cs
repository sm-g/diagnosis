using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Diagnosis.Core;

namespace Tests
{
    [TestClass]
    public class PatientTest
    {
        [TestMethod]
        public void TestAge()
        {
            var p = new Patient(year: 2000, month: 1, day: 5);
            var age = DateTime.Today.Year - p.BirthYear.Value;
            if (new DateTime(p.BirthYear.Value, p.BirthMonth.Value, p.BirthDay.Value) > DateTime.Today.AddYears(-age))
                age--;
            Assert.IsTrue(p.Age == age);
        }

        [TestMethod]
        public void TestOrder()
        {
            var p1 = new PatientViewModel(new Patient("Иванов", "Иван"));
            var p2 = new PatientViewModel(new Patient());
            var p3 = new PatientViewModel(new Patient("Иванов", "Петр"));

            var list = new List<PatientViewModel>(new[] { p1, p2, p3 }.OrderBy(p => p.patient.FullName, new EmptyStringsAreLast()));
            Assert.IsTrue(list[0] == p1);
            Assert.IsTrue(list[1] == p3);
            Assert.IsTrue(list[2] == p2);
        }
    }
}
