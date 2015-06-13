using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.Contracts;
using System.Diagnostics.Contracts.Internal;
using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class BlankSetterTest : ViewModelTest
    {
        private BlankSetter bs;
        private string q;
        private Word word;
        private IcdDisease icd1;
        private SuggestionsMaker r;
        private TagViewModel tag;

        [TestInitialize]
        public void AutocompleteTestInit()
        {
            CreatedWordsManager.ClearCreated();

            r = new SuggestionsMaker(session, AuthorityController.CurrentDoctor);
            bs = new BlankSetter(r.FirstMatchingOrNewWord, null, null);

            var a = new HrEditorAutocomplete(r);
            tag = new TagViewModel(a);

            word = session.Get<Word>(IntToGuid<Word>(1));
            icd1 = session.Get<IcdDisease>(1);
            q = "123";

            //Contract.ContractFailed += (sender, e) =>
            //{
            //    e.SetUnwind(); // cause code to abort after event
            //    Assert.Fail(e.FailureKind.ToString() + ":" + e.Message);
            //};
        }

        [TestCleanup]
        public void Clean()
        {
        }

        #region SetBlank

        [TestMethod]
        public void Empty_SetNull_Null()
        {
            bs.SetBlank(tag, null, false, false);
            Assert.AreEqual(null, tag.Blank);
        }

        [TestMethod]
        public void WithBlank_SetNull_Comment()
        {
            tag.Blank = word;

            bs.SetBlank(tag, null, false, false);
            Assert.IsTrue(tag.BlankType == BlankType.Comment);
            Assert.AreEqual(word.Title, (tag.Blank as Comment).String);
        }

        [TestMethod]
        public void WithQuery_SetNull_Comment()
        {
            tag.Query = q;

            bs.SetBlank(tag, null, false, false);
            Assert.IsTrue(tag.BlankType == BlankType.Comment);
            Assert.AreEqual(q, (tag.Blank as Comment).String);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Empty_SetNullInverse_Fails()
        {
            // тк делаем слово по запросу
            bs.SetBlank(tag, null, false, true);
            Assert.IsTrue(tag.BlankType == BlankType.Word);
        }

        [TestMethod]
        public void WithQuery_SetNullInverse_Word()
        {
            tag.Query = q;

            bs.SetBlank(tag, null, false, true);
            Assert.IsTrue(tag.BlankType == BlankType.Word);
            Assert.AreEqual(q, (tag.Blank as Word).Title);
        }

        [TestMethod]
        public void Any_SetWord_ThisWord()
        {
            bs.SetBlank(tag, word, false, false);
            Assert.AreEqual(word, tag.Blank);
        }

        [TestMethod]
        public void Any_SetIcd_ThisIcd()
        {
            bs.SetBlank(tag, icd1, false, false);
            Assert.AreEqual(icd1, tag.Blank);
        }

        [TestMethod]
        public void Empty_SetWordExact_Comment()
        {
            // CompleteOnLostFocus на пустом
            bs.SetBlank(tag, word, true, false);
            Assert.IsTrue(tag.BlankType == BlankType.Comment);
            Assert.AreEqual("", (tag.Blank as Comment).String);
        }

        [TestMethod]
        public void WithQuery_SetWordExact_Comment()
        {
            // запрос не совпал с предположением (CompleteOnLostFocus)
            tag.Query = q;
            bs.SetBlank(tag, word, true, false);
            Assert.IsTrue(tag.BlankType == BlankType.Comment);
            Assert.AreEqual(q, (tag.Blank as Comment).String);
        }

        [TestMethod]
        public void WithQueryAsWord_SetWordExact_ThisWord()
        {
            // CompleteOnLostFocus
            tag.Query = word.Title;
            bs.SetBlank(tag, word, true, false);
            Assert.AreEqual(word, tag.Blank);
        }

        [TestMethod]
        public void Empty_SetWordInverse_Empty()
        {
            bs.SetBlank(tag, word, false, true);
            Assert.AreEqual(null, tag.Blank);
        }

        [TestMethod]
        public void WithQuery_SetWordInverse_Comment()
        {
            tag.Query = q;

            bs.SetBlank(tag, word, false, true);
            Assert.IsTrue(tag.BlankType == BlankType.Comment);
            Assert.AreEqual(q, (tag.Blank as Comment).String);
        }

        #endregion SetBlank

        #region ConvertBlank

        [TestMethod]
        public void WithWord_CovertToComment()
        {
            tag.Blank = word;
            bs.ConvertBlank(tag, BlankType.Comment, () => { });
            Assert.IsTrue(tag.BlankType == BlankType.Comment);
            Assert.AreEqual(word.Title, (tag.Blank as Comment).String);
        }

        [TestMethod]
        public void WithMeasure_CovertToWord_ThisWord()
        {
            tag.Blank = new Measure(1, word: word); // измерение без слова в теге не бывает
            bs.ConvertBlank(tag, BlankType.Word, () => { });

            Assert.AreEqual(word, tag.Blank);
        }

        [TestMethod]
        public void WithMeasure_CovertToComment_()
        {
            var m = new Measure(1, word: word);
            tag.Blank = m;// измерение без слова в теге не бывает
            bs.ConvertBlank(tag, BlankType.Comment, () => { });

            Assert.AreEqual(m.ToString(), (tag.Blank as Comment).String);
        }

        [TestMethod]
        public void Empty_CovertToComment()
        {
            bs.ConvertBlank(tag, BlankType.Comment, () => { });
            Assert.IsTrue(tag.BlankType == BlankType.Comment);
            Assert.AreEqual("", (tag.Blank as Comment).String);
        }

        [TestMethod]
        public void Empty_CovertToWord()
        {
            bs.ConvertBlank(tag, BlankType.Word, () => { });
            Assert.IsTrue(tag.BlankType == BlankType.Word);
            Assert.AreEqual("", (tag.Blank as Word).Title);
        }

        [TestMethod]
        public void WithQuery_CovertToWord()
        {
            tag.Query = q;

            bs.ConvertBlank(tag, BlankType.Word, () => { });
            Assert.IsTrue(tag.BlankType == BlankType.Word);
            Assert.AreEqual(q, (tag.Blank as Word).Title);
        }

        #endregion ConvertBlank
    }
}