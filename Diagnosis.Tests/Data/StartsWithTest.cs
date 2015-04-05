using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate.Linq;
using System;
using System.Linq;

namespace Tests.Data
{
    [TestClass]
    public class StartsWithTest : InMemoryDatabaseTest
    {
        [TestMethod]
        public void MyTestMethod()
        {
            object id;
            using (var tx = session.BeginTransaction())
            {
                id = session.Save(new Word("abcd"));

                tx.Commit();

                var words = session.Query<Word>().Where(m => m.Title.Contains("b")).ToList();

                Assert.AreEqual(id, words.First().Id);

                words = session.Query<Word>().Where(m => m.Title.StartsWith("a")).ToList();
                Assert.AreEqual(id, words.First().Id);
            }
        }
    }
}