using Diagnosis.Client.App;
using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Mappings;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;
using NHibernate.Tool.hbm2ddl;

namespace Tests
{
    [TestClass]
    public class PersistTest : InMemoryDatabaseTest
    {
        [TestMethod]
        public void Patient()
        {
            object id;

            using (var tx = session.BeginTransaction())
            {
                id = session.Save(new Patient()
                {
                    Age = 15,
                    LastName = "Hello"
                });

                tx.Commit();
            }

            session.Clear();


            using (var tx = session.BeginTransaction())
            {
                var pat = session.Get<Patient>(id);

                Assert.AreEqual(15, pat.Age);
                Assert.AreEqual("Hello", pat.LastName);

                tx.Commit();
            }
        }

        [TestMethod]
        public void MyTestMethod()
        {
            var d1 = session.Get<Doctor>(IntToGuid<Doctor>(1));
            AuthorityController.LogIn(d1);
            using (var tx = session.BeginTransaction())
            {
                var p = new Patient();
                var c = d1.StartCourse(p);
                p.RemoveCourse(c);
                Assert.IsTrue(p.Courses.Count() == 0); // see output

                session.SaveOrUpdate(p);
                tx.Commit();
            }
        }
    }
}
