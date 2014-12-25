﻿using Diagnosis.Models;
using Diagnosis.ViewModels.Search.Autocomplete;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class AutocompleteTest : InMemoryDatabaseTest
    {
        private Recognizer r;
        private AutocompleteViewModel a;
        private string q;
        private string qFull;
        private Word word;
        private static string notExistQ = "qwe";

        public TagViewModel First { get { return a.Tags.First(); } }

        public IHrItemObject FirstItem { get { return First.Entities.First(); } }

        public object FirstBlank { get { return First.Blank; } }

        [TestInitialize]
        public void AutocompleteTestInit()
        {
            r = new Recognizer(session);
            a = new AutocompleteViewModel(r, true, true, false, null);
            word = session.Get<Word>(IntToGuid<Word>(1));
            q = word.Title.Substring(0, word.Title.Length - 1);
            qFull = word.Title;

            a.SelectedTag = a.Tags.Last();
        }

        [TestMethod]
        public void Create()
        {
            Assert.IsTrue(a.Tags.Count == 1);
            Assert.IsTrue(a.Tags.Last().State == TagViewModel.States.Init);
            Assert.IsTrue(a.Tags.Last().IsLast);
        }

        [TestMethod]
        public void TypeQuery()
        {
            a.SelectedTag.Query = "123";
            Assert.IsTrue(a.SelectedTag.State == TagViewModel.States.Typing);
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

        /// <summary>
        /// Можно вводить такое же слово.
        /// </summary>
        [TestMethod]
        public void QueryExistingWordTwice()
        {
            a.SelectedTag.Query = qFull;
            a.EnterCommand.Execute(a.SelectedTag);

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
            Assert.IsTrue(a.Tags.Last().State == TagViewModel.States.Init);
            Assert.IsTrue(a.SelectedTag == a.Tags.Last());
        }

        [TestMethod]
        public void AcceptExisting()
        {
            a.SelectedTag.Query = q;
            a.EnterCommand.Execute(a.SelectedTag);

            Assert.IsTrue(First.State == TagViewModel.States.Completed);
            Assert.IsTrue(First.BlankType == TagViewModel.BlankTypes.Word);
            Assert.AreEqual(word, First.Blank);

            var entities = a.GetEntities();

            Assert.AreEqual(word, entities.Single());
            Assert.AreEqual(word, FirstItem);
        }

        [TestMethod]
        public void InverseAcceptExisting()
        {
            a.SelectedTag.Query = q;
            a.InverseEnterCommand.Execute(a.SelectedTag);

            Assert.IsTrue(First.State == TagViewModel.States.Completed);
            Assert.AreEqual(q, First.Blank);
            Assert.IsTrue(First.BlankType == TagViewModel.BlankTypes.Query);

            var entities = a.GetEntities();

            Assert.IsTrue(FirstItem is Comment);
            Assert.IsTrue((FirstItem as Comment).String == q);
        }

        [TestMethod]
        public void AcceptNotExisting()
        {
            a.SelectedTag.Query = notExistQ;
            a.EnterCommand.Execute(a.SelectedTag);

            Assert.IsTrue(First.State == TagViewModel.States.Completed);
            Assert.AreEqual(notExistQ, First.Blank);
            Assert.IsTrue(First.BlankType == TagViewModel.BlankTypes.Query);

            var entities = a.GetEntities();

            Assert.IsTrue(FirstItem is Comment);
            Assert.IsTrue((FirstItem as Comment).String == notExistQ);
        }

        [TestMethod]
        public void InverseAcceptNotExisting()
        {
            a.SelectedTag.Query = notExistQ;
            a.InverseEnterCommand.Execute(a.SelectedTag);

            Assert.IsTrue(First.State == TagViewModel.States.Completed);
            Assert.IsTrue(First.BlankType == TagViewModel.BlankTypes.Word);
            Assert.IsTrue(First.Signalization == Signalizations.NewWord);

            var newW = First.Blank as Word;
            Assert.IsTrue(newW.Title == notExistQ);
            Assert.IsTrue(newW.IsTransient);

            var entities = a.GetEntities();

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
            var a = new AutocompleteViewModel(r, true, true, true, null);
            Assert.IsTrue(a.Tags.Count == 1);

            a.AddTag(word);
            Assert.AreEqual(word, a.LastTag.Blank);
            Assert.IsTrue(a.Tags.Count == 1);
        }

        [TestMethod]
        public void CopyPaste()
        {
            a.AddTag(word);
            a.SelectedTag = a.Tags.First();
            a.CopySelected();

            a.Paste();

            Assert.AreEqual(3, a.Tags.Count);
            Assert.AreEqual(a.Tags[0].Blank, a.Tags[1].Blank);
        }

        [TestMethod]
        public void Cut()
        {
            a.AddTag(word);
            a.SelectedTag = a.Tags.First();
            a.CutSelected();

            Assert.AreEqual(1, a.Tags.Count);
        }
    }
}