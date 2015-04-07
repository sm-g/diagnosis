using Diagnosis.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Diagnosis.Tests.Common
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
    }
}