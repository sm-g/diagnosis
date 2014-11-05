using Diagnosis.App;
using Diagnosis.Common;
using Diagnosis.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
namespace Tests
{
    [TestClass]
    public class QuerySeparatorTest
    {
        QuerySeparator qs;
        [TestInitialize]
        public void Init()
        {
            qs = new QuerySeparator(' ', ',');
        }

        [TestMethod]
        public void TestFormatDelimeters()
        {
            var i = "a,, b t ,r,";
            var o = "a, b t, r, ";
            Assert.AreEqual(o, qs.FormatDelimiters(i));
        }

        [TestMethod]
        public void TestFormatDelimeters2()
        {
            var i = "a,   b ";
            var o = "a, b";
            Assert.AreEqual(o, qs.FormatDelimiters(i));
        }

        [TestMethod]
        public void TestFormatDelimeters3()
        {
            var i = "a   b";
            var o = "a b";
            Assert.AreEqual(o, qs.FormatDelimiters(i));
        }
    }
}
