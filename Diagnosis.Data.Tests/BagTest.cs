using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Wintellect.PowerCollections;

namespace Diagnosis.Data.Tests
{
    [TestClass]
    public class BagTest : InMemoryDatabaseTest
    {
        [TestInitialize]
        public void Init()
        {
            Load<Word>();
            Load<IcdDisease>();
            Load<Uom>();
        }

        [TestMethod]
        public void WordComments()
        {
            var c1 = new Comment("1");
            var w1 = new Word("1");
            var bag1 = new Bag<IHrItemObject>(new List<IHrItemObject> { w1, c1 });
            var bag2 = new Bag<IHrItemObject>(new[] { w1 });

            var diff1 = bag1.Difference(bag2);
            var diff2 = bag2.Difference(bag1);
            Assert.IsTrue(diff1.Single() == c1);
            Assert.IsTrue(diff2.Count == 0);
        }

        [TestMethod]
        public void SameComment()
        {
            var w1 = new Word("1");
            var c1 = new Comment("1");
            var c2 = new Comment("1");
            var bag1 = new Bag<IHrItemObject>(new List<IHrItemObject> { w[1], w[2], w[2], c1 });
            var bag2 = new Bag<IHrItemObject>(new List<IHrItemObject> { c2, w[1], w1 });

            var diff1 = bag1.Difference(bag2);
            var diff2 = bag2.Difference(bag1);
            Assert.IsTrue(diff1.Count == 2);
            Assert.IsTrue(diff2.Single() as Word == w1);
        }

        [TestMethod]
        public void Words()
        {
            var bag1 = new Bag<IHrItemObject>(new[] { w[1], w[2], w[3], w[3] });
            var bag2 = new Bag<IHrItemObject>(new[] { w[2], w[2], w[3] });

            var diff1 = bag1.Difference(bag2);
            var diff2 = bag2.Difference(bag1);
            Assert.IsTrue(diff1.Count == 2);
            Assert.IsTrue(diff2.Single() as Word == w[2]);
        }
        [TestMethod]
        public void CHIOs()
        {
            var chio1 = new ConfWithHio(w[1], Confidence.Present);
            var chio2 = new ConfWithHio(w[2], Confidence.Present);
            var chio1comment = new ConfWithHio(new Comment(w[1].Title), Confidence.Present);

            var bag1 = new Bag<ConfWithHio>(new[] { chio1, chio2 });
            var bag2 = new Bag<ConfWithHio>(new[] { chio1comment, chio2 });

            var diff1 = bag1.Difference(bag2);
            var diff2 = bag2.Difference(bag1);
            Assert.IsTrue(diff1.Single() == chio1);
            Assert.IsTrue(diff2.Single() == chio1comment);
        }
        [TestMethod]
        public void Transient()
        {
            var w1 = new Word("1");
            var w2 = new Word("2");
            var w3 = new Word("3");
            var bag1 = new Bag<IHrItemObject>(new[] { w1, w2, w3, w3 });
            var bag2 = new Bag<IHrItemObject>(new[] { w2, w2, w3 });

            var diff1 = bag1.Difference(bag2);
            var diff2 = bag2.Difference(bag1);
            Assert.IsTrue(diff1.Count == 2);
            Assert.IsTrue(diff2.Single() as Word == w2);
        }

        [TestMethod]
        public void SameIds()
        {
            var bag1 = new Bag<IHrItemObject>(new List<IHrItemObject> { w[1], icd[2], w[2], w[3], w[3] });
            var bag2 = new Bag<IHrItemObject>(new List<IHrItemObject> { w[2], w[2], icd[3] });

            var diff1 = bag1.Difference(bag2);
            var diff2 = bag2.Difference(bag1);
            Assert.IsTrue(diff1.Count == 4);
            Assert.IsTrue(diff2.Count == 2);
        }

        [TestMethod]
        public void MeasureSameUomType()
        {
            var m = new Measure(0, uom[1]);
            var m2 = new Measure(0, uom[2]);
            var bag1 = new Bag<IHrItemObject>(new[] { m, m2 });
            var bag2 = new Bag<IHrItemObject>(new[] { m });

            var diff1 = bag1.Difference(bag2);
            var diff2 = bag2.Difference(bag1);
            Assert.IsTrue(diff1.Single() as Measure == m2);
            Assert.IsTrue(diff2.Count == 0);
        }
    }

    [TestClass]
    public class DifferenceWithTest : InMemoryDatabaseTest
    {
        [TestInitialize]
        public void Init()
        {
            Load<Word>();
            Load<IcdDisease>();
            Load<Uom>();
        }

        [TestMethod]
        public void WordComments()
        {
            var c1 = new Comment("1");
            var w1 = new Word("1");
            var bag1 = new List<IHrItemObject> { w1, c1 };
            var bag2 = new[] { w1 };

            var diff1 = bag1.DifferenceWith(bag2);
            var diff2 = bag2.DifferenceWith(bag1);
            Assert.IsTrue(diff1.Single() == c1);
            Assert.IsTrue(diff2.Count() == 0);
        }

        [TestMethod]
        public void SameComment()
        {
            var w1 = new Word("1");
            var c1 = new Comment("1");
            var c2 = new Comment("1");
            var bag1 = new List<IHrItemObject> { w[1], w[2], w[2], c1 };
            var bag2 = new List<IHrItemObject> { c2, w[1], w1 };

            var diff1 = bag1.DifferenceWith(bag2);
            var diff2 = bag2.DifferenceWith(bag1);
            Assert.IsTrue(diff1.Count() == 2);
            Assert.IsTrue(diff2.Single() as Word == w1);
        }

        [TestMethod]
        public void Words()
        {
            var bag1 = new[] { w[1], w[2], w[3], w[3] };
            var bag2 = new[] { w[2], w[2], w[3] };

            var diff1 = bag1.DifferenceWith(bag2);
            var diff2 = bag2.DifferenceWith(bag1);
            Assert.IsTrue(diff1.Count() == 2);
            Assert.IsTrue(diff2.Single() as Word == w[2]);
        }
        [TestMethod]
        public void CHIOs()
        {
            var chio1 = new ConfWithHio(w[1], Confidence.Present);
            var chio2 = new ConfWithHio(w[2], Confidence.Present);
            var chio1comment = new ConfWithHio(new Comment(w[1].Title), Confidence.Present);

            var bag1 = new[] { chio1, chio2 };
            var bag2 = new[] { chio1comment, chio2 };

            var diff1 = bag1.DifferenceWith(bag2);
            var diff2 = bag2.DifferenceWith(bag1);
            Assert.IsTrue(diff1.Single() == chio1);
            Assert.IsTrue(diff2.Single() == chio1comment);
        }
        [TestMethod]
        public void Transient()
        {
            var w1 = new Word("1");
            var w2 = new Word("2");
            var w3 = new Word("3");
            var bag1 = new[] { w1, w2, w3, w3 };
            var bag2 = new[] { w2, w2, w3 };

            var diff1 = bag1.DifferenceWith(bag2);
            var diff2 = bag2.DifferenceWith(bag1);
            Assert.IsTrue(diff1.Count() == 2);
            Assert.IsTrue(diff2.Single() as Word == w2);
        }

        [TestMethod]
        public void SameIds()
        {
            var bag1 = new List<IHrItemObject> { w[1], icd[2], w[2], w[3], w[3] };
            var bag2 = new List<IHrItemObject> { w[2], w[2], icd[3] };

            var diff1 = bag1.DifferenceWith(bag2);
            var diff2 = bag2.DifferenceWith(bag1);
            Assert.IsTrue(diff1.Count() == 4);
            Assert.IsTrue(diff2.Count() == 2);
        }

        [TestMethod]
        public void MeasureSameUomType()
        {
            var m = new Measure(0, uom[1]);
            var m2 = new Measure(0, uom[2]);
            var bag1 = new[] { m, m2 };
            var bag2 = new[] { m };

            var diff1 = bag1.DifferenceWith(bag2);
            var diff2 = bag2.DifferenceWith(bag1);
            Assert.IsTrue(diff1.Single() as Measure == m2);
            Assert.IsTrue(diff2.Count() == 0);
        }
    }
}