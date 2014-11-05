using Diagnosis.App;
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
    }
}
