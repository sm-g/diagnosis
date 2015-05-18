using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.Tests;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.ViewModels.Screens;
using Diagnosis.ViewModels.Search;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class SuggestionsMakerTest : ViewModelTest
    {
        private SuggestionsMaker r;

        private static string notExistQ = "qwe";

        [TestInitialize]
        public void RecognizerTestInit()
        {
            Load<Doctor>();
            Load<Word>();
            r = new SuggestionsMaker(session, clearCreated: true);
            AuthorityController.TryLogIn(d1);
        }

        [TestMethod]
        public void FirstMatchingReturnsWordsNotInDoctorVocs()
        {
            // слова становятся видны доктору только после сохранения записи
            AuthorityController.TryLogIn(d2);
            var word = r.FirstMatchingOrNewWord(notExistQ);
            Assert.IsTrue(!d2.Words.Contains(word));
            Assert.IsTrue(!d2.CustomVocabulary.Words.Contains(word));

            Word wForD1Only = w[1];
            Assert.IsTrue(!d2.Words.Contains(wForD1Only));

            var word2 = r.FirstMatchingOrNewWord(wForD1Only.Title);
            Assert.IsTrue(!d2.Words.Contains(word2));
            Assert.IsTrue(!d2.CustomVocabulary.Words.Contains(word2));
            Assert.AreEqual(wForD1Only, word2);
        }

        [TestMethod]
        public void CreateWord_Save_Delete_CreateAgain()
        {
            var word = r.FirstMatchingOrNewWord(notExistQ);
            // слово удаляется из created при сохранении
            session.Save(word);

            session.Delete(word);

            var word2 = r.FirstMatchingOrNewWord(notExistQ);
            Assert.IsTrue(word2.IsTransient);
            Assert.AreNotEqual(word, word2);

            var wordInCreated = SuggestionsMaker.GetSameWordFromCreated(word2);
            Assert.AreEqual(null, wordInCreated);
        }

        [TestMethod]
        public void CreateWordInQueryEditor_ThanInWordList()
        {
            // внутри используется SuggestionsMaker.GetSameWordFromCreated

            var qe = new QueryEditorViewModel(session);
            var wl = new WordsListViewModel();

            qe.QueryBlocks[0].IsSelected = true; // prevent making options on AllEmpty changes after typing in autocomplete

            var auto = (qe.QueryBlocks[0].AutocompleteAll as AutocompleteViewModel);
            var w = auto.CompleteWord("1");

            var fromList = wl.AddWord("1");

            Assert.AreEqual(w, fromList);
            qe.Dispose();
            wl.Dispose();
        }

        [TestMethod]
        public void CreateSameWordInQueryEditors()
        {
            var qe = new QueryEditorViewModel(session);
            var auto = (qe.QueryBlocks[0].AutocompleteAll as AutocompleteViewModel);
            var w1 = auto.CompleteWord("1");
            var w2 = auto.CompleteWord("1");

            var auto2 = (qe.QueryBlocks[0].AutocompleteAll as AutocompleteViewModel);
            var qe2 = new QueryEditorViewModel(session);
            var w3 = auto2.CompleteWord("1");

            Assert.AreEqual(w1, w2);
            Assert.AreEqual(w1, w3);
            qe.Dispose();
            qe2.Dispose();
        }
    }
}