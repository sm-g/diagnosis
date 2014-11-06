using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Diagnosis.Common;

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
    }
}
