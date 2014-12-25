using Diagnosis.Models;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Screens;
using Diagnosis.ViewModels.Search.Autocomplete;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tests.ViewModelsTests
{
    [TestClass]
    public class HrEditorTest : InMemoryDatabaseTest
    {
        private Doctor d1;
        private HrEditorViewModel e;
        private HealthRecord hr;
        private Word word;
        private string q;

        private AutocompleteViewModel a { get { return e.Autocomplete; } }

        [TestInitialize]
        public void HrEditorTestInit()
        {
            d1 = session.Get<Doctor>(IntToGuid<Doctor>(1));
            AuthorityController.LogIn(d1);

            e = new HrEditorViewModel(session);
            hr = session.Get<HealthRecord>(IntToGuid<HealthRecord>(1));
            word = session.Get<Word>(IntToGuid<Word>(3));
            q = word.Title.Substring(0, word.Title.Length - 1);
        }

        [TestMethod]
        public void Load()
        {
            e.Load(hr);
            Assert.IsTrue(e.IsActive);
            Assert.IsTrue(e.HealthRecord.healthRecord == hr);
        }

        [TestMethod]
        public void Unload()
        {
            e.Unload();

            e.Load(hr);
            e.Unload();
            Assert.IsFalse(e.IsActive);
        }

        [TestMethod]
        public void OrderAddedItem()
        {
            var countBefore = hr.HrItems.Count;

            e.Load(hr);
            a.SelectedTag = a.Tags.Last();
            a.SelectedTag.Query = q;
            a.EnterCommand.Execute(a.SelectedTag);

            e.Unload();

            Assert.IsTrue(hr.HrItems.Count == countBefore + 1);

            var addedItem = hr.HrItems.Where(i => (Word)i.Entity == word).Single();
            Assert.IsTrue(addedItem.Ord == countBefore);
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
    }
}