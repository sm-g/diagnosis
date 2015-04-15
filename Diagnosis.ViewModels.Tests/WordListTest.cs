using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class WordListTest : ViewModelTest
    {
        [TestInitialize]
        public void Init()
        {
            Load<Doctor>();
            Load<Word>();
            Load<Vocabulary>();

            AuthorityController.TryLogIn(d1);
        }

        [TestMethod]
        public void MultiCustom()
        {
            // отдельный пользовательский словарь на врача

            // первый создает слово
            var newW = CreateWordAsInEditor("123");
            Assert.IsTrue(newW.Vocabularies.Single().IsCustom);
            Assert.AreEqual(d1, newW.Vocabularies.Single().Doctor);

            // другой врач не видит это слово
            AuthorityController.TryLogIn(d2);
            using (var wordList = new WordsListViewModel())
            {
                Assert.IsFalse(wordList.Words.Select(x => x.word).Contains(newW));

                // но может добавить
                var newW2 = CreateWordAsInEditor("123");
                wordList.OnWordSaved(newW2);
                Assert.AreEqual(newW, newW2);

                // это слово в двух пользовательских словарях
                Assert.IsTrue(newW.Vocabularies.Count() == 2);
                Assert.IsTrue(newW.Vocabularies.All(x => x.IsCustom));

                // при удалении одним врачом остается для другого
                wordList.SelectWord(newW2);
                wordList.DeleteCommand.Execute(null);

                Assert.IsFalse(wordList.Words.Select(x => x.word).Contains(newW2));
                // пока врач удаляет как админ, сразу для всех врачей
                //Assert.AreEqual(d1, newW.Vocabularies.Single().Doctor);
            }
        }

        [TestMethod]
        public void CanCreateWordInHiddenVoc()
        {
            // врач создает слово, которое есть в недоступном ему словаре

            var d1w = w[1];
            Assert.IsTrue(d1.Words.Contains(d1w));

            AuthorityController.TryLogIn(d2);

            // другой врач не видит это слово
            Assert.IsFalse(d2.Words.Contains(d1w));

            // но может добавить
            var newW2 = CreateWordAsInEditor(d1w.Title);
            Assert.AreEqual(d1w, newW2);
        }

        [TestMethod]
        public void CanNotAddWordInOtherCase()
        {
            var newW = new Word(w[1].Title.ToUpper());
            using (var wEditor = new WordEditorViewModel(newW))
            {
                Assert.IsFalse(wEditor.OkCommand.CanExecute(null));
            }
        }
    }
}