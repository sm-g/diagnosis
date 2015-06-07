using Diagnosis.Common;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Diagnosis.Data.Tests
{
    [TestClass]
    public class HrQueryAllInScopeTest : InMemoryDatabaseTest
    {
        [TestInitialize]
        public void Init()
        {
            Load<HealthRecord>();
            Load<Word>();
        }

        #region WithAllWordsInScope

        [TestMethod]
        public void WithAllWords()
        {
            var hrs = HealthRecordQuery.WithAllWordsInScope(session)(new Word[] { w[1], w[2] }, HealthRecordQueryAndScope.HealthRecord);

            Assert.IsTrue(hrs.Count() == 1);
            Assert.IsTrue(hrs.Contains(hr[1]));
        }

        [TestMethod]
        public void WithAllWords2()
        {
            var hrs = HealthRecordQuery.WithAllWordsInScope(session)(new Word[] { w[1] }, HealthRecordQueryAndScope.HealthRecord);

            Assert.AreEqual(6, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[21]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[72]));
        }

        [TestMethod]
        public void WithAllWords3()
        {
            var hrs = HealthRecordQuery.WithAllWordsInScope(session)(new Word[] { w[1], w[3] }, HealthRecordQueryAndScope.HealthRecord);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[22]));
        }

        [TestMethod]
        public void WithAllWordsRepeat()
        {
            var hrs = HealthRecordQuery.WithAllWordsInScope(session)(new Word[] { w[22], w[22] }, HealthRecordQueryAndScope.HealthRecord);

            Assert.AreEqual(1, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[70]));
        }

        [TestMethod]
        public void WithAllWordsRepeatInPat()
        {
            var hrs = HealthRecordQuery.WithAllWordsInScope(session)(new Word[] { w[22], w[22], w[22] }, HealthRecordQueryAndScope.Patient);

            Assert.AreEqual(4, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[40]));
            Assert.IsTrue(hrs.Contains(hr[70]));
            Assert.IsTrue(hrs.Contains(hr[72]));
        }

        [TestMethod]
        public void WithAllWordsInApp()
        {
            var hrs = HealthRecordQuery.WithAllWordsInScope(session)(new Word[] { w[5], w[51] }, HealthRecordQueryAndScope.Appointment);

            Assert.IsTrue(hrs.Count() == 2);
            Assert.IsTrue(hrs.Contains(hr[30]));
            Assert.IsTrue(hrs.Contains(hr[31]));
        }

        [TestMethod]
        public void WithAllWordsInApp2()
        {
            var hrs = HealthRecordQuery.WithAllWordsInScope(session)(new Word[] { w[1], w[2] }, HealthRecordQueryAndScope.Appointment);

            Assert.IsTrue(hrs.Count() == 2);
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
        }

        [TestMethod]
        public void WithAllWordsInApp3()
        {
            var hrs = HealthRecordQuery.WithAllWordsInScope(session)(new Word[] { w[5], w[22] }, HealthRecordQueryAndScope.Appointment);

            Assert.IsTrue(hrs.Count() == 0);
        }

        [TestMethod]
        public void WithAllWordsInApp4()
        {
            var hrs = HealthRecordQuery.WithAllWordsInScope(session)(new Word[] { w[1], w[5] }, HealthRecordQueryAndScope.Appointment);

            Assert.IsTrue(hrs.Count() == 0);
        }

        [TestMethod]
        public void WithAllWordsInCourse()
        {
            var hrs = HealthRecordQuery.WithAllWordsInScope(session)(new Word[] { w[5], w[51] }, HealthRecordQueryAndScope.Course);

            Assert.IsTrue(hrs.Count() == 2);
            Assert.IsTrue(hrs.Contains(hr[30]));
            Assert.IsTrue(hrs.Contains(hr[31]));
        }

        [TestMethod]
        public void WithAllWordsInCourse2()
        {
            var hrs = HealthRecordQuery.WithAllWordsInScope(session)(new Word[] { w[1], w[2] }, HealthRecordQueryAndScope.Course);

            Assert.IsTrue(hrs.Count() == 5);
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[21]));
            Assert.IsTrue(hrs.Contains(hr[22]));
        }

        [TestMethod]
        public void WithAllWordsInCourse3()
        {
            var hrs = HealthRecordQuery.WithAllWordsInScope(session)(new Word[] { w[5], w[22] }, HealthRecordQueryAndScope.Course);

            Assert.IsTrue(hrs.Count() == 2);
            Assert.IsTrue(hrs.Contains(hr[30]));
            Assert.IsTrue(hrs.Contains(hr[40]));
        }

        [TestMethod]
        public void WithAllWordsInCourse4()
        {
            var hrs = HealthRecordQuery.WithAllWordsInScope(session)(new Word[] { w[1], w[22] }, HealthRecordQueryAndScope.Course);

            Assert.IsTrue(hrs.Count() == 6);
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[21]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[70]));
        }

        #endregion WithAllWordsInScope
    }
}