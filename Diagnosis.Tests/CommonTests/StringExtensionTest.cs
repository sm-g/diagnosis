using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Diagnosis.Common;

namespace Tests
{
    [TestClass]
    public class StringExtensionTest
    {
        [TestMethod]
        public void TestMethod()
        {
            var a = "aaa   ";
            Assert.AreEqual(a.TrimedOrNull(), "aaa");

            a = "  aaa";
            Assert.AreEqual(a.TrimedOrNull(), "aaa");

            a = "   ";
            Assert.AreEqual(a.TrimedOrNull(), null);
            a = "\t\n\r";
            Assert.AreEqual(a.TrimedOrNull(), null);
        }
    }
}
