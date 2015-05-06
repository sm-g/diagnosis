using Diagnosis.Common;
using Diagnosis.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.Models.Tests
{
    [TestClass]
    public class MeasureOpTest : InMemoryDatabaseTest
    {
        [TestInitialize]
        public void Init()
        {
            Load<Uom>();
            // uom 1 2 3 - same UomType
            Assert.IsTrue(new[] { 1, 2, 3 }.Select(x => uom[x]).Select(x => x.Type).Distinct().Count() == 1);
        }

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

        [TestMethod]
        public void Equals()
        {
            var op = new MeasureOp(MeasureOperator.Equal, 3);
            var op2 = new MeasureOp(MeasureOperator.Equal, 3);
            Assert.AreEqual(op.GetHashCode(), op2.GetHashCode());
            Assert.AreEqual(op, op2);
        }
        [TestMethod]
        public void EqualsWithRight()
        {
            var op = new MeasureOp(MeasureOperator.Equal, 3) { RightValue = 7 };
            var op2 = new MeasureOp(MeasureOperator.Equal, 3) { RightValue = 7 };
            Assert.AreEqual(op.GetHashCode(), op2.GetHashCode());
            Assert.AreEqual(op, op2);
        }
        #region ValueComparer

        [TestMethod]
        public void CompareGr()
        {
            var m = new Measure(5, uom[1]);
            var op = new MeasureOp(MeasureOperator.GreaterOrEqual, 1, uom[1]);
            Assert.AreEqual(true, op.ResultFor(m));
        }

        [TestMethod]
        public void CompareGrDiffUom()
        {
            var m = new Measure(2, uom[1]); // 2 л
            var op = new MeasureOp(MeasureOperator.Greater, Math.Pow(10, uom[1].Factor - uom[2].Factor), uom[2]); // 1000 мл

            Assert.AreEqual(true, op.ResultFor(m));
        }

        [TestMethod]
        public void CompareEqDiffUom()
        {
            var m = new Measure(1, uom[1]); // 1 л
            var op = new MeasureOp(MeasureOperator.Equal, Math.Pow(10, uom[1].Factor - uom[2].Factor), uom[2]); // 1000 мл

            Assert.AreEqual(true, op.ResultFor(m));
        }

        [TestMethod]
        public void CompareEqDiffUomType()
        {
            var m = new Measure(1, uom[1]);
            var op = new MeasureOp(MeasureOperator.Equal, 1, uom[5]);

            Assert.AreEqual(false, op.ResultFor(m));
        }

        [TestMethod]
        public void CompareEqNoUom()
        {
            var m = new Measure(1);
            var op = new MeasureOp(MeasureOperator.Equal, 1);

            Assert.AreEqual(true, op.ResultFor(m));
        }

        [TestMethod]
        public void CompareBetween()
        {
            var m = new Measure(1);
            var op = new MeasureOp(MeasureOperator.Between, 0) { RightValue = 2 };

            Assert.AreEqual(true, op.ResultFor(m));
        }
        [TestMethod]
        public void CompareBetweenRightEq()
        {
            var m = new Measure(1);
            var op = new MeasureOp(MeasureOperator.Between, 0) { RightValue = 1 };

            Assert.AreEqual(true, op.ResultFor(m));
        }
        [TestMethod]
        public void CompareBetweenLeftEq()
        {
            var m = new Measure(1);
            var op = new MeasureOp(MeasureOperator.Between, 1) { RightValue = 2 };

            Assert.AreEqual(false, op.ResultFor(m));
        }
        [TestMethod]
        public void CompareBetweenNotInterval()
        {
            var m = new Measure(1);
            var op = new MeasureOp(MeasureOperator.Between, 1) { RightValue = 1 };

            Assert.AreEqual(true, op.ResultFor(m));
        }
        [TestMethod]
        public void CompareBetweenReverse()
        {
            var m = new Measure(1);
            var op = new MeasureOp(MeasureOperator.Between, 2) { RightValue = 0 };

            Assert.AreEqual(true, op.ResultFor(m));
        }
        #endregion ValueComparer
    }
}