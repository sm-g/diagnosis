using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
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
        private Dictionary<int, Appointment> a = new Dictionary<int, Appointment>();
        private Dictionary<int, HealthRecord> hr = new Dictionary<int, HealthRecord>();

        private Doctor d1;

        private static int[] pIds = new[] { 1, 2, 3, 4, 5 };
        private static int[] cIds = new[] { 1, 2, 3, 4 };
        private static int[] aIds = new[] { 1, 2, 3, 4, 5 };
        private static int[] hrIds = new[] { 1, 2, 20, 21, 22, 30, 31, 32, 40, 70, 71, 72, 73, 74 };

        [TestInitialize]
        public void Init()
        {
            d1 = session.Get<Doctor>(IntToGuid<Doctor>(1));
            AuthorityController.LogIn(d1);

            pIds.ForAll((id) => p[id] = session.Get<Patient>(IntToGuid<Patient>(id)));
            cIds.ForAll((id) => c[id] = session.Get<Course>(IntToGuid<Course>(id)));
            aIds.ForAll((id) => a[id] = session.Get<Appointment>(IntToGuid<Appointment>(id)));
            hrIds.ForAll((id) => hr[id] = session.Get<HealthRecord>(IntToGuid<HealthRecord>(id)));

            // a[2] with hrs 20,21,22
        }

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
        public void DeselectHr()
        {
            var card = new CardViewModel(a[2], true);
            card.HrList.SelectHealthRecord(hr[20]);
            card.HrList.SelectedHealthRecord = null;

            Assert.AreEqual(null, card.HrList.SelectedHealthRecord);
        }

        [TestMethod]
        public void SelectManySelectedLast()
        {
            var card = new CardViewModel(a[2], true);
            card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });

            Assert.AreEqual(hr[21], card.HrList.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(2, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void SelectOneThenMany()
        {
            var card = new CardViewModel(a[2], true);
            card.HrList.SelectHealthRecord(hr[22]);
            card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });

            Assert.AreEqual(hr[21], card.HrList.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(2, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void SelectManyThenOne()
        {
            var card = new CardViewModel(a[2], true);
            card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });
            card.HrList.SelectHealthRecord(hr[21]);

            Assert.AreEqual(hr[21], card.HrList.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(1, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void SelectManyThenChangeSelected()
        {
            var card = new CardViewModel(a[2], true);
            card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });
            card.HrList.SelectHealthRecord(hr[20], true);

            Assert.AreEqual(hr[20], card.HrList.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(2, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void AddToSelected()
        {
            var card = new CardViewModel(a[2], true);
            card.HrList.SelectHealthRecord(hr[20], true);
            card.HrList.SelectHealthRecord(hr[21], true);

            Assert.AreEqual(hr[21], card.HrList.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(2, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void SelectOneByOne()
        {
            var card = new CardViewModel(a[2], true);
            card.HrList.SelectHealthRecord(hr[20]);
            card.HrList.SelectHealthRecord(hr[21]);

            Assert.AreEqual(hr[21], card.HrList.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(1, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void MoveHrSelection()
        {
            var card = new CardViewModel(a[2], true);
            card.HrList.SelectHealthRecord(hr[20]);
            card.HrList.MoveHrSelectionCommand.Execute(true);

            Assert.AreNotEqual(hr[20], card.HrList.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(1, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void MoveEmptyHrSelection()
        {
            var card = new CardViewModel(a[2], true);
            Assert.AreEqual(null, card.HrList.SelectedHealthRecord);

            card.HrList.MoveHrSelectionCommand.Execute(true);

            Assert.AreNotEqual(null, card.HrList.SelectedHealthRecord);
        }

        [TestMethod]
        public void MoveHrSelectionMany()
        {
            var card = new CardViewModel(a[2], true);
            card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });
            card.HrList.MoveHrSelectionCommand.Execute(true); // up to 20

            Assert.AreEqual(hr[20], card.HrList.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(1, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void CopyPasteHr()
        {
            var card = new CardViewModel(a[2], true);

            card.HrList.SelectHealthRecord(hr[20]);
            card.HrList.Copy();
            card.HrList.Paste();

            Assert.AreEqual(4, card.HrList.HealthRecords.Count);
        }

        [TestMethod]
        public void CutDeleteNewWordPaste()
        {
            var card = new CardViewModel(a[2], true);
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

        [TestMethod]
        public void CutPasteHrs()
        {
            var card = new CardViewModel(a[2], true);
            var count = card.HrList.HealthRecords.Count;

            card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });
            card.HrList.Cut();
            card.HrList.Paste();

            Assert.AreEqual(count, card.HrList.HealthRecords.Count);
            Assert.AreEqual(2, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void PastedHrEqual()
        {
            var card = new CardViewModel(a[2], true);
            var count = card.HrList.HealthRecords.Count;

            card.HrList.SelectHealthRecord(hr[20]);
            card.HrList.Cut();
            card.HrList.Paste();

            var new20 = card.HrList.SelectedHealthRecords.First().healthRecord;

            Assert.AreEqual(hr[20].Appointment, new20.Appointment);
            Assert.AreEqual(hr[20].Category, new20.Category);
            Assert.AreEqual(hr[20].Course, new20.Course);
            Assert.AreEqual(hr[20].DateOffset, new20.DateOffset);
            Assert.AreEqual(hr[20].Doctor, new20.Doctor);
            Assert.AreEqual(hr[20].FromDay, new20.FromDay);
            Assert.AreEqual(hr[20].FromMonth, new20.FromMonth);
            Assert.AreEqual(hr[20].FromYear, new20.FromYear);
            // Assert.AreEqual(hr[20].Ord, new20.Ord);
            Assert.AreEqual(hr[20].Patient, new20.Patient);
            Assert.AreEqual(hr[20].Unit, new20.Unit);

            Assert.AreEqual(false, new20.IsDeleted);
            Assert.AreEqual(false, new20.IsDirty);
            Assert.AreNotEqual(hr[20].CreatedAt, new20.CreatedAt);
        }
    }
}