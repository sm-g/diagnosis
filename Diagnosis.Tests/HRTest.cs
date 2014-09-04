using Diagnosis.App;
using Diagnosis.Core;
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
    public class HRTest : InMemoryDatabaseTest
    {
        [TestMethod]
        public void MyTestMethod()
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
    }
}
