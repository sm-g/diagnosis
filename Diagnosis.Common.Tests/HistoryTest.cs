using Diagnosis.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;

using System.Linq;

namespace Diagnosis.Common.Tests
{
    [TestClass]
    public class HistoryTest
    {
        private History<string> h;

        [TestInitialize]
        public void InitHistoryName()
        {
            h = new History<string>();
        }

        [TestMethod]
        public void Init()
        {
            Assert.AreEqual(null, h.CurrentState);
            Assert.AreEqual(-1, h.CurrentVersion);
            Assert.AreEqual(-1, h.LastVersion);
            Assert.AreEqual(true, h.CurrentIsFirst);
            Assert.AreEqual(true, h.CurrentIsLast);
        }

        [TestMethod]
        public void Add()
        {
            h.Memorize("qwe");
            Assert.AreEqual("qwe", h.CurrentState);
            Assert.AreEqual(0, h.CurrentVersion);
            Assert.AreEqual(h.CurrentVersion, h.LastVersion);
            Assert.AreEqual(true, h.CurrentIsFirst);
            Assert.AreEqual(true, h.CurrentIsLast);
        }

        [TestMethod]
        public void AddAddBack()
        {
            h.Memorize("1");
            h.Memorize("2");
            var b = h.MoveBack();

            Assert.AreEqual("1", b);
            Assert.AreEqual("1", h.CurrentState);
            Assert.AreEqual(true, h.CurrentIsFirst);
            Assert.AreEqual(false, h.CurrentIsLast);
        }

        [TestMethod]
        public void AddSameNotSaved()
        {
            h.Memorize("1");
            var e = h.Memorize("1");
            Assert.AreEqual(false, e);
            Assert.AreEqual(0, h.CurrentVersion);
        }

        [TestMethod]
        public void AddAddBackAdd()
        {
            h.Memorize("1");
            h.Memorize("2");
            var b = h.MoveBack();
            h.Memorize("3");
            Assert.AreEqual(1, h.CurrentVersion);
            Assert.AreEqual("3", h.CurrentState);
            Assert.AreEqual(true, h.CurrentIsLast);
        }

        [TestMethod]
        public void InitMove()
        {
            var b = h.MoveBack();
            var f = h.MoveForward();

            Assert.AreEqual(null, b);
            Assert.AreEqual(null, f);
        }

        [TestMethod]
        public void MoveBackAtStart()
        {
            h.Memorize("1");
            h.Memorize("2");
            var b1 = h.MoveBack();
            var b2 = h.MoveBack();
            Assert.AreEqual(true, h.CurrentIsFirst);
            Assert.AreEqual("1", h.CurrentState);        
            Assert.AreEqual(b1, b2);
        }

        [TestMethod]
        public void AddAddBackAddSame()
        {
            h.Memorize("1");
            h.Memorize("2");
            var b = h.MoveBack();
            h.Memorize("1");
            Assert.AreEqual(0, h.CurrentVersion);
            Assert.AreEqual(0, h.LastVersion);
            Assert.AreEqual("1", h.CurrentState);
        }

    }
}