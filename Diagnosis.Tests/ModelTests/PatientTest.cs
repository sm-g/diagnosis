using Diagnosis.Common;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var p1 = new Patient("Иванов", "Иван");
            var p2 = new Patient();
            var p3 = new Patient("Иванов", "Петр");

            var list = new List<Patient>(new[] { p1, p2, p3 }.OrderBy(p => p.FullName, new EmptyStringsAreLast()));
            Assert.IsTrue(list[0] == p1);
            Assert.IsTrue(list[1] == p3);
            Assert.IsTrue(list[2] == p2);
        }

        [TestMethod]
        public void StartCourse()
        {
            var p1 = new Patient("Иванов", "Иван");
            var d1 = new Doctor("last", "first");

            var patientCoursesBefore = p1.Courses.Count();
            d1.StartCourse(p1);

            Assert.AreEqual(patientCoursesBefore + 1, p1.Courses.Count());
        }
    }
}