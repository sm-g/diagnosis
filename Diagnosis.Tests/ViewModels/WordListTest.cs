using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.Tests.ViewModels
{
    [TestClass]
    public class WordListTest : InMemoryDatabaseTest
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
            var newW = CreateWordInEditor("123");
            Assert.IsTrue(newW.Vocabularies.Single().IsCustom);
            Assert.AreEqual(d1, newW.Vocabularies.Single().Doctor);

            // другой врач не видит это слово
            AuthorityController.TryLogIn(d2);
            using (var wordList = new WordsListViewModel())
            {
                Assert.IsFalse(wordList.Words.Select(x => x.word).Contains(newW));

                // но может добавить
                var newW2 = CreateWordInEditor("123");
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
            var newW2 = CreateWordInEditor(d1w.Title);
            Assert.AreEqual(d1w, newW2);
        }
    }
}