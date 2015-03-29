using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Autocomplete;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class RecognizerTest : InMemoryDatabaseTest
    {
        private Recognizer r;
        private Doctor d1;
        private static string notExistQ = "qwe";
        private Doctor d2;

        [TestInitialize]
        public void RecognizerTestInit()
        {
            d1 = session.Get<Doctor>(IntToGuid<Doctor>(1));
            d2 = session.Get<Doctor>(IntToGuid<Doctor>(2));
            AuthorityController.TryLogIn(d1);
            r = new Recognizer(session, clearCreated: true);
            wIds.ForAll((id) => w[id] = session.Get<Word>(IntToGuid<Word>(id)));
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
    }
}