using Diagnosis.Common;
using Diagnosis.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.Models.Tests
{
    [TestClass]
    public class MeasureOpTest
    {
        [TestMethod]
        public void Create()
        {
            var op = new MeasureOp(MeasureOperator.Between, 3);
            Assert.AreEqual(3, op.Value);

        }

        [TestMethod]
        public void SetRightBetweenValue()
        {
            var op = new MeasureOp(MeasureOperator.Between, 3) { RightValue = 5 };
            Assert.AreEqual(5, op.RightValue);
            Assert.AreEqual(3, op.Value);
        }

        [TestMethod]
        public void ChangeValue()
        {
            var op = new MeasureOp(MeasureOperator.Between, 3);
            op.Value = 4;
            Assert.AreEqual(4, op.Value);
        }

        [TestMethod]
        public void ChangeRightBetweenValue()
        {
            var op = new MeasureOp(MeasureOperator.Between, 3) { RightValue = 5 };
            op.RightValue = 4;
            Assert.AreEqual(4, op.RightValue);
            Assert.AreEqual(3, op.Value);
        }
        [TestMethod]
        public void SetRightBetweenValueLessThanValue()
        {
            var op = new MeasureOp(MeasureOperator.Between, 3) { RightValue = 2 };
            Assert.AreEqual(2, op.RightValue);
            Assert.AreEqual(3, op.Value);
        }
        [TestMethod]
        public void ChangeRightBetweenValueOutOfRange()
        {
            var op = new MeasureOp(MeasureOperator.Between, 3) { RightValue = 5 };
            op.RightValue = 2;
            Assert.AreEqual(2, op.RightValue);
            Assert.AreEqual(3, op.Value);
        }

        [TestMethod]
        public void CreateNotBetweenOp()
        {
            var op = new MeasureOp(MeasureOperator.Equal, 3);
            Assert.AreEqual(3, op.Value);
        }

        [TestMethod]
        public void ChangeValueNotBetweenOp()
        {
            var op = new MeasureOp(MeasureOperator.Equal, 3);
            op.Value = 4;
            Assert.AreEqual(4, op.Value);
        }

        [TestMethod]
        public void ChangeOpToBetween()
        {
            var op = new MeasureOp(MeasureOperator.Equal, 3);
            op.Operator = MeasureOperator.Between;
            Assert.AreEqual(3, op.Value);
        }
    }
}