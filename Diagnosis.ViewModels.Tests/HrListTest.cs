using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.Models.Enums;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class HrListTest : ViewModelTest
    {
        private CardViewModel card;

        public HrListViewModel l { get { return card.HrList; } }

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

        [TestCleanup]
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
            l.SelectHealthRecord(hr[20]);

            Assert.AreEqual(hr[20], l.SelectedHealthRecord.healthRecord);
        }

        [TestMethod]
        public void SelectNotInList()
        {
            l.SelectHealthRecord(hr[1]);

            Assert.AreEqual(null, l.SelectedHealthRecord);
            Assert.AreEqual(0, l.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void DeselectHr()
        {
            l.SelectHealthRecord(hr[20]);
            l.SelectedHealthRecord = null;

            Assert.AreEqual(null, l.SelectedHealthRecord);
        }

        [TestMethod]
        public void DeselectWhenManySelected()
        {
            l.SelectHealthRecords(new[] { hr[20], hr[21] });
            l.SelectedHealthRecord = null;

            Assert.AreEqual(null, l.SelectedHealthRecord);
            Assert.AreEqual(0, l.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void SelectManySelectedLast()
        {
            l.SelectHealthRecords(new[] { hr[20], hr[21] });

            Assert.AreEqual(hr[21], l.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(2, l.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void SelectOneThenMany()
        {
            l.SelectHealthRecord(hr[22]);
            l.SelectHealthRecords(new[] { hr[20], hr[21] });

            Assert.AreEqual(hr[21], l.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(2, l.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void SelectManyThenOne()
        {
            l.SelectHealthRecords(new[] { hr[20], hr[21] });
            l.SelectHealthRecord(hr[21]);

            Assert.AreEqual(hr[21], l.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(1, l.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void SelectManyThenChangeSelected()
        {
            l.SelectHealthRecords(new[] { hr[20], hr[21] });
            l.SelectHealthRecord(hr[20], true);
            Assert.AreEqual(hr[20], l.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(2, l.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void AddToSelected()
        {
            l.SelectHealthRecord(hr[20], true);
            l.SelectHealthRecord(hr[21], true);

            Assert.AreEqual(hr[21], l.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(2, l.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void AddToSelectedNotInList()
        {
            l.SelectHealthRecord(hr[20], true);
            l.SelectHealthRecord(hr[1], true);

            Assert.AreEqual(hr[20], l.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(1, l.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void SelectOneByOne()
        {
            l.SelectHealthRecord(hr[20]);
            l.SelectHealthRecord(hr[21]);

            Assert.AreEqual(hr[21], l.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(1, l.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void MoveHrSelection()
        {
            l.SelectHealthRecord(hr[20]);
            l.MoveHrSelectionCommand.Execute(true);

            Assert.AreNotEqual(hr[20], l.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(1, l.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void DateEditorSavesOpenedState()
        {
            l.SelectHealthRecord(hr[20]);
            card.ToogleHrEditor();
            card.HrEditor.HealthRecord.DateEditor.IsDateEditorExpanded = true;

            l.MoveHrSelectionCommand.Execute(true);
            Assert.AreEqual(true, card.HrEditor.HealthRecord.DateEditor.IsDateEditorExpanded);
        }

        [TestMethod]
        public void MoveEmptyHrSelection()
        {
            Assert.AreEqual(null, l.SelectedHealthRecord);

            l.MoveHrSelectionCommand.Execute(true);

            Assert.AreNotEqual(null, l.SelectedHealthRecord);
        }

        [TestMethod]
        public void MoveHrSelectionMany()
        {
            l.SelectHealthRecords(new[] { hr[20], hr[21] });
            l.Grouping = HrViewColumn.None;
            l.MoveHrSelectionCommand.Execute(true); // up to 20

            Assert.AreEqual(hr[20], l.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(1, l.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void MoveHrSelectionManySorted()
        {
            OpenEmptyCard();

            var hr0 = AddHrToCard(card);
            var hr1 = AddHrToCard(card);
            var hr2 = AddHrToCard(card);
            l.Sorting = HrViewColumn.CreatedAt;
            l.Grouping = HrViewColumn.Category;

            // 1 cat
            // 0
            // 2
            // 2 cat
            // 1

            hr0.Category = cat[1];
            hr1.Category = cat[2];
            hr2.Category = cat[1];

            l.SelectHealthRecord(hr0);
            l.MoveHrSelectionCommand.Execute(true); // up

            // move selection to last in view
            Assert.AreEqual(hr1, l.SelectedHealthRecord.healthRecord);
        }

        #endregion Selection

        #region CurCopyPaste

        [TestMethod]
        public void CopyPasteHr()
        {
            l.SelectHealthRecord(hr[20]);
            l.Copy();
            l.Paste();

            Assert.AreEqual(4, l.HealthRecords.Count);
        }

        [TestMethod]
        public void CopyPasteHrWithIcd()
        {
            Load<IcdDisease>();
            Open(a[3]);

            l.SelectHealthRecord(hr[31]);
            l.Copy();
            l.Paste();

            var pastedHr = l.SelectedHealthRecord.healthRecord;
            var pastedIcd = pastedHr.GetOrderedEntities().FirstOrDefault(x => x.Equals(icd[1])) as IcdDisease;
            Assert.IsTrue(pastedIcd != null);
            Assert.AreEqual(icd[1].IcdBlock, pastedIcd.IcdBlock);
        }

        [TestMethod]
        public void CutDeletePastePersistedWord()
        {
            var w = new Word("11");

            l.AddHealthRecordCommand.Execute(null);
            (card.HrEditor.Autocomplete as AutocompleteViewModel).AddTag(w);
            card.HrEditor.CloseCommand.Execute(null);

            Assert.IsTrue(!w.IsTransient);
            Assert.AreEqual(1, w.HealthRecords.Count());

            l.Cut();

            Assert.AreEqual(0, w.HealthRecords.Count());

            // delete word
            w.OnDelete();
            using (var tr = session.BeginTransaction())
            {
                session.Delete(w);
                tr.Commit();
            }

            l.Paste();

            var newWord = l.SelectedHealthRecord.healthRecord.HrItems.Single().Word;
            // word recreated and saved with same title
            Assert.AreNotEqual(w, newWord);
            Assert.AreEqual(0, newWord.CompareTo(w));
            Assert.IsTrue(!newWord.IsTransient);
        }

        [TestMethod]
        public void NoSelectedAfterCut()
        {
            var count = l.HealthRecords.Count;

            l.SelectHealthRecords(new[] { hr[20], hr[21] });
            l.Cut();

            Assert.AreEqual(null, l.SelectedHealthRecord);
            Assert.AreEqual(0, l.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void CutPasteHrs()
        {
            var count = l.HealthRecords.Count;

            l.SelectHealthRecords(new[] { hr[20], hr[21] });
            l.Cut();
            l.Paste();

            Assert.AreEqual(count, l.HealthRecords.Count);
            Assert.AreEqual(2, l.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void PastedHrEqual()
        {
            var count = l.HealthRecords.Count;

            l.SelectHealthRecord(hr[20]);
            l.Cut();
            l.Paste();

            var new20 = l.SelectedHealthRecords.First().healthRecord;

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
            card.SaveAllHrs();

            Assert.AreEqual(0, hr0.Ord);
            Assert.AreEqual(1, hr1.Ord);
            Assert.AreEqual(2, hr2.Ord);
            Assert.AreEqual(3, hr3.Ord);

            l.Sorting = HrViewColumn.Ord;
            l.Grouping = HrViewColumn.None;

            Assert.IsTrue(l.CanReorder);
            var vms = l.HealthRecords[1].ToEnumerable();
            Assert.IsTrue(l.CanDropTo(vms, null));

            l.Reorder(vms, 0, null);
            // hrs -> b a c d

            card.SaveAllHrs();

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
            card.SaveAllHrs();

            hr2.IsDeleted = true;

            // hrs -> a b (c) d

            l.Sorting = HrViewColumn.Ord;
            l.Grouping = HrViewColumn.None;

            var vms = l.HealthRecords[1].ToEnumerable();
            l.Reorder(vms, 3, null);
            // hrs -> a (c) d b

            card.SaveAllHrs();

            Assert.AreEqual(0, hr0.Ord);
            Assert.AreEqual(3, hr1.Ord);
            Assert.AreEqual(1, hr2.Ord);
            Assert.AreEqual(2, hr3.Ord);

            hr2.IsDeleted = false;
            // hrs -> a c d b
            Assert.AreEqual(0, l.HealthRecords[0].Ord);
            Assert.AreEqual(1, l.HealthRecords[1].Ord);
            Assert.AreEqual(2, l.HealthRecords[2].Ord);
            Assert.AreEqual(3, l.HealthRecords[3].Ord);
        }

        [TestMethod]
        public void MoveToOtherCategory()
        {
            OpenEmptyCard();

            l.Sorting = HrViewColumn.Ord;
            l.Grouping = HrViewColumn.Category;

            var hr0 = AddHrToCard(card, "a");
            var hr1 = AddHrToCard(card, "b");
            var hr2 = AddHrToCard(card, "c");
            hr0.Category = cat[1];
            hr1.Category = cat[2];
            hr2.Category = cat[2];

            l.SelectHealthRecord(hr0);

            Assert.IsTrue(l.MoveHrCommand.CanExecute(false));

            l.MoveHrCommand.Execute(false); //down
            Assert.AreEqual(cat[2], hr0.Category);
        }

        [TestMethod]
        public void MoveToEnd()
        {
            //OpenEmptyCard();

            //l.Sorting = HrViewColumn.Ord;
            //l.Grouping = HrViewColumn.None;

          //  var hr0 = AddHrToCard(card, "a");
        //    var hr1 = AddHrToCard(card, "b");
       //     var hr3 = AddHrToCard(card, "c");

        //    l.SelectHealthRecord(hr0);

      //      l.MoveHrCommand.Execute(false); //down
       //     l.MoveHrCommand.Execute(false); //down

        //    var lastHr = l.view.Cast<ShortHealthRecordViewModel>().Last().healthRecord;

        //    Assert.AreEqual(hr0, lastHr);
        }

        [TestMethod]
        public void CanMoveFirstLast()
        {
            OpenEmptyCard();

            l.Sorting = HrViewColumn.Ord;
            l.Grouping = HrViewColumn.None;

            var hr0 = AddHrToCard(card, "a");
            var hr1 = AddHrToCard(card, "b");

            l.SelectHealthRecord(hr0);

            Assert.IsTrue(l.MoveHrCommand.CanExecute(false)); // down
            Assert.IsFalse(l.MoveHrCommand.CanExecute(true)); // up

            l.SelectHealthRecord(hr1);

            Assert.IsFalse(l.MoveHrCommand.CanExecute(false)); // down
            Assert.IsTrue(l.MoveHrCommand.CanExecute(true)); // up
        }

        [TestMethod]
        public void CanMoveGroupingCreatedAt()
        {
            OpenEmptyCard();

            l.Sorting = HrViewColumn.Ord;
            l.Grouping = HrViewColumn.CreatedAt;

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
            l.SelectHealthRecord(hr0);

            Assert.IsTrue(l.MoveHrCommand.CanExecute(false)); // down
            Assert.IsFalse(l.MoveHrCommand.CanExecute(true)); // up

            l.SelectHealthRecord(hr1);

            Assert.IsFalse(l.MoveHrCommand.CanExecute(false)); // down
            Assert.IsTrue(l.MoveHrCommand.CanExecute(true)); // up

            l.SelectHealthRecord(hr2);

            Assert.IsTrue(l.MoveHrCommand.CanExecute(false)); // down
            Assert.IsFalse(l.MoveHrCommand.CanExecute(true)); // up

            l.SelectHealthRecord(hr3);

            Assert.IsFalse(l.MoveHrCommand.CanExecute(false)); // down
            Assert.IsTrue(l.MoveHrCommand.CanExecute(true)); // up
        }

        [TestMethod]
        public void CanDropGroupingCreatedAt()
        {
            OpenEmptyCard();

            l.Sorting = HrViewColumn.Ord;
            l.Grouping = HrViewColumn.CreatedAt;

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
            var group = l.GetGroupObject(l.HealthRecords[0]);
            Assert.IsTrue(l.CanDropTo(l.HealthRecords.Take(2), group));
            Assert.IsFalse(l.CanDropTo(l.HealthRecords.Skip(1).Take(2), group));
        }

        #endregion Movement

        [TestMethod]
        public void GroupingAddsSort()
        {
            l.Sorting = HrViewColumn.None;
            l.Grouping = HrViewColumn.None;

            l.Grouping = HrViewColumn.Category;
            var sortD = new SortDescription(HrViewColumn.Category.ToSortingProperty(), ListSortDirection.Ascending);

            Assert.IsTrue(l.view.SortDescriptions.Contains(sortD));
        }

        [TestMethod]
        public void AlwaysSortByOrd()
        {
            l.Sorting = HrViewColumn.Category;
            l.Grouping = HrViewColumn.None;

            var ordSortD = new SortDescription(HrViewColumn.Ord.ToSortingProperty(), ListSortDirection.Ascending);

            Assert.IsTrue(l.view.SortDescriptions.Contains(ordSortD));
        }

        private HealthRecord AddHrToCard(CardViewModel card, string comment = null)
        {
            var hr = l.holder.AddHealthRecord(d1);
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