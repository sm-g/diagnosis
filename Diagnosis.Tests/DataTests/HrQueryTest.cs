using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class HrQueryTest : InMemoryDatabaseTest
    {
        private Dictionary<int, HealthRecord> hr = new Dictionary<int, HealthRecord>();
        private Dictionary<int, Word> w = new Dictionary<int, Word>();

        [TestInitialize]
        public void Init()
        {
            var hrIds = new[] { 1, 2, 20, 21, 22, 30, 31, 32, 40, 70, 71, 72, 73, 74 };
            var wIds = new[] { 1, 2, 3, 4, 5, 22, 51, 94 };

            foreach (var id in hrIds)
            {
                hr[id] = session.Get<HealthRecord>(id);
            }
            foreach (var id in wIds)
            {
                w[id] = session.Get<Word>(id);
            }
        }

        [TestMethod]
        public void GetHealthRecordWithAllWords()
        {
            var hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[1], w[2] }, HealthRecordQuery.AndScopes.HealthRecord);

            Assert.IsTrue(hrs.Count() == 1);
            Assert.IsTrue(hrs.Contains(hr[1]));

            hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[1] }, HealthRecordQuery.AndScopes.HealthRecord);

            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
        }

        [TestMethod]
        public void GetHealthRecordWithAllWordsInApp()
        {
            var hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[5], w[51] }, HealthRecordQuery.AndScopes.Appointment);

            Assert.IsTrue(hrs.Count() == 2);
            Assert.IsTrue(hrs.Contains(hr[30]));
            Assert.IsTrue(hrs.Contains(hr[31]));

            hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[1], w[2] }, HealthRecordQuery.AndScopes.Appointment);

            Assert.IsTrue(hrs.Count() == 2);
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));

            hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[5], w[22] }, HealthRecordQuery.AndScopes.Appointment);

            Assert.IsTrue(hrs.Count() == 0);
        }

        [TestMethod]
        public void GetHealthRecordWithAllWordsInCourse()
        {
            var hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[5], w[51] }, HealthRecordQuery.AndScopes.Course);

            Assert.IsTrue(hrs.Count() == 2);
            Assert.IsTrue(hrs.Contains(hr[30]));
            Assert.IsTrue(hrs.Contains(hr[31]));

            hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[1], w[2] }, HealthRecordQuery.AndScopes.Course);

            Assert.IsTrue(hrs.Count() == 5);
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[21]));
            Assert.IsTrue(hrs.Contains(hr[22]));

            hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[5], w[22] }, HealthRecordQuery.AndScopes.Course);

            Assert.IsTrue(hrs.Count() == 2);
            Assert.IsTrue(hrs.Contains(hr[30]));
            Assert.IsTrue(hrs.Contains(hr[40]));

            hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[1], w[22] }, HealthRecordQuery.AndScopes.Course);

            Assert.IsTrue(hrs.Count() == 6);
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[21]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[70]));

            hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[1], w[5] }, HealthRecordQuery.AndScopes.Appointment);

            Assert.IsTrue(hrs.Count() == 0);
        }

        [TestMethod]
        public void GetHealthRecordsWithAnyWord()
        {
            var hrs = HealthRecordQuery.WithAnyWord(session)(new Word[] { w[1], w[2] });

            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));

            hrs = HealthRecordQuery.WithAnyWord(session)(new Word[] { w[1] });

            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
        }
    }
}