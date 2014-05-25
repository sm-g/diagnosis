using Diagnosis.App;
using Diagnosis.App.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;

namespace Tests
{
    [TestClass]
    public class RepositoryTest
    {
        [TestMethod]
        public void TestHealthRecordGetWithAllWords()
        {
            var repo = new HealthRecordRepository();
            var word1 = new WordRepository().GetById(3);
            var word2 = new WordRepository().GetById(1);
            var words = new[] { word1, word2 };

            var comparator = new CompareWord();
            var hrIds = repo.GetAll().Where(hr => hr.Symptom != null
                && hr.Symptom.Words.OrderBy(w => w, comparator).SequenceEqual(
                              words.OrderBy(w => w, comparator))).Select(a => a.Id);

            var hrs = repo.GetWithAllWords(words);
            Assert.IsNotNull(hrs);
            Assert.IsTrue(hrs.Count() == 1);
            Assert.IsTrue(hrs.First().Id == hrIds.First());
        }

        [TestMethod]
        public void TestHealthRecordGetByWords()
        {
            var repo = new HealthRecordRepository();
            var word1 = new WordRepository().GetById(3);
            var word2 = new WordRepository().GetById(1);
            var words = new[] { word1, word2 };

            var comparator = new CompareWord();
            var hrs = repo.GetAll().Where(hr => hr.Symptom != null
                && hr.Symptom.Words.Any(w => words.Contains(w))).ToList();

            var hrsFromRepo = repo.GetByWords(words);
            Assert.IsNotNull(hrsFromRepo);
            Assert.IsTrue(hrsFromRepo.Count() > 1);
            Assert.IsTrue(hrsFromRepo.SequenceEqual(hrs));
        }
    }
}
