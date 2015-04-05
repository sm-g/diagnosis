using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Model
{
    [TestClass]
    public class HrCategoryTest
    {
        [TestMethod]
        public void SameName()
        {
            var cat = new HrCategory("test", 1);
            var cat2 = new HrCategory("test", 1);
            Assert.IsTrue(cat != cat2);
            Assert.AreNotEqual(cat, cat2);
            Assert.IsTrue(cat.EqualsByVal(cat2));
        }

        [TestMethod]
        public void NullName()
        {
            var cat = HrCategory.Null;
            var cat2 = new HrCategory("test", 1);
            Assert.IsTrue(cat != cat2);
            Assert.AreNotEqual(cat, cat2);
        }

        [TestMethod]
        public void NullNull()
        {
            var cat = HrCategory.Null;
            var cat2 = HrCategory.Null;
            Assert.IsTrue(cat == cat2);
            Assert.AreEqual(cat, cat2);
        }

        [TestMethod]
        public void NullNothing()
        {
            var cat = HrCategory.Null;
            HrCategory cat2 = null;
            Assert.IsTrue(cat != cat2);
            Assert.IsFalse(cat.Equals(cat2));
            Assert.IsFalse(object.Equals(cat, cat2));
        }

        [TestMethod]
        public void NullNameOfNull()
        {
            var cat = HrCategory.Null;
            var cat2 = new HrCategory(HrCategory.Null.Title, 1);
            Assert.IsTrue(cat != cat2);
            Assert.AreNotEqual(cat, cat2);
            Assert.IsTrue(cat.EqualsByVal(cat2));
        }
    }
}