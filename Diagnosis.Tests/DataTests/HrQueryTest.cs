using Diagnosis.Core;
using Diagnosis.Data.Queries;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class HrQueryTest : DbTestBase
    {
        [TestInitialize]
        public void Init()
        {

            using (var tx = session.BeginTransaction())
            {
                hr1.Symptom = s1;
                hr2.Symptom = s2;
                session.Update(hr1);
                session.Update(hr2);
                tx.Commit();
            }
        }

        [TestMethod]
        public void GetHealthRecordWithAllWords()
        {
            var hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w1, w2 });

            Assert.IsTrue(hrs.Count() == 1);
            Assert.IsTrue(hrs.Contains(hr1));

            hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w1 });

            Assert.IsTrue(hrs.Count() == 2);
            Assert.IsTrue(hrs.Contains(hr1));
            Assert.IsTrue(hrs.Contains(hr2));
        }

        [TestMethod]
        public void GetHealthRecordsWithAnyWord()
        {
            var hrs = HealthRecordQuery.WithAnyWord(session)(new Word[] { w1, w2 });

            Assert.IsTrue(hrs.Count() == 2);
            Assert.IsTrue(hrs.Contains(hr1));
            Assert.IsTrue(hrs.Contains(hr2));

            hrs = HealthRecordQuery.WithAnyWord(session)(new Word[] { w1 });

            Assert.IsTrue(hrs.Count() == 2);
            Assert.IsTrue(hrs.Contains(hr1));
            Assert.IsTrue(hrs.Contains(hr2));
        }
    }
}