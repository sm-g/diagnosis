using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.Linq;

namespace Diagnosis.Models.Tests
{
    [TestClass]
    public class HealthRecordTest : InMemoryDatabaseTest
    {
        private static Word w1;
        private static Word w2;
        private static Word w3;
        private static Comment com = new Comment("comment");
        private HealthRecord hr1;

        [TestInitialize]
        public void AutocompleteTestInit()
        {
            Load<Uom>();
            hr1 = session.Load<HealthRecord>(IntToGuid<HealthRecord>(71));
            w1 = session.Get<Word>(IntToGuid<Word>(1));
            w2 = session.Get<Word>(IntToGuid<Word>(2));
            w3 = session.Get<Word>(IntToGuid<Word>(3));

        }

        [TestMethod]
        public void DataConditions()
        {
            Assert.IsTrue(hr1.HrItems.Count() == 0);
        }

        [TestMethod]
        public void Unit()
        {
            Load<HealthRecord>();
            Assert.AreEqual(HealthRecordUnit.Year, hr[40].Unit);
            Assert.AreEqual(2005, hr[40].FromDate.Year);

            Assert.AreEqual(HealthRecordUnit.Month, hr[20].Unit);
            Assert.AreEqual(2014, hr[20].FromDate.Year);
            Assert.AreEqual(1, hr[20].FromDate.Month);

            Assert.AreEqual(HealthRecordUnit.NotSet, hr[1].Unit);
            Assert.AreEqual(2013, hr[1].FromDate.Year);
            Assert.AreEqual(11, hr[1].FromDate.Month);

            Assert.AreEqual(HealthRecordUnit.ByAge, hr[2].Unit);
            Assert.AreEqual(2013, hr[2].FromDate.Year);
            Assert.AreEqual(12, hr[2].FromDate.Month);
        }

        [TestMethod]
        public void SetItems()
        {
            var hiosSequence = new IHrItemObject[] { w1, w2, com };
            hr1.SetItems(hiosSequence);
            Assert.AreEqual(hiosSequence.Count(), hr1.HrItems.Count());
            Assert.IsTrue(hiosSequence.SequenceEqual(hr1.GetOrderedEntities()));
        }

        [TestMethod]
        public void SetItemsWithRepeat()
        {
            var hiosSequence = new IHrItemObject[] { w1, w1, com };
            hr1.SetItems(hiosSequence);
            Assert.AreEqual(hiosSequence.Count(), hr1.HrItems.Count());
            Assert.IsTrue(hiosSequence.SequenceEqual(hr1.GetOrderedEntities()));
        }

        [TestMethod]
        public void SetItemsAfterReorder()
        {
            var hiosSequence = new IHrItemObject[] { w1, w2, com };
            var hiosSequence2 = new IHrItemObject[] { w1, com, w2 };
            hr1.SetItems(hiosSequence);

            bool reordered = false;
            hr1.ItemsChanged += (s, e) =>
            {
                reordered = true;
            };
            hr1.SetItems(hiosSequence2);

            Assert.IsTrue(reordered);
            Assert.AreEqual(hiosSequence2.Count(), hr1.HrItems.Count());
            Assert.IsTrue(hiosSequence2.SequenceEqual(hr1.GetOrderedEntities()));
        }

        [TestMethod]
        public void AddItems()
        {
            var hiosSequence = new IHrItemObject[] { w1, w2, com };
            var hiosToAdd = new IHrItemObject[] { w3, w2 };

            hr1.SetItems(hiosSequence);
            bool changed = false;
            hr1.ItemsChanged += (s, e) =>
            {
                changed = true;
            };
            hr1.AddItems(hiosToAdd);

            Assert.IsTrue(changed);
            Assert.AreEqual(hiosSequence.Count() + hiosToAdd.Count(), hr1.HrItems.Count());
            Assert.IsTrue(hiosSequence.Concat(hiosToAdd).SequenceEqual(hr1.GetOrderedEntities()));
        }

        [TestMethod]
        public void SetSameItems()
        {
            var hiosSequence = new IHrItemObject[] { w1, w2, com };
            var hiosSequence2 = new IHrItemObject[] { w1, w2, com };
            hr1.SetItems(hiosSequence);

            bool changed = false;
            hr1.ItemsChanged += (s, e) =>
            {
                changed = true;
            };
            hr1.SetItems(hiosSequence2);

            Assert.IsFalse(changed);
        }

        [TestMethod]
        public void ChangeWordToComment()
        {
            // OrderedBag.Difference был с тем же самымм словом, из-за Proxy
            // (c1, w2).Difference(w1, w2) == (c1, w2)
            var hiosSequence = new IHrItemObject[] { w1, w2 };
            var hiosSequence2 = new IHrItemObject[] { new Comment(w1.Title), w2 };
            hr1.SetItems(hiosSequence);
            hr1.SetItems(hiosSequence2);

            Assert.AreEqual(1, hr1.GetOrderedEntities().OfType<Comment>().Count());
            Assert.AreEqual(1, hr1.GetOrderedEntities().OfType<Word>().Count());
        }

        [TestMethod]
        public void RemoveOneOfDoulbeWords()
        {
            var hiosSequence = new IHrItemObject[] { w1, w1, com };
            var hiosSequence2 = new IHrItemObject[] { w1, com };

            hr1.SetItems(hiosSequence);
            hr1.SetItems(hiosSequence2);

            Assert.AreEqual(2, hr1.HrItems.Count());
            Assert.IsTrue(hr1.GetOrderedEntities().Contains(w1));
        }

        [TestMethod]
        public void AddItemsWithDefaultConfidence()
        {
            var chiosSequence = new IHrItemObject[] { w1, w2, com }
                .Select(x => new ConfindenceWithHrItemObject(x, Confidence.Absent))
                .ToList();
            var hiosToAdd = new IHrItemObject[] { w3, w2 };

            hr1.SetItems(chiosSequence);
            hr1.AddItems(hiosToAdd);

            var chios = hr1.HrItems.Select(x => x.GetConfindenceHrItemObject()).ToList();
            Assert.AreEqual(hiosToAdd.Count(), chios.Count(x => x.Confidence == default(Confidence)));
        }

        [TestMethod]
        public void SetNewConfidence()
        {
            var hiosSequence = new IHrItemObject[] { w1, w2, com };

            hr1.SetItems(hiosSequence);
            var chios = hr1.GetOrderedCHIOs().ToList();
            chios[0].Confidence = Confidence.Absent;

            hr1.SetItems(chios);

            Assert.AreEqual(w1, hr1.HrItems.Single(x => x.Confidence == Confidence.Absent).Entity);
        }

        [TestMethod]
        public void ChangeMeasureUom()
        {
            var m = new Measure(0, uom[1]) { Word = w1 };
            var hiosSequence = new IHrItemObject[] { m };

            hr1.SetItems(hiosSequence);
            var m2 = new Measure(0, uom[2]) { Word = w1 };
            hr1.SetItems(new IHrItemObject[] { m2 });
            Assert.AreEqual(uom[2], hr1.HrItems.First().Measure.Uom);
        }

        [TestMethod]
        public void ClearFromDate_ToNotEmpty()
        {
            hr1.FromDate.Year = 2010;
            hr1.ToDate.Year = 2010;

            hr1.FromDate.Year = null;

            Assert.AreEqual(false, hr1.ToDate.IsEmpty);
        }

        [TestMethod]
        public void SetDateNowChangesDescribedAt()
        {
            var d = new DateTime(2000, 1, 1);
            hr1.FromDate.Now = d;

            Assert.AreEqual(d, hr1.DescribedAt);
        }

        [TestMethod]
        public void ChangeHrDate_HrIsDirty()
        {
            var hr = session.Load<HealthRecord>(IntToGuid<HealthRecord>(1));
            (hr as IEditableObject).BeginEdit();
            hr.FromDate.Year = 2000;
            Assert.IsTrue(hr.IsDirty);
        }
    }
}