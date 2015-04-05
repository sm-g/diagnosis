using Diagnosis.Common;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Tests.Model
{
    [TestClass]
    public class HrItemTest : InMemoryDatabaseTest
    {
        private static Word w1 = new Word("1");
        private static Comment com = new Comment("comment");
        private new HealthRecord hr;

        [TestInitialize]
        public void AutocompleteTestInit()
        {
            hr = session.Get<HealthRecord>(IntToGuid<HealthRecord>(71)); // without hios
        }

        [TestMethod]
        public void ConfidenceHrItemEquals()
        {
            var chio1 = new ConfindenceHrItemObject(w1, Confidence.Absent);
            var chio2 = new ConfindenceHrItemObject(w1, Confidence.Absent);
            var chioOther = new ConfindenceHrItemObject(w1, Confidence.Present);

            Assert.AreEqual(chio1, chio2);
            Assert.AreEqual(0, chio2.CompareTo(chio1));
            Assert.IsTrue(chio1 != chio2);

            Assert.AreNotEqual(chio1, chioOther);
            Assert.IsTrue(chio1 != chioOther);
        }

        [TestMethod]
        public void HrItemCHIOGetter()
        {
            var item = new HrItem(hr, w1);
            Assert.AreEqual(Confidence.Present, item.Confidence);

            var chio = new ConfindenceHrItemObject(w1, Confidence.Present);
            Assert.AreEqual(chio, item.CHIO);
        }
    }
}