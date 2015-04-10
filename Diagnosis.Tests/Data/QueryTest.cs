﻿using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate.Linq;
using System;
using System.Linq;

using System.Linq;

namespace Diagnosis.Tests.Data
{
    [TestClass]
    public class QueryTest : SdfDatabaseTest
    {
        [TestMethod]
        public void WordQueryByTitleUpperCase()
        {
            var w1 = session.Load<Word>(IntToGuid<Word>(1));
            var res = WordQuery.ByTitle(session)(w1.Title.ToUpper());
            Assert.AreEqual(w1, res);
        }

        [TestMethod]
        public void WordQueryStartingWithUpperCase()
        {
            var w1 = session.Load<Word>(IntToGuid<Word>(1));
            var res = WordQuery.StartingWith(session)(w1.Title.ToUpper());
            Assert.IsTrue(res.Contains(w1));
        }

        [TestMethod]
        public void PatientQueryStartingWithUpperCase()
        {
            var p1 = session.Load<Patient>(IntToGuid<Patient>(1));
            var res = PatientQuery.StartingWith(session)(p1.LastName.ToUpper());
            Assert.IsTrue(res.Contains(p1));
        }

        [TestMethod]
        public void UomQueryStartingWithUpperCase()
        {
            var u = session.Load<Uom>(IntToGuid<Uom>(1));
            var res = UomQuery.Contains(session)(u.Abbr.ToUpper());
            Assert.IsTrue(res.Contains(u));
        }

        [TestMethod]
        public void DiagnosisQueryStartingWithUpperCase()
        {
            var p1 = session.Load<IcdDisease>(1);
            var res = DiagnosisQuery.StartingWith(session)(p1.Title.Substring(0, 5).ToUpper());
            Assert.IsTrue(res.Contains(p1));
        }

        [TestMethod]
        public void DiagnosisQueryBlockStartingWithUpperCase()
        {
            var p1 = session.Load<IcdBlock>(91);
            var res = DiagnosisQuery.BlockStartingWith(session)(p1.Title.Substring(0, 5).ToUpper());
            Assert.IsTrue(res.Contains(p1));
        }

        [TestMethod]
        public void StartsWith()
        {
            using (var tx = session.BeginTransaction())
            {
                var id = session.Save(new Word("abcd"));

                tx.Commit();

                var words = session.Query<Word>().Where(m => m.Title.Contains("b")).ToList();

                Assert.AreEqual(id, words.First().Id);

                words = session.Query<Word>().Where(m => m.Title.StartsWith("a")).ToList();
                Assert.AreEqual(id, words.First().Id);
            }
        }
    }
}