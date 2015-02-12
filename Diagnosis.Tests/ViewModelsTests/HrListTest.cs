using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.Models.Enums;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class HrListTest : InMemoryDatabaseTest
    {
        private Dictionary<int, Patient> p = new Dictionary<int, Patient>();
        private Dictionary<int, Course> c = new Dictionary<int, Course>();
        private Dictionary<int, HrCategory> cat = new Dictionary<int, HrCategory>();
        private Dictionary<int, Appointment> a = new Dictionary<int, Appointment>();
        private Dictionary<int, HealthRecord> hr = new Dictionary<int, HealthRecord>();

        private Doctor d1;

        private static int[] pIds = new[] { 1, 2, 3, 4, 5 };
        private static int[] cIds = new[] { 1, 2, 3, 4 };
        private static int[] catIds = new[] { 1, 2, 3, 4, 5, 6 };
        private static int[] aIds = new[] { 1, 2, 3, 4, 5 };
        private static int[] hrIds = new[] { 1, 2, 20, 21, 22, 30, 31, 32, 40, 70, 71, 72, 73, 74 };

        [TestInitialize]
        public void Init()
        {
            d1 = session.Get<Doctor>(IntToGuid<Doctor>(1));
            AuthorityController.LogIn(d1);

            pIds.ForAll((id) => p[id] = session.Get<Patient>(IntToGuid<Patient>(id)));
            cIds.ForAll((id) => c[id] = session.Get<Course>(IntToGuid<Course>(id)));
            cIds.ForAll((id) => cat[id] = session.Get<HrCategory>(IntToGuid<HrCategory>(id)));
            aIds.ForAll((id) => a[id] = session.Get<Appointment>(IntToGuid<Appointment>(id)));
            hrIds.ForAll((id) => hr[id] = session.Get<HealthRecord>(IntToGuid<HealthRecord>(id)));

            // a[2] with hrs 20,21,22
        }

        #region Selection

        [TestMethod]
        public void SelectHr()
        {
            using (var card = new CardViewModel(a[2], true))
            {
                card.HrList.SelectHealthRecord(hr[20]);

                Assert.AreEqual(hr[20], card.HrList.SelectedHealthRecord.healthRecord);
            }
        }
        [TestMethod]
        public void SelectNotInList()
        {
            using (var card = new CardViewModel(a[2], true))
            {
                card.HrList.SelectHealthRecord(hr[1]);

                Assert.AreEqual(null, card.HrList.SelectedHealthRecord);
                Assert.AreEqual(0, card.HrList.SelectedHealthRecords.Count());
            }
        }
        [TestMethod]
        public void DeselectHr()
        {
            using (var card = new CardViewModel(a[2], true))
            {
                card.HrList.SelectHealthRecord(hr[20]);
                card.HrList.SelectedHealthRecord = null;

                Assert.AreEqual(null, card.HrList.SelectedHealthRecord);
            }
        }
        [TestMethod]
        public void DeselectWhenManySelected()
        {
            using (var card = new CardViewModel(a[2], true))
            {
                card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });
                card.HrList.SelectedHealthRecord = null;

                Assert.AreEqual(null, card.HrList.SelectedHealthRecord);
                Assert.AreEqual(0, card.HrList.SelectedHealthRecords.Count());
            }
        }
        [TestMethod]
        public void SelectManySelectedLast()
        {
            using (var card = new CardViewModel(a[2], true))
            {
                card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });

                Assert.AreEqual(hr[21], card.HrList.SelectedHealthRecord.healthRecord);
                Assert.AreEqual(2, card.HrList.SelectedHealthRecords.Count());
            }
        }

        [TestMethod]
        public void SelectOneThenMany()
        {
            using (var card = new CardViewModel(a[2], true))
            {
                card.HrList.SelectHealthRecord(hr[22]);
                card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });

                Assert.AreEqual(hr[21], card.HrList.SelectedHealthRecord.healthRecord);
                Assert.AreEqual(2, card.HrList.SelectedHealthRecords.Count());
            }
        }
        [TestMethod]
        public void SelectManyThenOne()
        {
            using (var card = new CardViewModel(a[2], true))
            {
                card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });
                card.HrList.SelectHealthRecord(hr[21]);

                Assert.AreEqual(hr[21], card.HrList.SelectedHealthRecord.healthRecord);
                Assert.AreEqual(1, card.HrList.SelectedHealthRecords.Count());
            }
        }
        [TestMethod]
        public void SelectManyThenChangeSelected()
        {
            using (var card = new CardViewModel(a[2], true))
            {
                card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });
                card.HrList.SelectHealthRecord(hr[20], true);
                Assert.AreEqual(hr[20], card.HrList.SelectedHealthRecord.healthRecord);
                Assert.AreEqual(2, card.HrList.SelectedHealthRecords.Count());
            }
        }

        [TestMethod]
        public void AddToSelected()
        {
            using (var card = new CardViewModel(a[2], true))
            {
                card.HrList.SelectHealthRecord(hr[20], true);
                card.HrList.SelectHealthRecord(hr[21], true);

                Assert.AreEqual(hr[21], card.HrList.SelectedHealthRecord.healthRecord);
                Assert.AreEqual(2, card.HrList.SelectedHealthRecords.Count());
            }
        }
        [TestMethod]
        public void AddToSelectedNotInList()
        {
            using (var card = new CardViewModel(a[2], true))
            {
                card.HrList.SelectHealthRecord(hr[20], true);
                card.HrList.SelectHealthRecord(hr[1], true);

                Assert.AreEqual(hr[20], card.HrList.SelectedHealthRecord.healthRecord);
                Assert.AreEqual(1, card.HrList.SelectedHealthRecords.Count());
            }
        }
        [TestMethod]
        public void SelectOneByOne()
        {
            using (var card = new CardViewModel(a[2], true))
            {
                card.HrList.SelectHealthRecord(hr[20]);
                card.HrList.SelectHealthRecord(hr[21]);

                Assert.AreEqual(hr[21], card.HrList.SelectedHealthRecord.healthRecord);
                Assert.AreEqual(1, card.HrList.SelectedHealthRecords.Count());
            }
        }

        [TestMethod]
        public void MoveHrSelection()
        {
            using (var card = new CardViewModel(a[2], true))
            {
                card.HrList.SelectHealthRecord(hr[20]);
                card.HrList.MoveHrSelectionCommand.Execute(true);

                Assert.AreNotEqual(hr[20], card.HrList.SelectedHealthRecord.healthRecord);
                Assert.AreEqual(1, card.HrList.SelectedHealthRecords.Count());
            }
        }

        [TestMethod]
        public void MoveEmptyHrSelection()
        {
            using (var card = new CardViewModel(a[2], true))
            {
                Assert.AreEqual(null, card.HrList.SelectedHealthRecord);

                card.HrList.MoveHrSelectionCommand.Execute(true);

                Assert.AreNotEqual(null, card.HrList.SelectedHealthRecord);
            }
        }
        [TestMethod]
        public void MoveHrSelectionMany()
        {
            using (var card = new CardViewModel(a[2], true))
            {
                card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });
                card.HrList.Grouping = HrViewGroupingColumn.None;
                card.HrList.MoveHrSelectionCommand.Execute(true); // up to 20

                Assert.AreEqual(hr[20], card.HrList.SelectedHealthRecord.healthRecord);
                Assert.AreEqual(1, card.HrList.SelectedHealthRecords.Count());
            }
        }
        [TestMethod]
        public void MoveHrSelectionManySorted()
        {
            using (var card = new CardViewModel(a[5], true))
            {
                card.HrList.AddHealthRecordCommand.Execute(null);
                var hr0 = card.HrList.HealthRecords.Last();
                card.HrList.AddHealthRecordCommand.Execute(null);
                var hr1 = card.HrList.HealthRecords.Last();
                card.HrList.AddHealthRecordCommand.Execute(null);
                var hr2 = card.HrList.HealthRecords.Last();

                card.HrList.Sorting = HrViewSortingColumn.CreatedAt;
                card.HrList.Grouping = HrViewGroupingColumn.Category;

                // 1 cat
                // 0
                // 2
                // 2 cat
                // 1

                hr0.healthRecord.Category = cat[1];
                hr1.healthRecord.Category = cat[2];
                hr2.healthRecord.Category = cat[1];

                card.HrList.SelectedHealthRecord = hr0;
                card.HrList.MoveHrSelectionCommand.Execute(true); // up

                Assert.AreEqual(hr1, card.HrList.SelectedHealthRecord);
            }
        }

        #endregion

        [TestMethod]
        public void CopyPasteHr()
        {
            using (var card = new CardViewModel(a[2], true))
            {

                card.HrList.SelectHealthRecord(hr[20]);
                card.HrList.Copy();
                card.HrList.Paste();

                Assert.AreEqual(4, card.HrList.HealthRecords.Count);
            }
        }

        [TestMethod]
        public void CutDeleteNewWordPaste()
        {
            using (var card = new CardViewModel(a[2], true))
            {
                var w = new Word("11");

                card.HrList.AddHealthRecordCommand.Execute(null);
                card.HrEditor.Autocomplete.AddTag(w);
                card.ToogleHrEditor();

                Assert.IsTrue(!w.IsTransient);

                card.HrList.Cut();

                Assert.AreEqual(0, w.HealthRecords.Count());

                new Saver(session).Delete(w);

                card.HrList.Paste();

                var newWord = card.HrList.SelectedHealthRecord.healthRecord.HrItems.Single().Word;
                // word recreated and saved with same title
                Assert.AreEqual(0, newWord.CompareTo(w));
                Assert.IsTrue(!newWord.IsTransient);
            }
        }
        [TestMethod]
        public void NoSelectedAfterCut()
        {
            using (var card = new CardViewModel(a[2], true))
            {
                var count = card.HrList.HealthRecords.Count;

                card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });
                card.HrList.Cut();

                Assert.AreEqual(null, card.HrList.SelectedHealthRecord);
                Assert.AreEqual(0, card.HrList.SelectedHealthRecords.Count());
            }
        }
        [TestMethod]
        public void CutPasteHrs()
        {
            using (var card = new CardViewModel(a[2], true))
            {
                var count = card.HrList.HealthRecords.Count;

                card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });
                card.HrList.Cut();
                card.HrList.Paste();

                Assert.AreEqual(count, card.HrList.HealthRecords.Count);
                Assert.AreEqual(2, card.HrList.SelectedHealthRecords.Count());
            }
        }
        [TestMethod]
        public void PastedHrEqual()
        {
            using (var card = new CardViewModel(a[2], true))
            {
                var count = card.HrList.HealthRecords.Count;

                card.HrList.SelectHealthRecord(hr[20]);
                card.HrList.Cut();
                card.HrList.Paste();

                var new20 = card.HrList.SelectedHealthRecords.First().healthRecord;

                Assert.AreEqual(hr[20].Appointment, new20.Appointment);
                Assert.AreEqual(hr[20].Category, new20.Category);
                Assert.AreEqual(hr[20].Course, new20.Course);
                Assert.AreEqual(hr[20].FromDay, new20.FromDay);
                Assert.AreEqual(hr[20].FromMonth, new20.FromMonth);
                Assert.AreEqual(hr[20].FromYear, new20.FromYear);
                Assert.AreEqual(hr[20].Patient, new20.Patient);
                Assert.AreEqual(hr[20].Unit, new20.Unit);

                // Assert.AreEqual(hr[20].Ord, new20.Ord); в любое место

                Assert.AreEqual(false, new20.IsDeleted);
                Assert.AreEqual(false, new20.IsDirty);

                Assert.AreEqual(hr[20].Doctor, new20.Doctor); // или новый автор?
                Assert.AreNotEqual(hr[20].CreatedAt, new20.CreatedAt);
                // Assert.AreNotEqual(hr[20].UpdatedAt, new20.UpdatedAt);
            }
        }
        [TestMethod]
        public void Reorder()
        {
            using (var card = new CardViewModel(a[5], true))
            {
                card.HrList.AddHealthRecordCommand.Execute(null);
                var hr0 = card.HrList.HealthRecords.Last();
                hr0.healthRecord.AddItems(new Comment("a").ToEnumerable());
                card.HrList.AddHealthRecordCommand.Execute(null);
                var hr1 = card.HrList.HealthRecords.Last();
                hr1.healthRecord.AddItems(new Comment("b").ToEnumerable());
                card.HrList.AddHealthRecordCommand.Execute(null);
                var hr2 = card.HrList.HealthRecords.Last();
                hr2.healthRecord.AddItems(new Comment("c").ToEnumerable());
                card.HrList.AddHealthRecordCommand.Execute(null);
                var hr3 = card.HrList.HealthRecords.Last();
                hr3.healthRecord.AddItems(new Comment("d").ToEnumerable());

                // hrs -> a b c d
                card.SaveHealthRecords(card.HrList, new ListEventArgs<HealthRecord>(null));

                Assert.AreEqual(0, hr0.Ord);
                Assert.AreEqual(1, hr1.Ord);
                Assert.AreEqual(2, hr2.Ord);
                Assert.AreEqual(3, hr3.Ord);

                card.HrList.Sorting = HrViewSortingColumn.Ord;
                card.HrList.Grouping = HrViewGroupingColumn.None;

                Assert.IsTrue(card.HrList.CanReorder);
                var vms = card.HrList.HealthRecords[1].ToEnumerable();
                Assert.IsTrue(card.HrList.CanMove(vms, null));

                card.HrList.hrManager.Reorder(vms, card.HrList.HealthRecords, 0);
                // hrs -> b a c d

                card.SaveHealthRecords(card.HrList, new ListEventArgs<HealthRecord>(null));

                Assert.AreEqual(1, hr0.Ord);
                Assert.AreEqual(0, hr1.Ord);
                Assert.AreEqual(2, hr2.Ord);
                Assert.AreEqual(3, hr3.Ord);

            }
        }
        [TestMethod]
        public void ReorderDeleted()
        {
            using (var card = new CardViewModel(a[5], true))
            {
                card.HrList.AddHealthRecordCommand.Execute(null);
                var hr0 = card.HrList.HealthRecords.Last();
                hr0.healthRecord.AddItems(new Comment("a").ToEnumerable());
                card.HrList.AddHealthRecordCommand.Execute(null);
                var hr1 = card.HrList.HealthRecords.Last();
                hr1.healthRecord.AddItems(new Comment("b").ToEnumerable());
                card.HrList.AddHealthRecordCommand.Execute(null);
                var hr2 = card.HrList.HealthRecords.Last();
                hr2.healthRecord.AddItems(new Comment("c").ToEnumerable());
                card.HrList.AddHealthRecordCommand.Execute(null);
                var hr3 = card.HrList.HealthRecords.Last();
                hr3.healthRecord.AddItems(new Comment("d").ToEnumerable());

                // hrs -> a b c d
                card.SaveHealthRecords(card.HrList, new ListEventArgs<HealthRecord>(null));

                Assert.AreEqual(0, hr0.Ord);
                Assert.AreEqual(1, hr1.Ord);
                Assert.AreEqual(2, hr2.Ord);
                Assert.AreEqual(3, hr3.Ord);

                hr2.healthRecord.IsDeleted = true;

                // hrs -> a b (c) d

                card.HrList.Sorting = HrViewSortingColumn.Ord;
                card.HrList.Grouping = HrViewGroupingColumn.None;

                Assert.IsTrue(card.HrList.CanReorder);
                var vms = card.HrList.HealthRecords[1].ToEnumerable();
                Assert.IsTrue(card.HrList.CanMove(vms, null));

                card.HrList.hrManager.Reorder(vms, card.HrList.HealthRecords, 3);
                // hrs -> a (c) d b

                card.SaveHealthRecords(card.HrList, new ListEventArgs<HealthRecord>(null));

                Assert.AreEqual(0, hr0.Ord);
                Assert.AreEqual(3, hr1.Ord);
                Assert.AreEqual(1, hr2.Ord);
                Assert.AreEqual(2, hr3.Ord);

                hr2.healthRecord.IsDeleted = false;
                // hrs -> a c d b
                Assert.AreEqual(0, card.HrList.HealthRecords[0].Ord);
                Assert.AreEqual(1, card.HrList.HealthRecords[1].Ord);
                Assert.AreEqual(2, card.HrList.HealthRecords[2].Ord);
                Assert.AreEqual(3, card.HrList.HealthRecords[3].Ord);
            }
        }
    }
}