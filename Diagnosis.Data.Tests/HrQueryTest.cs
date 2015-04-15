using Diagnosis.Common;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Diagnosis.Data.Tests
{
    [TestClass]
    public class HrQueryTest : InMemoryDatabaseTest
    {
        [TestInitialize]
        public void Init()
        {
            Load<HealthRecord>();
            Load<Word>();
        }

        [TestMethod]
        public void GetHealthRecordWithAllWords()
        {
            var hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[1], w[2] }, HealthRecordQueryAndScope.HealthRecord);

            Assert.IsTrue(hrs.Count() == 1);
            Assert.IsTrue(hrs.Contains(hr[1]));

            hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[1] }, HealthRecordQueryAndScope.HealthRecord);

            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
        }

        [TestMethod]
        public void GetHealthRecordWithAllWordsInApp()
        {
            var hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[5], w[51] }, HealthRecordQueryAndScope.Appointment);

            Assert.IsTrue(hrs.Count() == 2);
            Assert.IsTrue(hrs.Contains(hr[30]));
            Assert.IsTrue(hrs.Contains(hr[31]));

            hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[1], w[2] }, HealthRecordQueryAndScope.Appointment);

            Assert.IsTrue(hrs.Count() == 2);
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));

            hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[5], w[22] }, HealthRecordQueryAndScope.Appointment);

            Assert.IsTrue(hrs.Count() == 0);

            hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[1], w[5] }, HealthRecordQueryAndScope.Appointment);

            Assert.IsTrue(hrs.Count() == 0);
        }

        [TestMethod]
        public void GetHealthRecordWithAllWordsInCourse()
        {
            var hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[5], w[51] }, HealthRecordQueryAndScope.Course);

            Assert.IsTrue(hrs.Count() == 2);
            Assert.IsTrue(hrs.Contains(hr[30]));
            Assert.IsTrue(hrs.Contains(hr[31]));

            hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[1], w[2] }, HealthRecordQueryAndScope.Course);

            Assert.IsTrue(hrs.Count() == 5);
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[21]));
            Assert.IsTrue(hrs.Contains(hr[22]));

            hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[5], w[22] }, HealthRecordQueryAndScope.Course);

            Assert.IsTrue(hrs.Count() == 2);
            Assert.IsTrue(hrs.Contains(hr[30]));
            Assert.IsTrue(hrs.Contains(hr[40]));

            hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[1], w[22] }, HealthRecordQueryAndScope.Course);

            Assert.IsTrue(hrs.Count() == 6);
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[21]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[70]));

        }

        [TestMethod]
        public void GetHealthRecordsWithAnyWord()
        {
            var hrs = HealthRecordQuery.WithAnyWord(session)(new Word[] { w[1], w[3] });

            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[32]));

            hrs = HealthRecordQuery.WithAnyWord(session)(new Word[] { w[1] });

            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));

            hrs = HealthRecordQuery.WithAnyWord(session)(new Word[] { w[100] });

            Assert.IsTrue(hrs.Count() == 0);
        }
        [TestMethod]
        public void GetHealthRecordsWithAnyEmptyWord()
        {
            var hrs = HealthRecordQuery.WithAnyWord(session)(new Word[] { });

            Assert.IsTrue(hrs.Count() == 0);
        }

        [TestMethod]
        public void GetHealthRecordsWithAllAndAnyWords()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { w[1] }, new Word[] { w[2], w[3] }, new Word[] { });

            Assert.IsTrue(hrs.Count() == 3);
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[20]));

        }

        [TestMethod]
        public void GetHealthRecordsWithAllAndAtLeastOneFromAnyWords()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { w[1], w[2] }, new Word[] { w[3] }, new Word[] { });

            Assert.IsTrue(hrs.Count() == 0);
        }

        [TestMethod]
        public void GetHealthRecordsWithAllAndNotAnyWords()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { w[1] }, new Word[] { }, new Word[] { w[2], w[3] });

            Assert.IsTrue(hrs.Count() == 3);
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[21]));
            Assert.IsTrue(hrs.Contains(hr[72]));
        }

        [TestMethod]
        public void GetHealthRecordsWithAnyAndNotAnyWords()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { }, new Word[] { w[4], w[31] }, new Word[] { w[5], w[3] });

            Assert.IsTrue(hrs.Count() == 2);
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[31]));
        }

        [TestMethod]
        public void GetHealthRecordsWithoutAnyWords()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { }, new Word[] { }, new Word[] { w[5], w[22], w[1] });

            Assert.IsTrue(hrs.Contains(hr[31]));
            Assert.IsTrue(hrs.Contains(hr[32]));
            Assert.IsTrue(hrs.Contains(hr[71]));
        }
    }
}