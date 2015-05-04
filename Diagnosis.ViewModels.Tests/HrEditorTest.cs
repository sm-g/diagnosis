using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class HrEditorTest : ViewModelTest
    {
        private HrEditorViewModel e;
        private new HealthRecord hr;
        private Word word3;
        private HealthRecord hr2;

        private new AutocompleteViewModel a { get { return e.Autocomplete as AutocompleteViewModel; } }

        [TestInitialize]
        public void HrEditorTestInit()
        {
            Load<Doctor>();
            AuthorityController.TryLogIn(d1);

            e = new HrEditorViewModel(session);
            hr = session.Get<HealthRecord>(IntToGuid<HealthRecord>(1));
            hr2 = session.Get<HealthRecord>(IntToGuid<HealthRecord>(2));
            word3 = session.Get<Word>(IntToGuid<Word>(3));
        }

        [TestCleanup]
        public void HrEditorTestCleanup()
        {
            if (e != null)
                e.Dispose();
            // no need to recreate hreditor - create in ctor?
        }

        [TestMethod]
        public void DataConditions()
        {
            Assert.IsTrue(hr.ToDate.IsEmpty);
        }

        [TestMethod]
        public void Load()
        {
            e.Load(hr);
            Assert.IsTrue(e.HasHealthRecord);
            Assert.IsTrue(e.HealthRecord.healthRecord == hr);
        }

        [TestMethod]
        public void CanUnloadOrCloseEmpty()
        {
            e.Unload();
            Assert.IsTrue(e.CloseCommand.CanExecute(null));
            e.CloseCommand.Execute(null);

            e.Load(hr);
            e.Unload();
            Assert.IsFalse(e.HasHealthRecord);
        }

        [TestMethod]
        public void LoadClose()
        {
            // closing > unloaded > closed

            IDomainObject closing, closed, unloaded;
            bool closingWas = false, closedWas = false, unloadedWas = false;
            var closingEh = (EventHandler<HealthRecordEventArgs>)((s, e1) =>
            {
                closing = e1.hr;
                closingWas = true;

                Assert.AreEqual(hr, closing);
                Assert.IsFalse(unloadedWas);
                Assert.IsFalse(closedWas);
                Assert.IsTrue(e.HasHealthRecord);
            });
            var unloadedEh = (EventHandler<HealthRecordEventArgs>)((s, e1) =>
            {
                unloaded = e1.hr;
                unloadedWas = true;
                Assert.AreEqual(hr, unloaded);
                Assert.IsTrue(closingWas);
                Assert.IsFalse(closedWas);
                Assert.IsTrue(e.HasHealthRecord);
            });
            var closedEh = (EventHandler<HealthRecordEventArgs>)((s, e1) =>
            {
                closed = e1.hr;
                closedWas = true;

                Assert.AreEqual(hr, closed);
                Assert.IsTrue(unloadedWas);
                Assert.IsTrue(closingWas);
                Assert.IsFalse(e.HasHealthRecord);
            });
            e.Closing += closingEh;
            e.Unloaded += unloadedEh;
            e.Closed += closedEh;

            e.Load(hr);

            Assert.IsTrue(e.CloseCommand.CanExecute(null));
            e.CloseCommand.Execute(null);

            Assert.IsTrue(closingWas);
            Assert.IsTrue(closedWas);
            Assert.IsTrue(unloadedWas);

            e.Closing -= closingEh;
            e.Unloaded -= unloadedEh;
            e.Closed -= closedEh;
        }

        [TestMethod]
        public void CloseEmpty()
        {
            // closing > closed

            IDomainObject closing, closed;
            bool closingWas = false, closedWas = false, unloadedWas = false;
            var closingEh = (EventHandler<HealthRecordEventArgs>)((s, e1) =>
            {
                closing = e1.hr;
                closingWas = true;

                Assert.AreEqual(null, closing);
                Assert.IsFalse(unloadedWas);
                Assert.IsFalse(closedWas);
            });
            var unloadedEh = (EventHandler<HealthRecordEventArgs>)((s, e1) =>
            {
                unloadedWas = true;
            });
            var closedEh = (EventHandler<HealthRecordEventArgs>)((s, e1) =>
            {
                closed = e1.hr;
                closedWas = true;
                Assert.AreEqual(null, closed);
            });
            e.Closing += closingEh;
            e.Unloaded += unloadedEh;
            e.Closed += closedEh;

            e.CloseCommand.Execute(null);

            Assert.IsTrue(closingWas);
            Assert.IsTrue(closedWas);
            Assert.IsFalse(unloadedWas);

            e.Closing -= closingEh;
            e.Unloaded -= unloadedEh;
            e.Closed -= closedEh;
        }

        [TestMethod]
        public void LoadOther()
        {
            // unloaded. в редакторе не перестает быть запись

            IDomainObject unloaded;
            bool closingWas = false, closedWas = false, unloadedWas = false;
            var closingEh = (EventHandler<HealthRecordEventArgs>)((s, e1) =>
            {
                closingWas = true;
            });
            var unloadedEh = (EventHandler<HealthRecordEventArgs>)((s, e1) =>
            {
                unloaded = e1.hr;
                unloadedWas = true;
                Assert.AreEqual(hr, unloaded);
                Assert.IsFalse(closedWas);
                Assert.IsFalse(closingWas);
                Assert.IsTrue(e.HasHealthRecord);
            });
            var closedEh = (EventHandler<HealthRecordEventArgs>)((s, e1) =>
            {
                closedWas = true;
            });
            e.Closing += closingEh;
            e.Unloaded += unloadedEh;
            e.Closed += closedEh;

            e.Load(hr);
            e.Load(hr2);

            Assert.IsFalse(closingWas);
            Assert.IsFalse(closedWas);
            Assert.IsTrue(unloadedWas);
            e.Closing -= closingEh;
            e.Unloaded -= unloadedEh;
            e.Closed -= closedEh;
        }

        [TestMethod]
        public void LoadSame()
        {
            // nothing

            bool closingWas = false, closedWas = false, unloadedWas = false;
            var closingEh = (EventHandler<HealthRecordEventArgs>)((s, e1) =>
            {
                closingWas = true;
            });
            var unloadedEh = (EventHandler<HealthRecordEventArgs>)((s, e1) =>
            {
                unloadedWas = true;
            });
            var closedEh = (EventHandler<HealthRecordEventArgs>)((s, e1) =>
            {
                closedWas = true;
            });
            e.Closing += closingEh;
            e.Unloaded += unloadedEh;
            e.Closed += closedEh;

            e.Load(hr);
            e.Load(hr);
            Assert.IsFalse(closingWas);
            Assert.IsFalse(closedWas);
            Assert.IsFalse(unloadedWas);

            e.Closing -= closingEh;
            e.Unloaded -= unloadedEh;
            e.Closed -= closedEh;
        }

        [TestMethod]
        public void LoadUnload()
        {
            // unloaded > closed

            IDomainObject closed, unloaded;
            bool closingWas = false, closedWas = false, unloadedWas = false;
            var closingEh = (EventHandler<HealthRecordEventArgs>)((s, e1) =>
            {
                closingWas = true;
            });
            var unloadedEh = (EventHandler<HealthRecordEventArgs>)((s, e1) =>
            {
                unloaded = e1.hr;
                unloadedWas = true;
                Assert.AreEqual(hr, unloaded);
                Assert.IsFalse(closedWas);
                Assert.IsFalse(closingWas);
                Assert.IsTrue(e.HasHealthRecord);
            });
            var closedEh = (EventHandler<HealthRecordEventArgs>)((s, e1) =>
            {
                closed = e1.hr;
                closedWas = true;
                Assert.AreEqual(hr, closed);
                Assert.IsTrue(unloadedWas);
            });

            e.Closing += closingEh;
            e.Unloaded += unloadedEh;
            e.Closed += closedEh;

            e.Load(hr);
            e.Unload();

            Assert.IsFalse(closingWas);
            Assert.IsTrue(closedWas);
            Assert.IsTrue(unloadedWas);
            e.Closing -= closingEh;
            e.Unloaded -= unloadedEh;
            e.Closed -= closedEh;
        }

        [TestMethod]
        public void OrderAddedItem()
        {
            Assert.IsFalse(hr.Words.Contains(word3));

            var countBefore = hr.HrItems.Count;

            e.Load(hr);
            a.SelectedTag = a.Tags.Last();
            a.SelectedTag.Query = word3.Title.Substring(0, word3.Title.Length - 1);
            a.EnterCommand.Execute(a.SelectedTag);

            e.Unload();

            Assert.IsTrue(hr.HrItems.Count == countBefore + 1);

            var addedItem = hr.HrItems.Where(i => (Word)i.Entity == word3).Single();
            Assert.IsTrue(addedItem.Ord == countBefore);
        }

        [TestMethod]
        public void ConvertWordToComment()
        {
            e.Load(hr);
            a.SelectedTag = a.Tags.First();
            var word = a.SelectedTag.Blank as Word;
            a.SelectedTag.ConvertToCommand.Execute(BlankType.Comment);

            Assert.AreEqual(word.Title, hr.GetOrderedEntities().OfType<Comment>().Single().String);
            Assert.IsTrue(!hr.GetOrderedEntities().Contains(word));
        }

        [TestMethod]
        public void OrderAfterDeleteItem()
        {
            Assert.IsTrue(hr.HrItems.Count > 1);
            var second = hr.HrItems.ElementAt(1);
            Assert.IsTrue(second.Ord == 1);

            e.Load(hr);
            a.SelectedTag = a.Tags.First();
            a.SelectedTag.DeleteCommand.Execute(null);
            e.Unload();
            Assert.IsTrue(second.Ord == 0);
        }

        [TestMethod]
        public void AddWordToDoctorAfterSaveHisHr()
        {
            // слова становятся видны доктору только после сохранения записи
            AuthorityController.TryLogIn(d2);

            Word wForD1Only = word3;
            Assert.IsTrue(!d2.Words.Contains(wForD1Only));
            using (var card = new CardViewModel(hr))
            {
                card.HrList.AddHealthRecordCommand.Execute(null);
                (card.HrEditor.Autocomplete as AutocompleteViewModel).AddTag(wForD1Only);

                Assert.IsTrue(!d2.Words.Contains(wForD1Only));

                card.HrEditor.CloseCommand.Execute(null);
            }
            Assert.IsTrue(d2.Words.Contains(wForD1Only));
        }

        [TestMethod]
        public void HideIntervalEditor()
        {
            e.Load(hr);
            var old = hr.ToDate;
            Assert.IsTrue(e.HealthRecord.DateEditor.IsIntervalEditorOpened);

            // скрыт редактор интервала - дата-точка, открыт - последний ввод
            e.HealthRecord.DateEditor.IsIntervalEditorOpened = false;
            Assert.AreEqual(hr.FromDate, hr.ToDate);

            e.HealthRecord.DateEditor.IsIntervalEditorOpened = true;
            Assert.AreEqual(old, hr.ToDate);
        }

        [TestMethod]
        public void SetToDateSameAsFromDate()
        {
            e.Load(hr);
            Assert.IsTrue(e.HealthRecord.DateEditor.IsIntervalEditorOpened);

            hr.ToDate.FillDateFrom(hr.FromDate);

            // редактор интервала остается открыт
            Assert.IsTrue(e.HealthRecord.DateEditor.IsIntervalEditorOpened);
        }
    }
}