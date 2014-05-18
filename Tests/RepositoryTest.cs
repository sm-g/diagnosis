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
        public void TestHealthRecordGetByWords()
        {
            var repo = new HealthRecordRepository();
            var word1 = new WordRepository().GetById(3);
            var word2 = new WordRepository().GetById(1);
            var words = new[] { word1, word2 };

            var comparator = new CompareWord();
            var hrIds = repo.GetAll().Where(hr => hr.Symptom != null
                && hr.Symptom.Words.OrderBy(w => w, comparator).SequenceEqual(
                              words.OrderBy(w => w, comparator))).Select(a => a.Id);

            var hrs = repo.GetByWords(words);
            Assert.IsNotNull(hrs);
            Assert.IsTrue(hrs.FirstOrDefault().Id == hrIds.FirstOrDefault());
        }
    }
}
