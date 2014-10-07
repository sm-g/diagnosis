using Diagnosis.Core;
using Diagnosis.Data.Queries;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class HrQueryTest : InMemoryDatabaseTest
    {
        protected Word w1;
        protected Word w2;
        protected HealthRecord hr1;
        protected HealthRecord hr2;

        [TestInitialize]
        public void Init()
        {
            w1 = session.Get<Word>(1);
            w2 = session.Get<Word>(2);
            hr1 = session.Get<HealthRecord>(1);
            hr2 = session.Get<HealthRecord>(2);
        }

        [TestMethod]
        public void GetHealthRecordWithAllWords()
        {
            var hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w1, w2 });

            Assert.IsTrue(hrs.Count() == 1);
            Assert.IsTrue(hrs.Contains(hr1));

            hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w1 });

            Assert.IsTrue(hrs.Contains(hr1));
            Assert.IsTrue(hrs.Contains(hr2));
        }

        [TestMethod]
        public void GetHealthRecordsWithAnyWord()
        {
            var hrs = HealthRecordQuery.WithAnyWord(session)(new Word[] { w1, w2 });

            Assert.IsTrue(hrs.Contains(hr1));
            Assert.IsTrue(hrs.Contains(hr2));

            hrs = HealthRecordQuery.WithAnyWord(session)(new Word[] { w1 });

            Assert.IsTrue(hrs.Contains(hr1));
            Assert.IsTrue(hrs.Contains(hr2));
        }
    }
}