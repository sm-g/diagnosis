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
        public void SelectMany()
        {
            var card = new CardViewModel(a[2], true);
            card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });

            Assert.AreEqual(hr[21], card.HrList.SelectedHealthRecord.healthRecord);
        }

        [TestMethod]
        public void SelectOneByOne()
        {
            var card = new CardViewModel(a[2], true);
            card.HrList.SelectHealthRecord(hr[20]);
            card.HrList.SelectHealthRecord(hr[21]);

            Assert.AreEqual(hr[21], card.HrList.SelectedHealthRecord.healthRecord);
        }

        [TestMethod]
        public void CopyPasteHrs()
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
    }
}