using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.Tests;
using Diagnosis.ViewModels.Autocomplete;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class SuggestionsMakerTest : InMemoryDatabaseTest
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
        public void CreateWordSaveDeleteCreateAgain()
        {
            var word = r.FirstMatchingOrNewWord(notExistQ);
            // слово удаляется из created при сохранении
            session.Save(word);

            session.Delete(word);

            var word2 = r.FirstMatchingOrNewWord(notExistQ);
            Assert.IsTrue(word2.IsTransient);
            Assert.AreNotEqual(word, word2);

            var tryToGetWordBut = SuggestionsMaker.GetWordFromCreated(word2);
            Assert.AreEqual(word2, tryToGetWordBut);
        }
    }
}