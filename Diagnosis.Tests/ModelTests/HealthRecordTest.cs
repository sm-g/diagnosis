using Diagnosis.Common;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class HealthRecordTest : InMemoryDatabaseTest
    {
        private static Word w1 = new Word("1");
        private static Word w2 = new Word("2");
        private static Word w3 = new Word("3");
        private static Comment com = new Comment("comment");
        private HealthRecord hr1;

        [TestInitialize]
        public void AutocompleteTestInit()
        {
            hrIds.ForAll((id) => hr[id] = session.Get<HealthRecord>(IntToGuid<HealthRecord>(id)));
            uomIds.ForAll((id) => uom[id] = session.Get<Uom>(IntToGuid<Uom>(id)));
            hr1 = hr[71]; // without hios
        }

        [TestMethod]
        public void Unit()
        {
            Assert.AreEqual(HealthRecordUnit.Year, hr[40].Unit);
            Assert.AreEqual(2005, hr[40].FromYear);

            Assert.AreEqual(HealthRecordUnit.Month, hr[20].Unit);
            Assert.AreEqual(2014, hr[20].FromYear);
            Assert.AreEqual(1, hr[20].FromMonth);

            Assert.AreEqual(HealthRecordUnit.NotSet, hr[1].Unit);
            Assert.AreEqual(2013, hr[1].FromYear);
            Assert.AreEqual(11, hr[1].FromMonth);

            Assert.AreEqual(HealthRecordUnit.ByAge, hr[2].Unit);
            Assert.AreEqual(2013, hr[2].FromYear);
            Assert.AreEqual(12, hr[2].FromMonth);
        }

        [TestMethod]
        public void SetItems()
        {
            var hiosSequence = new IHrItemObject[] { w1, w2, com };
            hr1.SetItems(hiosSequence);
            Assert.AreEqual(hiosSequence.Count(), hr1.HrItems.Count);
            Assert.IsTrue(hiosSequence.SequenceEqual(hr1.GetOrderedEntities()));
        }

        [TestMethod]
        public void SetItemsWithRepeat()
        {
            var hiosSequence = new IHrItemObject[] { w1, w1, com };
            hr1.SetItems(hiosSequence);
            Assert.AreEqual(hiosSequence.Count(), hr1.HrItems.Count);
            Assert.IsTrue(hiosSequence.SequenceEqual(hr1.GetOrderedEntities()));
        }

        [TestMethod]
        public void SetItemsAfterReorder()
        {
            var hiosSequence = new IHrItemObject[] { w1, w2, com };
            var hiosSequence2 = new IHrItemObject[] { w1, com, w2 };
            hr1.SetItems(hiosSequence);
            hr1.SetItems(hiosSequence2);

            Assert.AreEqual(hiosSequence2.Count(), hr1.HrItems.Count);
            Assert.IsTrue(hiosSequence2.SequenceEqual(hr1.GetOrderedEntities()));
        }

        [TestMethod]
        public void AddItems()
        {
            var hiosSequence = new IHrItemObject[] { w1, w2, com };
            var hiosToAdd = new IHrItemObject[] { w3, w2 };

            hr1.SetItems(hiosSequence);
            hr1.AddItems(hiosToAdd);

            Assert.AreEqual(hiosSequence.Count() + hiosToAdd.Count(), hr1.HrItems.Count);
            Assert.IsTrue(hiosSequence.Concat(hiosToAdd).SequenceEqual(hr1.GetOrderedEntities()));
        }

        [TestMethod]
        public void AddItemsWithDefaultConfidence()
        {
            var chiosSequence = new IHrItemObject[] { w1, w2, com }
                .Select(x => new ConfindenceHrItemObject(x, Confidence.Absent))
                .ToList();
            var hiosToAdd = new IHrItemObject[] { w3, w2 };

            hr1.SetItems(chiosSequence);
            hr1.AddItems(hiosToAdd);

            var chios = hr1.HrItems.Select(x => x.CHIO).ToList();
            Assert.AreEqual(hiosToAdd.Count(), chios.Count(x => x.Confindence == default(Confidence)));
        }

        [TestMethod]
        public void SetNewConfidence()
        {
            var hiosSequence = new IHrItemObject[] { w1, w2, com };

            hr1.SetItems(hiosSequence);
            var chios = hr1.GetOrderedCHIOs().ToList();
            chios[0].Confindence = Confidence.Absent;

            hr1.SetItems(chios);

            Assert.AreEqual(w1, hr1.HrItems.Single(x => x.Confidence == Confidence.Absent).Entity);
        }

        [TestMethod]
        public void ChangeMeasure()
        {
            var m = new Measure(0, uom[1]) { Word = w1 };
            var hiosSequence = new IHrItemObject[] { m };

            hr1.SetItems(hiosSequence);
            var m2 = new Measure(0, uom[2]) { Word = w1 };
            hr1.SetItems(new IHrItemObject[] { m2 });
            Assert.AreEqual(uom[2], hr1.HrItems.First().Measure.Uom);
        }
    }
}