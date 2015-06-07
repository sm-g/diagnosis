using Diagnosis.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.Models.Tests
{
    [TestClass]
    public class CritTest
    {
        private Criterion cr;

        [TestInitialize]
        public void Init()
        {
        }

        [TestMethod]
        public void SetWords()
        {
            cr = new Criterion();
            var w = new Word("1");
            var w2 = new Word("2");
            cr.SetWords(new[] { w, w2 });

            Assert.IsTrue(cr.Words.Contains(w));
            Assert.IsTrue(cr.Words.Contains(w2));

            cr.SetWords(new[] { w });
            Assert.IsTrue(cr.Words.Contains(w));
            Assert.IsFalse(cr.Words.Contains(w2));
        }
    }
}