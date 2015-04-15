using Diagnosis.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.Common.Tests
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