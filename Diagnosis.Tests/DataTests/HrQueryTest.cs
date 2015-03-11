﻿using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Diagnosis.Common;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class HrQueryTest : InMemoryDatabaseTest
    {
        [TestInitialize]
        public void Init()
        {
            hrIds.ForAll((id) => hr[id] = session.Get<HealthRecord>(IntToGuid<HealthRecord>(id)));
            wIds.ForAll((id) => w[id] = session.Get<Word>(IntToGuid<Word>(id)));
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

            hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[1], w[5] }, HealthRecordQueryAndScope.Appointment);

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