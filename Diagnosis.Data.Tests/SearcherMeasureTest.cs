using Diagnosis.Common;
using Diagnosis.Data.Search;
using Diagnosis.Models;
using Diagnosis.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.Data.Tests
{
    [TestClass]
    public class SearcherMeasureTest : InMemoryDatabaseTest
    {
        private SearchOptions o;
        private int hrsTotal;

        [TestInitialize]
        public void Init()
        {
            Load<HealthRecord>();
            Load<Word>();
            Load<Appointment>();
            Load<Uom>();

            o = new SearchOptions(true);
            Searcher.logOn = true;
            hrsTotal = hr.Count;
        }

        [TestCleanup]
        public void Clean()
        {
            Searcher.logOn = false;
        }

        [TestMethod]
        public void MeasureAndWord()
        {
            var hrs = o
                .SetAll(new MeasureOp(MeasureOperator.Equal, 0.05, uom[1]) { Word = w[3] }, w[1])
                .Search(session);

            Assert.AreEqual(hr[22], hrs.Single());
        }

        [TestMethod]
        public void MeasureOrWord()
        {
            var hrs = o
                .SetAny(new MeasureOp(MeasureOperator.Equal, 0.05, uom[1]) { Word = w[3] }, w[5])
                .Search(session);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[30]));
        }

        [TestMethod]
        public void AnyMeasureWordsNot()
        {
            var hrs = o
                .SetAny(new MeasureOp(MeasureOperator.GreaterOrEqual, 0.05, uom[1]) { Word = w[3] })
                .SetNot(w[22])
                .Search(session);

            Assert.AreEqual(1, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[20]));
        }

        [TestMethod]
        public void MeasureAndWordOrWords()
        {
            var hrs = o
                .SetAll(new MeasureOp(MeasureOperator.GreaterOrEqual, 0.05, uom[1]) { Word = w[3] }, w[1])
                .SetAny(w[22], w[94])
                .Search(session);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[20]));
        }

        [TestMethod]
        public void AnyMeasure()
        {
            var hrs = o
                .SetAny(new MeasureOp(MeasureOperator.Equal, 0.05, uom[1]) { Word = w[3] })
                .Search(session);

            Assert.AreEqual(1, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[22]));
        }

        [TestMethod]
        public void MeasureAndWord_SameWord_UseOne()
        {
            // все элементы подошли к записи - достаточно одного измерения
            var hrs = o
               .SetAll(w[3], new MeasureOp(MeasureOperator.Equal, 0.05, uom[1]) { Word = w[3] })
               .Search(session);

            Assert.AreEqual(1, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[22]));
        }

        [TestMethod]
        public void MinAnyTwoMeasureAndWord()
        {
            // хотя бы 2 = все
            var hrs = o
                .SetAny(w[22], new MeasureOp(MeasureOperator.GreaterOrEqual, 0.05, uom[1]) { Word = w[3] })
                .MinAny(2)
                .Search(session);

            Assert.AreEqual(1, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[22]));
        }
        [TestMethod]
        public void MinAnyTwo_SameMeasuresAndWord_UseOne()
        {
            // хотя бы 2 (22, 3>0.05, 3>0.06)
            var hrs = o
                .SetAny(w[22],
                    new MeasureOp(MeasureOperator.GreaterOrEqual, 0.05, uom[1]) { Word = w[3] },
                    new MeasureOp(MeasureOperator.GreaterOrEqual, 0.06, uom[1]) { Word = w[3] })
                .MinAny(2)
                .Search(session);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[22]));
        }
        [TestMethod]
        public void AllMeasure_AnyMeasure_Same_UseOne()
        {
            // к записи подошли все элементы и хотя бы 1 из элементов. a & a = a
            var hrs = o
                .SetAll(new MeasureOp(MeasureOperator.Equal, 0.05, uom[1]) { Word = w[3] })
                .SetAny(new MeasureOp(MeasureOperator.Equal, 0.05, uom[1]) { Word = w[3] })
                .Search(session);

            Assert.AreEqual(1, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[22]));
        }

        [TestMethod]
        public void AllMeasures_SameWord_UseOne()
        {
            // все элементы подошли к записи, достаточно одного измерения
            var hrs = o
               .SetAll(
                   new MeasureOp(MeasureOperator.GreaterOrEqual, 0.05, uom[1]) { Word = w[3] },
                   new MeasureOp(MeasureOperator.GreaterOrEqual, 0.06, uom[1]) { Word = w[3] })
               .Search(session);

            Assert.AreEqual(1, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[20]));
        }
        [TestMethod]
        public void MinAnyTwo_SameMeasures_UseOne()
        {
            // все элементы подошли к записи, достаточно одного измерения
            var hrs = o
                .SetAny(
                    new MeasureOp(MeasureOperator.GreaterOrEqual, 0.05, uom[1]) { Word = w[3] },
                    new MeasureOp(MeasureOperator.GreaterOrEqual, 0.06, uom[1]) { Word = w[3] })
                .MinAny(2)
                .Search(session);

            Assert.AreEqual(1, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[20]));
        }
        [TestMethod]
        public void AnyTwoMeasures_SameWord_NotOverlap()
        {
            var hrs = o
               .MinAny(2)
               .SetAny(
                   new MeasureOp(MeasureOperator.Less, 0.05, uom[1]) { Word = w[3] },
                   new MeasureOp(MeasureOperator.GreaterOrEqual, 0.06, uom[1]) { Word = w[3] })
               .Search(session);

            Assert.AreEqual(0, hrs.Count());
        }

        [TestMethod]
        public void AllMeasures_SameWord_NotOverlap()
        {
            var hrs = o
               .SetAll(
                   new MeasureOp(MeasureOperator.Less, 0.05, uom[1]) { Word = w[3] },
                   new MeasureOp(MeasureOperator.GreaterOrEqual, 0.06, uom[1]) { Word = w[3] })
               .Search(session);

            Assert.AreEqual(0, hrs.Count());
        }
    }
}