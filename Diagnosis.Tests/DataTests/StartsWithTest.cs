﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;
using Diagnosis.Models;

namespace Tests
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