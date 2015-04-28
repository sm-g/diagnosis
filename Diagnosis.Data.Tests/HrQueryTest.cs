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
        #endregion

        [TestMethod]
        public void WithAnyWord()
        {
            var hrs = HealthRecordQuery.WithAnyWord(session)(new Word[] { w[1], w[3] });

            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[32]));
        }

        [TestMethod]
        public void WithAnyWord2()
        {
            var hrs = HealthRecordQuery.WithAnyWord(session)(new Word[] { w[1] });

            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
        }

        [TestMethod]
        public void WithAnyWord3()
        {
            var hrs = HealthRecordQuery.WithAnyWord(session)(new Word[] { w[100] });

            Assert.IsTrue(hrs.Count() == 0);
        }

        [TestMethod]
        public void WithAnyEmptyWord()
        {
            var hrs = HealthRecordQuery.WithAnyWord(session)(new Word[] { });

            Assert.IsTrue(hrs.Count() == 0);
        }

        [TestMethod]
        public void WithoutAnyWord()
        {
            var hrs = HealthRecordQuery.WithoutAnyWord(session)(new Word[] { w[1], w[3], w[22] });

            Assert.IsTrue(hrs.Contains(hr[71]));
            Assert.IsTrue(hrs.Contains(hr[31]));
            Assert.IsTrue(hrs.Contains(hr[30]));
        }

        [TestMethod]
        public void WithAnyWordsAtLeastTwoWords()
        {
            var hrs = HealthRecordQuery.WithAnyWords(session)(new Word[] { w[1], w[22], w[3] }, 2);

            Assert.AreEqual(4, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[70])); // одно слово дважды
            Assert.IsTrue(hrs.Contains(hr[72]));
        }

        [TestMethod]
        public void WithAnyWordsAtLeastOneWord_SameAsWithAnyWord()
        {
            var hrs = HealthRecordQuery.WithAnyWords(session)(new Word[] { w[1], w[3] }, 1);

            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[32]));

            hrs = HealthRecordQuery.WithAnyWords(session)(new Word[] { w[1] }, 1);

            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));

            hrs = HealthRecordQuery.WithAnyWords(session)(new Word[] { w[100] }, 1);

            Assert.IsTrue(hrs.Count() == 0);
        }

        [TestMethod]
        public void WithAnyAtLeastTwoSameWords()
        {
            var hrs = HealthRecordQuery.WithAnyWords(session)(new Word[] { w[22] }, 2);

            Assert.AreEqual(1, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[70])); // одно слово дважды
        }

        #region WithAllAnyNotWords

        [TestMethod]
        public void WithAllAndAnyWords()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { w[1] }, new Word[] { w[2], w[3] }, new Word[] { });

            Assert.IsTrue(hrs.Count() == 3);
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[20]));
        }

        [TestMethod]
        public void WithAllAndOneFromAnyWords()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { w[1], w[2] }, new Word[] { w[3] }, new Word[] { });

            Assert.IsTrue(hrs.Count() == 0);
        }

        [TestMethod]
        public void WithAllAndNotAnyWords()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { w[1] }, new Word[] { }, new Word[] { w[2], w[3] });

            Assert.IsTrue(hrs.Count() == 3);
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[21]));
            Assert.IsTrue(hrs.Contains(hr[72]));
        }

        [TestMethod]
        public void WithAnyAndNotAnyWords()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { }, new Word[] { w[4], w[31] }, new Word[] { w[5], w[3] });

            Assert.IsTrue(hrs.Count() == 2);
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[31]));
        }

        public void WithAllOfWords()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { w[1], w[3] }, new Word[] { }, new Word[] { });

            Assert.IsTrue(hrs.Count() == 2);
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[20]));
        }

        public void WithAnyWords()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { }, new Word[] { w[94], w[3] }, new Word[] { });

            Assert.IsTrue(hrs.Count() == 3);
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[31]));
        }

        [TestMethod]
        public void WithoutAnyWords()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { }, new Word[] { }, new Word[] { w[5], w[22], w[1] });

            Assert.IsTrue(hrs.Contains(hr[31]));
            Assert.IsTrue(hrs.Contains(hr[32]));
            Assert.IsTrue(hrs.Contains(hr[71]));
        }

        [TestMethod]
        public void WithAllAnyNotWords()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { w[1] }, new Word[] { w[3], w[22] }, new Word[] { w[94] });

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[22]));
        }

        [TestMethod]
        public void WithNoWords()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { }, new Word[] { }, new Word[] { });

            Assert.AreEqual(hr.Count, hrs.Count());
        }

        #endregion WithAllAnyNotWords

        #region WithAllAnyNotWords_MinAny1 as WithAllAnyNotWords

        [TestMethod]
        public void WithAllAndAnyWords_MinAny()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWordsMinAny(session)(new Word[] { w[1] }, new Word[] { w[2], w[3] }, new Word[] { }, 1);

            Assert.IsTrue(hrs.Count() == 3);
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[20]));
        }

        [TestMethod]
        public void WithAllAndOneFromAnyWords_MinAny()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWordsMinAny(session)(new Word[] { w[1], w[2] }, new Word[] { w[3] }, new Word[] { }, 1);

            Assert.IsTrue(hrs.Count() == 0);
        }


        [TestMethod]
        public void WithAnyAndNotAnyWords_MinAny()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWordsMinAny(session)(new Word[] { }, new Word[] { w[4], w[31] }, new Word[] { w[5], w[3] }, 1);

            Assert.IsTrue(hrs.Count() == 2);
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[31]));
        }

        public void WithAnyWords_MinAny()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWordsMinAny(session)(new Word[] { }, new Word[] { w[94], w[3] }, new Word[] { }, 1);

            Assert.IsTrue(hrs.Count() == 3);
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[31]));
        }

        [TestMethod]
        public void WithAllAnyNotWords_MinAny()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWordsMinAny(session)(new Word[] { w[1] }, new Word[] { w[3], w[22] }, new Word[] { w[94] }, 1);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[22]));
        }

        #endregion WithAllAnyNotWordsMinAny
    }
}