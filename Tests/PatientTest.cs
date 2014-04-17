using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Diagnosis.Models;

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

    }
}
