using Diagnosis.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.Models.Tests
{
    [TestClass]
    public class WordTest
    {
        [TestMethod]
        public void CompareIgnoreCase()
        {
            var w1 = new Word("Q");
            var w2 = new Word("q");

            Assert.IsTrue(w1.CompareTo(w2) == 0);
        }
    }
}