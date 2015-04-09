using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Sync;
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

        [TestMethod]
        public void AddWordSaveVoc()
        {
            Load<Vocabulary>();
            var w = new Word("asdsad");
            voc[1].AddWord(w);

            Assert.IsTrue(voc[1].Words.Contains(w));

            new Saver(session).Save(voc[1]);

            Assert.IsTrue(!w.IsTransient);
        }

        [TestMethod]
        public void RemoveWordSaveVoc()
        {
            Load<Vocabulary>();

            var l = new VocLoader(session);
            l.LoadOrUpdateVocs(voc[1]);
            var w = voc[1].Words.First();

            voc[1].RemoveWord(w);
            w.OnDelete();
            new Saver(session).Delete(w); // или после
            new Saver(session).Save(voc[1]);

            Assert.IsTrue(!voc[1].Words.Contains(w));
            Assert.IsTrue(w.Vocabularies.Count() == 0);
            Assert.IsTrue(!session.QueryOver<Word>().List().Any(x => x.Title == w.Title));
            Assert.IsTrue(w.IsTransient);

        }
    }
}