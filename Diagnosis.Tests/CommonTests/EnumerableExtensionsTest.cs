using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using Diagnosis.Common;
using System.Collections.ObjectModel;

namespace Tests
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
    }
}
