﻿using Diagnosis.Common;
using Diagnosis.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using System.Linq;

namespace Diagnosis.Models.Tests
{
    [TestClass]
    public class HrItemTest : InMemoryDatabaseTest
    {
        private static Word w1 = new Word("1");
        private static Comment com = new Comment("comment");
        private new HealthRecord hr;

        [TestInitialize]
        public void AutocompleteTestInit()
        {
            hr = session.Get<HealthRecord>(IntToGuid<HealthRecord>(71)); // without hios
        }

        [TestMethod]
        public void ConfidenceHrItemEquals()
        {
            var chio1 = w1.AsConfidencable(Confidence.Absent);
            var chio2 = w1.AsConfidencable(Confidence.Absent);
            var chioOther = w1.AsConfidencable(Confidence.Present);

            Assert.AreEqual(chio1, chio2);
            Assert.AreEqual(0, chio2.CompareTo(chio1));
            Assert.IsTrue(chio1 != chio2);

            Assert.AreNotEqual(chio1, chioOther);
            Assert.IsTrue(chio1 != chioOther);
        }

        [TestMethod]
        public void HrItemCHIOGetter()
        {
            var item = new HrItem(hr, w1);
            Assert.AreEqual(Confidence.Present, item.Confidence);

            var chio = w1.AsConfidencable(Confidence.Present);
            Assert.AreEqual(chio, item.GetConfindenceHrItemObject());
        }

        [TestMethod]
        public void WithMeasure_WordNotNull()
        {
            var m = new Measure(1, word: w1);
            var item = new HrItem(hr, m);
            Assert.AreEqual(m, item.Measure);
            Assert.AreEqual(m, item.Entity);
            Assert.AreEqual(w1, item.Word);
        }
    }
}