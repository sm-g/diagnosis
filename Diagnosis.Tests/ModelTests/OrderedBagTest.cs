using Diagnosis.Common;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Wintellect.PowerCollections;

namespace Tests.ModelTests
{
    [TestClass]
    public class OrderedBagTest : InMemoryDatabaseTest
    {
        [TestInitialize]
        public void Init()
        {
            wIds.ForAll((id) => w[id] = session.Get<Word>(IntToGuid<Word>(id)));
            icdIds.ForAll((id) => icd[id] = session.Get<IcdDisease>(id));
            uomIds.ForAll((id) => uom[id] = session.Get<Uom>(IntToGuid<Uom>(id)));
        }

        [TestMethod]
        public void WordComments()
        {
            var bag1 = new OrderedBag<IHrItemObject>();
            var bag2 = new OrderedBag<IHrItemObject>();
            var c1 = new Comment("1");
            var w1 = new Word("1");
            bag1.AddMany(new List<IHrItemObject> { w1, c1 });
            bag2.AddMany(new[] { w1 });

            var diff1 = bag1.Difference(bag2);
            var diff2 = bag2.Difference(bag1);
            Assert.IsTrue(diff1.Count == 1);
            Assert.IsTrue(diff2.Count == 0);
        }

        [TestMethod]
        public void SameComment()
        {
            var bag1 = new OrderedBag<IHrItemObject>();
            var bag2 = new OrderedBag<IHrItemObject>();
            var w1 = new Word("1");
            var c1 = new Comment("1");
            var c2 = new Comment("1");
            bag1.AddMany(new List<IHrItemObject> { w[1], w[2], w[2], c1 });
            bag2.AddMany(new List<IHrItemObject> { c2, w[1], w1 });

            var diff1 = bag1.Difference(bag2);
            var diff2 = bag2.Difference(bag1);
            Assert.IsTrue(diff1.Count == 2);
            Assert.IsTrue(diff2.Count == 1);
        }

        [TestMethod]
        public void Words()
        {
            var bag1 = new OrderedBag<IHrItemObject>();
            var bag2 = new OrderedBag<IHrItemObject>();
            bag1.AddMany(new[] { w[1], w[2], w[3], w[3] });
            bag2.AddMany(new[] { w[2], w[2], w[3] });

            var diff1 = bag1.Difference(bag2);
            var diff2 = bag2.Difference(bag1);
            Assert.IsTrue(diff1.Count == 2);
            Assert.IsTrue(diff2.Count == 1);
        }

        [TestMethod]
        public void Transient()
        {
            var bag1 = new OrderedBag<IHrItemObject>();
            var bag2 = new OrderedBag<IHrItemObject>();
            var w1 = new Word("1");
            var w2 = new Word("2");
            var w3 = new Word("3");
            bag1.AddMany(new[] { w1, w2, w3, w3 });
            bag2.AddMany(new[] { w2, w2, w3 });

            var diff1 = bag1.Difference(bag2);
            var diff2 = bag2.Difference(bag1);
            Assert.IsTrue(diff1.Count == 2);
            Assert.IsTrue(diff2.Count == 1);
        }

        [TestMethod]
        public void SameIds()
        {
            var bag1 = new OrderedBag<IHrItemObject>();
            var bag2 = new OrderedBag<IHrItemObject>();
            bag1.AddMany(new List<IHrItemObject> { w[1], icd[2], w[2], w[3], w[3] });
            bag2.AddMany(new List<IHrItemObject> { w[2], w[2], icd[3] });

            var diff1 = bag1.Difference(bag2);
            var diff2 = bag2.Difference(bag1);
            Assert.IsTrue(diff1.Count == 4);
            Assert.IsTrue(diff2.Count == 2);
        }

        [TestMethod]
        public void MeasureSameUomType()
        {
            var bag1 = new OrderedBag<IHrItemObject>();
            var bag2 = new OrderedBag<IHrItemObject>();
            var m = new Measure(0, uom[1]);
            var m2 = new Measure(0, uom[2]);

            bag1.AddMany(new List<IHrItemObject> { m, m2 });
            bag2.AddMany(new List<IHrItemObject> { m });

            var diff1 = bag1.Difference(bag2);
            var diff2 = bag2.Difference(bag1);
            Assert.IsTrue(diff1.Count == 1);
            Assert.IsTrue(diff2.Count == 0);
            Assert.IsTrue(diff1.Contains(m2));

        }
    }
}