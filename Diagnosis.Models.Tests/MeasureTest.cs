﻿using Diagnosis.Common;
using Diagnosis.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.Models.Tests
{
    [TestClass]
    public class MeasureTest : InMemoryDatabaseTest
    {
        private new Word w = new Word("word");

        [TestInitialize]
        public void Init()
        {
            Load<Uom>();
            // uom 1 2 3 - same UomType
            Assert.IsTrue(new[] { 1, 2, 3 }.Select(x => uom[x]).Select(x => x.Type).Distinct().Count() == 1);
        }

        #region Equality and compare

        [TestMethod]
        public void OnlyValue()
        {
            var m = new Measure(0);
            Assert.AreEqual(null, m.Word);
            Assert.AreEqual(null, m.Uom);
        }

        [TestMethod]
        public void NoWordNoUom()
        {
            var m = new Measure(0);
            var m2 = new Measure(0);
            Assert.IsTrue(m == m2);
            Assert.AreEqual(0, m.CompareTo(m2));
            Assert.AreEqual(true, m.Equals(m2));
            Assert.AreEqual(0, m.StrictCompareTo(m2));
        }

        [TestMethod]
        public void NoWordSameUom()
        {
            var m = new Measure(0, uom[1]);
            var m2 = new Measure(0, uom[1]);
            Assert.IsTrue(m == m2);
            Assert.AreEqual(0, m.CompareTo(m2));
            Assert.AreEqual(true, m.Equals(m2));
            Assert.AreEqual(0, m.StrictCompareTo(m2));
        }

        [TestMethod]
        public void NoWordDiffUom()
        {
            var m = new Measure(0, uom[1]);
            var m2 = new Measure(0);
            Assert.IsTrue(m != m2);
            Assert.AreNotEqual(0, m.CompareTo(m2));
            Assert.AreEqual(false, m.Equals(m2));
            Assert.AreNotEqual(0, m.StrictCompareTo(m2));
        }

        [TestMethod]
        public void SameWordNoUom()
        {
            var m = new Measure(0) { Word = w };
            var m2 = new Measure(0) { Word = w };
            Assert.AreEqual(m, m2);
            Assert.IsTrue(m == m2);
            Assert.AreEqual(0, m.CompareTo(m2));
            Assert.AreEqual(true, m.Equals(m2));
            Assert.AreEqual(0, m.StrictCompareTo(m2));
        }

        [TestMethod]
        public void SameWordSameUom()
        {
            var m = new Measure(0, uom[1]) { Word = w };
            var m2 = new Measure(0, uom[1]) { Word = w };
            Assert.AreEqual(m, m2);
            Assert.IsTrue(m == m2);
            Assert.AreEqual(0, m.CompareTo(m2));
            Assert.AreEqual(true, m.Equals(m2));
        }

        [TestMethod]
        public void SameWordDiffUom()
        {
            var m = new Measure(1, uom[1]) { Word = w };
            var m2 = new Measure(1, uom[2]) { Word = w };
            Assert.IsTrue(m != m2);
            Assert.AreNotEqual(0, m.CompareTo(m2));
            Assert.AreEqual(false, m.Equals(m2));

            Assert.AreNotEqual(0, m.StrictCompareTo(m2));
        }

        [TestMethod]
        public void SameWordDiffUomZero()
        {
            var m = new Measure(0, uom[1]) { Word = w };
            var m2 = new Measure(0, uom[2]) { Word = w };
            Assert.IsTrue(m != m2);
            Assert.AreEqual(0, m.CompareTo(m2));
            Assert.AreEqual(false, m.Equals(m2));

            Assert.AreNotEqual(0, m.StrictCompareTo(m2));
        }

        [TestMethod]
        public void DiffWordNoUom()
        {
            var m = new Measure(0);
            var m2 = new Measure(0) { Word = w };
            Assert.IsTrue(m != m2);
            Assert.AreNotEqual(0, m.CompareTo(m2));
            Assert.AreEqual(false, m.Equals(m2));
            Assert.AreNotEqual(0, m.StrictCompareTo(m2));
        }

        [TestMethod]
        public void DiffWordSameUom()
        {
            var m = new Measure(0, uom[1]);
            var m2 = new Measure(0, uom[1]) { Word = w };
            Assert.IsTrue(m != m2);
            Assert.AreNotEqual(0, m.CompareTo(m2));
            Assert.AreEqual(false, m.Equals(m2));
            Assert.AreNotEqual(0, m.StrictCompareTo(m2));
        }

        [TestMethod]
        public void DiffWordDiffUom()
        {
            var m = new Measure(0, uom[1]);
            var m2 = new Measure(0, uom[2]) { Word = w };
            Assert.IsTrue(m != m2);
            Assert.AreNotEqual(0, m.CompareTo(m2));
            Assert.AreEqual(false, m.Equals(m2));
            Assert.AreNotEqual(0, m.StrictCompareTo(m2));
        }

        [TestMethod]
        public void DiffUomType()
        {
            var m = new Measure(0, uom[1]);
            var m2 = new Measure(0, uom[4]);
            Assert.IsTrue(m != m2);
            Assert.AreNotEqual(0, m.CompareTo(m2));
            Assert.AreEqual(false, m.Equals(m2));
            Assert.AreNotEqual(0, m.StrictCompareTo(m2));
        }

        [TestMethod]
        public void DiffUomSameDbValue()
        {
            var m = new Measure(1, uom[1]); // 1 л
            var m2 = new Measure(Math.Pow(10, uom[1].Factor - uom[2].Factor), uom[2]); // 1000 мл

            Assert.IsTrue(m != m2);
            Assert.AreEqual(0, m.CompareTo(m2));
            Assert.AreEqual(false, m.Equals(m2));
            Assert.AreNotEqual(0, m.StrictCompareTo(m2));
        }

        #endregion Equality and compare


    }
}