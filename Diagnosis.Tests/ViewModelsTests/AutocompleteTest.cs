using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Autocomplete;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class AutocompleteTest : InMemoryDatabaseTest
    {
        private Recognizer r;
        private new AutocompleteViewModel a;
        private string q;
        private string qFull;
        private Word word;
        private IcdDisease icd1;
        private static string notExistQ = "qwe";

        public TagViewModel First { get { return a.Tags.First(); } }

        public IHrItemObject FirstItem { get { return First.Entity; } }

        public object FirstBlank { get { return First.Blank; } }

        [TestInitialize]
        public void AutocompleteTestInit()
        {
            r = new Recognizer(session, clearCreated: true);
            a = new AutocompleteViewModel(r, true, true, true, false, null);
            word = session.Get<Word>(IntToGuid<Word>(1));
            icd1 = session.Get<IcdDisease>(1);
            q = word.Title.Substring(0, word.Title.Length - 1);
            qFull = word.Title;

            a.SelectedTag = a.Tags.Last();
        }

        [TestMethod]
        public void Create()
        {
            Assert.IsTrue(a.Tags.Count == 1);
            Assert.IsTrue(a.Tags.Last().State == State.Init);
            Assert.IsTrue(a.Tags.Last().IsLast);
        }

        [TestMethod]
        public void TypeQuery()
        {
            a.SelectedTag.Query = "123";
            Assert.IsTrue(a.SelectedTag.State == State.Typing);
        }

        [TestMethod]
        public void QueryExistingWordStart()
        {
            a.SelectedTag.Query = q;

            Assert.IsTrue(a.Suggestions.Count > 0);
            Assert.IsTrue(a.SelectedSuggestion.ToString() == qFull);
            Assert.IsTrue(a.IsPopupOpen);
        }

        [TestMethod]
        public void QueryExistingWordFull()
        {
            a.SelectedTag.Query = qFull;

            Assert.IsTrue(a.Suggestions.Count > 0);
            Assert.IsTrue(a.SelectedSuggestion.ToString() == qFull);
            Assert.IsTrue(a.IsPopupOpen);
        }

        [TestMethod]
        public void QueryNotExisting()
        {
            a.SelectedTag.Query = notExistQ;

            Assert.IsTrue(a.Suggestions.Count == 0);
            Assert.IsFalse(a.IsPopupOpen);
        }

        [TestMethod]
        public void QueryNotExistingCreated()
        {
            a.SelectedTag.Query = notExistQ;
            a.InverseEnterCommand.Execute(a.SelectedTag);

            a.SelectedTag.Query = notExistQ;

            Assert.AreEqual(1, a.Suggestions.Count);
            Assert.IsTrue(a.IsPopupOpen);
        }
        [TestMethod]
        public void QueryNotExistingCreatedInOtherAutocomplete()
        {
            a.SelectedTag.Query = notExistQ;
            a.InverseEnterCommand.Execute(a.SelectedTag);

            var r = new Recognizer(session);
            var other = new AutocompleteViewModel(r, true, true, true, false, null);
            other.StartEdit();
            other.SelectedTag.Query = notExistQ;

            Assert.AreEqual(1, other.Suggestions.Count);
            Assert.IsTrue(other.IsPopupOpen);
        }
        /// <summary>
        /// Можно вводить такое же слово.
        /// </summary>
        [TestMethod]
        public void QueryExistingWordTwice()
        {
            a.SelectedTag.Query = qFull;
            a.EnterCommand.Execute(a.SelectedTag);

            a.LastTag.AddRightCommand.Execute(null);// ensure selected is new tag

            a.SelectedTag.Query = qFull;
            Assert.IsTrue(a.Suggestions.Count > 0);
            Assert.IsTrue(a.SelectedSuggestion.ToString() == qFull);
        }

        [TestMethod]
        public void Accept()
        {
            a.SelectedTag.Query = q;
            a.EnterCommand.Execute(a.SelectedTag);

            Assert.IsTrue(a.Tags.Count == 2);
            Assert.IsTrue(a.Tags.Last().IsLast);
            Assert.IsTrue(a.Tags.Last().State == State.Init);
            Assert.IsTrue(a.SelectedTag == a.Tags.Last());
        }

        [TestMethod]
        public void AcceptExisting()
        {
            a.SelectedTag.Query = q;
            a.EnterCommand.Execute(a.SelectedTag);

            Assert.IsTrue(First.State == State.Completed);
            Assert.IsTrue(First.BlankType == BlankType.Word);
            Assert.AreEqual(word, First.Blank);

            var entities = a.GetCHIOs();

            Assert.AreEqual(word, entities.Single().HIO);
            Assert.AreEqual(word, FirstItem);
        }

        [TestMethod]
        public void InverseAcceptExisting()
        {
            a.SelectedTag.Query = q;
            a.InverseEnterCommand.Execute(a.SelectedTag);

            Assert.IsTrue(First.State == State.Completed);
            Assert.AreEqual(q, First.Blank.ToString());
            Assert.IsTrue(First.BlankType == BlankType.Comment);

            var entities = a.GetCHIOs();

            Assert.IsTrue(FirstItem is Comment);
            Assert.IsTrue((FirstItem as Comment).String == q);
        }

        [TestMethod]
        public void AcceptNotExisting()
        {
            a.SelectedTag.Query = notExistQ;
            a.EnterCommand.Execute(a.SelectedTag);

            Assert.IsTrue(First.State == State.Completed);
            Assert.AreEqual(notExistQ, First.Blank.ToString());
            Assert.IsTrue(First.BlankType == BlankType.Comment);

            var entities = a.GetCHIOs();

            Assert.IsTrue(FirstItem is Comment);
            Assert.IsTrue((FirstItem as Comment).String == notExistQ);
        }

        [TestMethod]
        public void InverseAcceptNotExisting()
        {
            a.SelectedTag.Query = notExistQ;
            a.InverseEnterCommand.Execute(a.SelectedTag);

            Assert.IsTrue(First.State == State.Completed);
            Assert.IsTrue(First.BlankType == BlankType.Word);
            Assert.IsTrue(First.Signalization == Signalizations.NewWord);

            var newW = First.Blank as Word;
            Assert.IsTrue(newW.Title == notExistQ);
            Assert.IsTrue(newW.IsTransient);

            var entities = a.GetCHIOs();

            Assert.AreEqual(newW, FirstItem);
        }

        /// <summary>
        /// Повторное новое слово не создается заново.
        /// </summary>
        [TestMethod]
        public void AcceptNotExistingAfterInverseAccept()
        {
            a.SelectedTag.Query = notExistQ;
            a.InverseEnterCommand.Execute(a.SelectedTag);

            a.SelectedTag.Query = notExistQ;

            Assert.IsTrue(a.SelectedSuggestion.ToString() == notExistQ);

            a.EnterCommand.Execute(a.SelectedTag);
            var second = a.Tags.ElementAt(1);

            Assert.IsTrue(second.Signalization == Signalizations.NewWord);
            Assert.IsTrue(second.Blank == First.Blank);
        }

        [TestMethod]
        public void AddTagWhenSingleTag()
        {
            var a = new AutocompleteViewModel(r, true, true, true, true, null);
            Assert.IsTrue(a.Tags.Count == 1);

            a.AddTag(word);
            Assert.AreEqual(word, a.LastTag.Blank);
            Assert.IsTrue(a.Tags.Count == 1);
        }
        [TestMethod]
        public void AddMeasureWhenTyping()
        {
            var d1 = session.Get<Doctor>(IntToGuid<Doctor>(1));
            AuthorityController.TryLogIn(d1);

            var hre = new Diagnosis.ViewModels.Screens.HrEditorViewModel(session);
            hre.Load(session.Get<HealthRecord>(IntToGuid<HealthRecord>(1)));
            var a = hre.Autocomplete;

            a.StartEdit(a.Tags.First());
            a.SelectedTag.Query = "123";

            // from measureEditor
            a.AddTag(new Measure(5));
        }
        [TestMethod]
        public void CopyPasteWord()
        {
            a.AddTag(word);
            a.SelectedTag = a.Tags.First();
            a.Copy();

            a.Paste();

            Assert.AreEqual(3, a.Tags.Count);
            Assert.AreEqual(word, a.Tags[1].Blank);
        }

        [TestMethod]
        public void CutPasteWord()
        {
            a.AddTag(word);
            a.SelectedTag = a.Tags.First();
            a.Cut();

            Assert.AreEqual(1, a.Tags.Count);

            a.Paste();

            Assert.AreEqual(word, a.Tags[0].Blank);
        }

        [TestMethod]
        public void CopyPasteMeasure()
        {
            var m = new Measure(0) { Word = word };
            a.AddTag(m);
            a.SelectedTag = a.Tags.First();
            a.Copy();

            a.Paste();

            Assert.AreEqual(3, a.Tags.Count);
            Assert.AreEqual(m, a.Tags[1].Blank);
        }

        [TestMethod]
        public void CopyPasteIcd()
        {
            a.AddTag(icd1);
            a.SelectedTag = a.Tags.First();
            a.Copy();

            a.Paste();

            Assert.AreEqual(3, a.Tags.Count);
            Assert.AreEqual(icd1, a.Tags[1].Blank);
        }

        [TestMethod]
        public void CopyPasteToOtherAutocomplete()
        {
            a.AddTag(word);
            a.SelectedTag = a.Tags.First();
            a.Copy();

            var r2 = new Recognizer(session);
            var a2 = new AutocompleteViewModel(r2, true, true, true, false, null);
            a2.Paste();

            Assert.AreEqual(word, a2.Tags[0].Blank);
        }

        [TestMethod]
        public void CopyPasteNewWordToOtherAutocomplete()
        {
            var w = new Word("11");
            a.AddTag(w);
            a.SelectedTag = a.Tags.First();
            a.Copy();

            var r2 = new Recognizer(session);
            var a2 = new AutocompleteViewModel(r2, true, true, true, false, null);

            new Saver(session).Save(new[] { w });
            a2.Paste();

            Assert.AreEqual(w, a2.Tags[0].Blank);
        }
    }
}