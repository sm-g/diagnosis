using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.Models.Enums;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class HrListTest : ViewModelTest
    {
        private CardViewModel card;

        [TestInitialize]
        public void Init()
        {
            Load<Doctor>();

            Load<Patient>();
            Load<Course>();
            Load<HrCategory>();
            Load<Appointment>();
            Load<HealthRecord>();

            AuthorityController.TryLogIn(d1);

            card = new CardViewModel(a[2], true);
        }

        public void Clean()
        {
            if (card != null)
                card.Dispose();
        }

        [TestMethod]
        public void DataConditions()
        {
            Assert.IsTrue(a[2].HealthRecords.Contains(hr[20]));
            Assert.IsTrue(a[2].HealthRecords.Contains(hr[21]));
            Assert.IsTrue(a[2].HealthRecords.Contains(hr[22]));

            Assert.AreEqual(0, a[5].HealthRecords.Count());
        }

        #region Selection

        [TestMethod]
        public void SelectHr()
        {
            card.HrList.SelectHealthRecord(hr[20]);

            Assert.AreEqual(hr[20], card.HrList.SelectedHealthRecord.healthRecord);
        }

        [TestMethod]
        public void SelectNotInList()
        {
            card.HrList.SelectHealthRecord(hr[1]);

            Assert.AreEqual(null, card.HrList.SelectedHealthRecord);
            Assert.AreEqual(0, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void DeselectHr()
        {
            card.HrList.SelectHealthRecord(hr[20]);
            card.HrList.SelectedHealthRecord = null;

            Assert.AreEqual(null, card.HrList.SelectedHealthRecord);
        }

        [TestMethod]
        public void DeselectWhenManySelected()
        {
            card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });
            card.HrList.SelectedHealthRecord = null;

            Assert.AreEqual(null, card.HrList.SelectedHealthRecord);
            Assert.AreEqual(0, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void SelectManySelectedLast()
        {
            card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });

            Assert.AreEqual(hr[21], card.HrList.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(2, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void SelectOneThenMany()
        {
            card.HrList.SelectHealthRecord(hr[22]);
            card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });

            Assert.AreEqual(hr[21], card.HrList.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(2, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void SelectManyThenOne()
        {
            card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });
            card.HrList.SelectHealthRecord(hr[21]);

            Assert.AreEqual(hr[21], card.HrList.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(1, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void SelectManyThenChangeSelected()
        {
            card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });
            card.HrList.SelectHealthRecord(hr[20], true);
            Assert.AreEqual(hr[20], card.HrList.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(2, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void AddToSelected()
        {
            card.HrList.SelectHealthRecord(hr[20], true);
            card.HrList.SelectHealthRecord(hr[21], true);

            Assert.AreEqual(hr[21], card.HrList.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(2, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void AddToSelectedNotInList()
        {
            card.HrList.SelectHealthRecord(hr[20], true);
            card.HrList.SelectHealthRecord(hr[1], true);

            Assert.AreEqual(hr[20], card.HrList.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(1, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void SelectOneByOne()
        {
            card.HrList.SelectHealthRecord(hr[20]);
            card.HrList.SelectHealthRecord(hr[21]);

            Assert.AreEqual(hr[21], card.HrList.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(1, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void MoveHrSelection()
        {
            card.HrList.SelectHealthRecord(hr[20]);
            card.HrList.MoveHrSelectionCommand.Execute(true);

            Assert.AreNotEqual(hr[20], card.HrList.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(1, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void MoveEmptyHrSelection()
        {
            Assert.AreEqual(null, card.HrList.SelectedHealthRecord);

            card.HrList.MoveHrSelectionCommand.Execute(true);

            Assert.AreNotEqual(null, card.HrList.SelectedHealthRecord);
        }

        [TestMethod]
        public void MoveHrSelectionMany()
        {
            card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });
            card.HrList.Grouping = HrViewColumn.None;
            card.HrList.MoveHrSelectionCommand.Execute(true); // up to 20

            Assert.AreEqual(hr[20], card.HrList.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(1, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void MoveHrSelectionManySorted()
        {
            OpenEmptyCard();

            var hr0 = AddHrToCard(card);
            var hr1 = AddHrToCard(card);
            var hr2 = AddHrToCard(card);
            card.HrList.Sorting = HrViewColumn.CreatedAt;
            card.HrList.Grouping = HrViewColumn.Category;

            // 1 cat
            // 0
            // 2
            // 2 cat
            // 1

            hr0.Category = cat[1];
            hr1.Category = cat[2];
            hr2.Category = cat[1];

            card.HrList.SelectHealthRecord(hr0);
            card.HrList.MoveHrSelectionCommand.Execute(true); // up

            // move selection to last in view
            Assert.AreEqual(hr1, card.HrList.SelectedHealthRecord.healthRecord);
        }

        #endregion Selection

        #region CurCopyPaste

        [TestMethod]
        public void CopyPasteHr()
        {
            card.HrList.SelectHealthRecord(hr[20]);
            card.HrList.Copy();
            card.HrList.Paste();

            Assert.AreEqual(4, card.HrList.HealthRecords.Count);
        }

        [TestMethod]
        public void CopyPasteHrWithIcd()
        {
            Load<IcdDisease>();
            Open(a[3]);

            card.HrList.SelectHealthRecord(hr[31]);
            card.HrList.Copy();
            card.HrList.Paste();

            var pastedHr = card.HrList.SelectedHealthRecord.healthRecord;
            var pastedIcd = pastedHr.GetOrderedEntities().FirstOrDefault(x => x.Equals(icd[1])) as IcdDisease;
            Assert.IsTrue(pastedIcd != null);
            Assert.AreEqual(icd[1].IcdBlock, pastedIcd.IcdBlock);
        }

        [TestMethod]
        public void CutDeletePastePersistedWord()
        {
            var w = new Word("11");

            card.HrList.AddHealthRecordCommand.Execute(null);
            card.HrEditor.Autocomplete.AddTag(w);
            card.HrEditor.CloseCommand.Execute(null);

            Assert.IsTrue(!w.IsTransient);
            Assert.AreEqual(1, w.HealthRecords.Count());

            card.HrList.Cut();

            Assert.AreEqual(0, w.HealthRecords.Count());

            var wordList = new WordsListViewModel();
            wordList.SelectWord(w);
            wordList.DeleteCommand.Execute(null);

            card.HrList.Paste();

            var newWord = card.HrList.SelectedHealthRecord.healthRecord.HrItems.Single().Word;
            // word recreated and saved with same title
            Assert.AreEqual(0, newWord.CompareTo(w));
            Assert.IsTrue(!newWord.IsTransient);
        }

        [TestMethod]
        public void NoSelectedAfterCut()
        {
            var count = card.HrList.HealthRecords.Count;

            card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });
            card.HrList.Cut();

            Assert.AreEqual(null, card.HrList.SelectedHealthRecord);
            Assert.AreEqual(0, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void CutPasteHrs()
        {
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
            var count = card.HrList.HealthRecords.Count;

            card.HrList.SelectHealthRecord(hr[20]);
            card.HrList.Cut();
            card.HrList.Paste();

            var new20 = card.HrList.SelectedHealthRecords.First().healthRecord;

            Assert.AreEqual(hr[20].Patient, new20.Patient);
            Assert.AreEqual(hr[20].Course, new20.Course);
            Assert.AreEqual(hr[20].Appointment, new20.Appointment);

            Assert.AreEqual(hr[20].Category, new20.Category);

            Assert.AreEqual(hr[20].FromDate.Day, new20.FromDate.Day);
            Assert.AreEqual(hr[20].FromDate.Month, new20.FromDate.Month);
            Assert.AreEqual(hr[20].FromDate.Year, new20.FromDate.Year);
            Assert.AreEqual(hr[20].FromDate.Now, new20.FromDate.Now);
            Assert.AreEqual(hr[20].ToDate.Day, new20.ToDate.Day);
            Assert.AreEqual(hr[20].ToDate.Month, new20.ToDate.Month);
            Assert.AreEqual(hr[20].ToDate.Year, new20.ToDate.Year);
            Assert.AreEqual(hr[20].ToDate.Now, new20.ToDate.Now);

            Assert.AreEqual(hr[20].Unit, new20.Unit);

            // Assert.AreEqual(hr[20].Ord, new20.Ord); в любое место

            Assert.AreEqual(false, new20.IsDeleted);
            Assert.AreEqual(false, new20.IsDirty);

            Assert.AreEqual(hr[20].Doctor, new20.Doctor); // или новый автор?
            Assert.AreNotEqual(hr[20].CreatedAt, new20.CreatedAt);
            Assert.AreNotEqual(hr[20].UpdatedAt, new20.UpdatedAt);
        }

        #endregion CurCopyPaste

        #region Movement

        [TestMethod]
        public void Reorder()
        {
            OpenEmptyCard();

            var hr0 = AddHrToCard(card, "a");
            var hr1 = AddHrToCard(card, "b");
            var hr2 = AddHrToCard(card, "c");
            var hr3 = AddHrToCard(card, "d");

            // hrs -> a b c d
            card.SaveHealthRecords(card.HrList, new ListEventArgs<HealthRecord>(null));

            Assert.AreEqual(0, hr0.Ord);
            Assert.AreEqual(1, hr1.Ord);
            Assert.AreEqual(2, hr2.Ord);
            Assert.AreEqual(3, hr3.Ord);

            card.HrList.Sorting = HrViewColumn.Ord;
            card.HrList.Grouping = HrViewColumn.None;

            Assert.IsTrue(card.HrList.CanReorder);
            var vms = card.HrList.HealthRecords[1].ToEnumerable();
            Assert.IsTrue(card.HrList.CanDropTo(vms, null));

            card.HrList.Reorder(vms, 0, null);
            // hrs -> b a c d

            card.SaveHealthRecords(card.HrList, new ListEventArgs<HealthRecord>(null));

            Assert.AreEqual(1, hr0.Ord);
            Assert.AreEqual(0, hr1.Ord);
            Assert.AreEqual(2, hr2.Ord);
            Assert.AreEqual(3, hr3.Ord);
        }

        [TestMethod]
        public void ReorderDeleted()
        {
            OpenEmptyCard();

            var hr0 = AddHrToCard(card, "a");
            var hr1 = AddHrToCard(card, "b");
            var hr2 = AddHrToCard(card, "c");
            var hr3 = AddHrToCard(card, "d");

            // hrs -> a b c d
            card.SaveHealthRecords(card.HrList, new ListEventArgs<HealthRecord>(null));

            hr2.IsDeleted = true;

            // hrs -> a b (c) d

            card.HrList.Sorting = HrViewColumn.Ord;
            card.HrList.Grouping = HrViewColumn.None;

            var vms = card.HrList.HealthRecords[1].ToEnumerable();
            card.HrList.Reorder(vms, 3, null);
            // hrs -> a (c) d b

            card.SaveHealthRecords(card.HrList, new ListEventArgs<HealthRecord>(null));

            Assert.AreEqual(0, hr0.Ord);
            Assert.AreEqual(3, hr1.Ord);
            Assert.AreEqual(1, hr2.Ord);
            Assert.AreEqual(2, hr3.Ord);

            hr2.IsDeleted = false;
            // hrs -> a c d b
            Assert.AreEqual(0, card.HrList.HealthRecords[0].Ord);
            Assert.AreEqual(1, card.HrList.HealthRecords[1].Ord);
            Assert.AreEqual(2, card.HrList.HealthRecords[2].Ord);
            Assert.AreEqual(3, card.HrList.HealthRecords[3].Ord);
        }

        [TestMethod]
        public void MoveToOtherCategory()
        {
            OpenEmptyCard();

            card.HrList.Sorting = HrViewColumn.Ord;
            card.HrList.Grouping = HrViewColumn.Category;

            var hr0 = AddHrToCard(card, "a");
            var hr1 = AddHrToCard(card, "b");
            var hr2 = AddHrToCard(card, "c");
            hr0.Category = cat[1];
            hr1.Category = cat[2];
            hr2.Category = cat[2];

            card.HrList.SelectHealthRecord(hr0);

            Assert.IsTrue(card.HrList.MoveHrCommand.CanExecute(false));

            card.HrList.MoveHrCommand.Execute(false); //down
            Assert.AreEqual(cat[2], hr0.Category);
        }

        [TestMethod]
        public void MoveToEnd()
        {
            OpenEmptyCard();

            card.HrList.Sorting = HrViewColumn.Ord;
            card.HrList.Grouping = HrViewColumn.None;

            var hr0 = AddHrToCard(card, "a");
            var hr1 = AddHrToCard(card, "b");
            var hr3 = AddHrToCard(card, "c");

            card.HrList.SelectHealthRecord(hr0);

            card.HrList.MoveHrCommand.Execute(false); //down
            card.HrList.MoveHrCommand.Execute(false); //down

            var lastHr = card.HrList.view.Cast<ShortHealthRecordViewModel>().Last().healthRecord;

            Assert.AreEqual(hr0, lastHr);
        }

        [TestMethod]
        public void CanMoveFirstLast()
        {
            OpenEmptyCard();

            card.HrList.Sorting = HrViewColumn.Ord;
            card.HrList.Grouping = HrViewColumn.None;

            var hr0 = AddHrToCard(card, "a");
            var hr1 = AddHrToCard(card, "b");

            card.HrList.SelectHealthRecord(hr0);

            Assert.IsTrue(card.HrList.MoveHrCommand.CanExecute(false)); // down
            Assert.IsFalse(card.HrList.MoveHrCommand.CanExecute(true)); // up

            card.HrList.SelectHealthRecord(hr1);

            Assert.IsFalse(card.HrList.MoveHrCommand.CanExecute(false)); // down
            Assert.IsTrue(card.HrList.MoveHrCommand.CanExecute(true)); // up
        }

        [TestMethod]
        public void CanMoveGroupingCreatedAt()
        {
            OpenEmptyCard();

            card.HrList.Sorting = HrViewColumn.Ord;
            card.HrList.Grouping = HrViewColumn.CreatedAt;

            var hr0 = AddHrToCard(card, "a");
            var hr1 = AddHrToCard(card, "b");
            var hr2 = AddHrToCard(card, "c");
            var hr3 = AddHrToCard(card, "d");

            var dtOld = new DateTime(2015, 1, 1);
            var dtNow = DateTime.Now;
            (hr0 as IHaveAuditInformation).CreatedAt = dtOld;
            (hr1 as IHaveAuditInformation).CreatedAt = dtOld;
            (hr2 as IHaveAuditInformation).CreatedAt = dtNow;
            (hr3 as IHaveAuditInformation).CreatedAt = dtNow;

            // old
            // a
            // b
            // now
            // c
            // d

            // можно перемещать только в пределах группы
            card.HrList.SelectHealthRecord(hr0);

            Assert.IsTrue(card.HrList.MoveHrCommand.CanExecute(false)); // down
            Assert.IsFalse(card.HrList.MoveHrCommand.CanExecute(true)); // up

            card.HrList.SelectHealthRecord(hr1);

            Assert.IsFalse(card.HrList.MoveHrCommand.CanExecute(false)); // down
            Assert.IsTrue(card.HrList.MoveHrCommand.CanExecute(true)); // up

            card.HrList.SelectHealthRecord(hr2);

            Assert.IsTrue(card.HrList.MoveHrCommand.CanExecute(false)); // down
            Assert.IsFalse(card.HrList.MoveHrCommand.CanExecute(true)); // up

            card.HrList.SelectHealthRecord(hr3);

            Assert.IsFalse(card.HrList.MoveHrCommand.CanExecute(false)); // down
            Assert.IsTrue(card.HrList.MoveHrCommand.CanExecute(true)); // up
        }

        [TestMethod]
        public void CanDropGroupingCreatedAt()
        {
            OpenEmptyCard();

            card.HrList.Sorting = HrViewColumn.Ord;
            card.HrList.Grouping = HrViewColumn.CreatedAt;

            var hr0 = AddHrToCard(card, "a");
            var hr1 = AddHrToCard(card, "b");
            var hr2 = AddHrToCard(card, "c");
            var hr3 = AddHrToCard(card, "d");

            var dtOld = new DateTime(2015, 1, 1);
            var dtNow = DateTime.Now;
            (hr0 as IHaveAuditInformation).CreatedAt = dtOld;
            (hr1 as IHaveAuditInformation).CreatedAt = dtOld;
            (hr2 as IHaveAuditInformation).CreatedAt = dtNow;
            (hr3 as IHaveAuditInformation).CreatedAt = dtNow;

            // old
            // a
            // b
            // now
            // c
            // d

            // можно перемещать только в пределах группы
            var group = card.HrList.GetGroupObject(card.HrList.HealthRecords[0]);
            Assert.IsTrue(card.HrList.CanDropTo(card.HrList.HealthRecords.Take(2), group));
            Assert.IsFalse(card.HrList.CanDropTo(card.HrList.HealthRecords.Skip(1).Take(2), group));
        }

        #endregion Movement

        private HealthRecord AddHrToCard(CardViewModel card, string comment = null)
        {
            var hr = card.HrList.holder.AddHealthRecord(d1);
            if (comment != null)
                hr.AddItems(new Comment(comment).ToEnumerable());
            return hr;
        }

        private void OpenEmptyCard()
        {
            card.Dispose();
            card = new CardViewModel(a[5], true);
        }

        private void Open(IHrsHolder h)
        {
            card.Dispose();
            card = new CardViewModel(h, true);
        }
    }
}