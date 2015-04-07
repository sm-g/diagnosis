using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.Tests.Data
{
    [TestClass]
    public class PersistTest : InMemoryDatabaseTest
    {
        [TestMethod]
        public void MyTestMethod()
        {
            Load<Doctor>();
            AuthorityController.TryLogIn(d1);
            using (var tx = session.BeginTransaction())
            {
                var p = new Patient();
                var c = p.AddCourse(d1);
                p.RemoveCourse(c);
                Assert.IsTrue(p.Courses.Count() == 0); // see output

                session.SaveOrUpdate(p);
                tx.Commit();
            }
        }
    }
}