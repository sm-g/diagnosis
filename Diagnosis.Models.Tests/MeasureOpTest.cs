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
            Assert.AreEqual(3, op.RightBetweenValue);
            Assert.AreEqual(3, op.Value);
        
        }

        [TestMethod]
        public void SetRightBetweenValue()
        {
            var op = new MeasureOp(MeasureOperator.Between, 3) { RightBetweenValue = 5 };
            Assert.AreEqual(5, op.RightBetweenValue);
            Assert.AreEqual(3, op.Value);
        }

        [TestMethod]
        public void ChangeValue()
        {
            var op = new MeasureOp(MeasureOperator.Between, 3);
            op.Value = 4;
            Assert.AreEqual(4, op.RightBetweenDbValue);
            Assert.AreEqual(4, op.Value);
        }

        [TestMethod]
        public void ChangeRightBetweenValue()
        {
            var op = new MeasureOp(MeasureOperator.Between, 3) { RightBetweenValue = 5 };
            op.RightBetweenValue = 4;
            Assert.AreEqual(4, op.RightBetweenDbValue);
            Assert.AreEqual(3, op.Value);
        }
        [TestMethod]
        public void SetRightBetweenValueLessThanValue()
        {
            var op = new MeasureOp(MeasureOperator.Between, 3) { RightBetweenValue = 2 };
            Assert.AreEqual(3, op.RightBetweenValue);
            Assert.AreEqual(2, op.Value);
        }
        [TestMethod]
        public void ChangeRightBetweenValueOutOfRange()
        {
            var op = new MeasureOp(MeasureOperator.Between, 3) { RightBetweenValue = 5 };
            op.RightBetweenValue = 2;
            Assert.AreEqual(3, op.RightBetweenDbValue);
            Assert.AreEqual(2, op.Value);
        }

        [TestMethod]
        public void CreateNotBetweenOp()
        {
            var op = new MeasureOp(MeasureOperator.Equal, 3);
            Assert.AreEqual(3, op.RightBetweenValue);
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
            Assert.AreEqual(3, op.RightBetweenValue);
        }
    }
}