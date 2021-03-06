﻿using Diagnosis.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Diagnosis.Common.Tests
{
    [TestClass]
    public class EnumerableExtensionsTest
    {
        [TestMethod]
        public void TestElementNear()
        {
            IEnumerable<object> list = new List<object> { 1, 2, 3 };
            Assert.AreEqual(2, list.ElementNear(1));
            Assert.AreEqual(1, list.ElementNear(-5));
            Assert.AreEqual(3, list.ElementNear(5));

            list = new List<object> { };
            Assert.AreEqual(null, list.ElementNear(5));
        }

        [TestMethod]
        public void TestAddSorted()
        {
            var coll = new ObservableCollection<int>() { 1, 3, 5 };
            coll.AddSorted(2, x => x);

            Assert.AreEqual(1, coll.IndexOf(2));
        }

        [TestMethod]
        public void TestAddSortedReverse()
        {
            var coll = new ObservableCollection<int>(new[] { 1, 3, 5 }.Reverse());
            coll.AddSorted(2, x => x, true);

            Assert.AreEqual(2, coll.IndexOf(2));
        }

        [TestMethod]
        public void SortObservableCollectionReverse()
        {
            var coll = new ObservableCollection<int>(new[] { 1, 3, 5 });
            coll.Sort(x => x, true);

            Assert.AreEqual(2, coll.IndexOf(1));
        }

        [TestMethod]
        public void FirstAfterAndNotIn()
        {
            var list = new List<object> { 1, 2, 3, 4 };

            var test = new List<object> { };
            Assert.AreEqual(1, list.FirstAfterAndNotIn(test));

            test = new List<object> { 5 };
            Assert.AreEqual(1, list.FirstAfterAndNotIn(test));

            test = new List<object> { 1, 3 };
            Assert.AreEqual(2, list.FirstAfterAndNotIn(test));

            test = new List<object> { 1, 3 };
            Assert.AreEqual(4, list.FirstAfterAndNotIn(test, 1));

            test = new List<object> { 1, 3 };
            Assert.AreEqual(2, list.FirstAfterAndNotIn(test, 0, false));

            test = new List<object> { 3, 1 };
            Assert.AreEqual(4, list.FirstAfterAndNotIn(test));

            test = new List<object> { 4, 3 };
            Assert.AreEqual(2, list.FirstAfterAndNotIn(test));

            test = new List<object> { 1, 2, 3, 4 };
            Assert.AreEqual(null, list.FirstAfterAndNotIn(test));

            object obj = 5;
            list = new List<object> { obj, 1, obj, 2 };
            test = new List<object> { obj };
            Assert.AreEqual(1, list.FirstAfterAndNotIn(test));

            test = new List<object> { 1, obj };
            Assert.AreEqual(2, list.FirstAfterAndNotIn(test));
        }

        [TestMethod]
        public void IsOrdered()
        {
            var list = new List<int> { 1, 2, 3, 4 };
            Assert.IsTrue(list.IsOrdered(x => x));

            list = new List<int> { 1, 2, 3, 3 };
            Assert.IsTrue(list.IsOrdered(x => x));

            list = new List<int> { 1, 2, 3, 2 };
            Assert.IsFalse(list.IsOrdered(x => x));

            list = new List<int> { 3, 2, 3, 4 };
            Assert.IsFalse(list.IsOrdered(x => x));
        }

        [TestMethod]
        public void Submultiset()
        {
            var list = new List<int> { 1, 2, 3, 4 };
            var list2 = new List<int> { 1, 1 };

            Assert.AreEqual(false, list2.IsSubmultisetOf(list));
        }

        [TestMethod]
        public void Submultiset2()
        {
            var list = new List<int> { 1, 2, 3, 4 };
            var list2 = new List<int> { };

            Assert.AreEqual(true, list2.IsSubmultisetOf(list));
        }

        [TestMethod]
        public void Submultiset3()
        {
            var list = new List<int> { 1, 1, 1, 4 };
            var list2 = new List<int> { 1, 1, 1 };

            Assert.AreEqual(true, list2.IsSubmultisetOf(list));
        }

        [TestMethod]
        public void DifferenceWith()
        {
            var list = new List<int> { 1, 1, 1, 2 };
            var list2 = new List<int> { 1, 1 };

            var exp = new[] { 1, 2 };

            Assert.IsTrue(exp.ScrambledEquals(list.DifferenceWith(list2)));
        }

        [TestMethod]
        public void DifferenceWith2()
        {
            var list = new List<int> { 1, 1, 1, 2 };
            var list2 = new List<int> { };

            var exp = new[] { 1, 1, 1, 2 };

            Assert.IsTrue(exp.ScrambledEquals(list.DifferenceWith(list2)));
        }
        [TestMethod]
        public void DifferenceWith3()
        {
            var list = new List<int> { 1, 1, 1, 2 };
            var list2 = new List<int> { 1, 3 };

            var exp = new[] { 1, 1, 2 };

            Assert.IsTrue(exp.ScrambledEquals(list.DifferenceWith(list2)));
        }
        [TestMethod]
        public void Mode()
        {
            var list = new List<string> { "1", "1", "2" };
            Assert.AreEqual("1", list.Mode());

        }

        [TestMethod]
        public void Mode2()
        {
            var list = new List<string> { "1", "2" };
            Assert.AreEqual("1", list.Mode());

        }

        [TestMethod]
        public void ScrambledEquals()
        {
            var list = new List<string> { "1", "2" };
            var list2 = new List<string> { "2", "1" };
            Assert.IsTrue(list.ScrambledEquals(list2));
        }
        [TestMethod]
        public void ScrambledEquals2()
        {
            var list = new List<string> { "1", "2", "2" };
            var list2 = new List<string> { "2", "1" };
            Assert.IsTrue(!list.ScrambledEquals(list2));
        }

        [TestMethod]
        public void IsUniqueByTitleIgnoreCase()
        {
            var word1 = new { Title = "Q" };
            var word2 = new { Title = "q" };
            var words = new[] { word1, word2 };
            Assert.IsFalse(words.IsUnique(x => x.Title, StringComparer.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void IsUnique()
        {
            var word1 = new { Title = "Q" };
            var word2 = new { Title = "q" };
            var words = new[] { word1, word2 };
            Assert.IsTrue(words.IsUnique(x => x.Title));
        }
    }
}