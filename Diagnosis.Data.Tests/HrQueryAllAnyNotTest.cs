using Diagnosis.Common;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Diagnosis.Data.Tests
{
    [TestClass]
    public class HrQueryAllAnyNotTest : InMemoryDatabaseTest
    {
        [TestInitialize]
        public void Init()
        {
            Load<HealthRecord>();
            Load<Word>();
        }

        #region WithAllAnyNotWords

        [TestMethod]
        public void WithAllAndAnyWords()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { w[1] }, new Word[] { w[2], w[3] }, new Word[] { });

            Assert.AreEqual(3, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[20]));
        }

        [TestMethod]
        public void WithAllAndOneFromAnyWords()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { w[1], w[2] }, new Word[] { w[3] }, new Word[] { });

            Assert.AreEqual(0, hrs.Count());
        }

        [TestMethod]
        public void WithAllAndNotAnyWords()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { w[1] }, new Word[] { }, new Word[] { w[2], w[3] });

            Assert.AreEqual(3, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[21]));
            Assert.IsTrue(hrs.Contains(hr[72]));
        }

        [TestMethod]
        public void WithAnyAndNotAnyWords()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { }, new Word[] { w[4], w[31] }, new Word[] { w[5], w[3] });

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[31]));
        }

        public void WithAllOfWords()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { w[1], w[3] }, new Word[] { }, new Word[] { });

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[20]));
        }

        [TestMethod]
        public void WithAllOfWordsRepeat_Case1()
        {
            // a & a = a
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { w[22], w[22] }, new Word[] { }, new Word[] { });

            Assert.AreEqual(6, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[70]));
            Assert.IsTrue(hrs.Contains(hr[72]));
            Assert.IsTrue(hrs.Contains(hr[73]));
            Assert.IsTrue(hrs.Contains(hr[74]));
            Assert.IsTrue(hrs.Contains(hr[40]));
        }

        [TestMethod]
        [ExpectedException(typeof(AssertFailedException))]
        public void WithAllOfWordsRepeat_Case2()
        {
            // отдельные элементы
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { w[22], w[22] }, new Word[] { }, new Word[] { });

            Assert.AreEqual(1, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[70]));
        }

        public void WithAnyWords()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWords(session)(new Word[] { }, new Word[] { w[94], w[3] }, new Word[] { });

            Assert.AreEqual(3, hrs.Count());
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
            Assert.IsTrue(hrs.Contains(hr[72]));
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

            Assert.AreEqual(3, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[20]));
        }

        [TestMethod]
        public void WithAllAndOneFromAnyWords_MinAny()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWordsMinAny(session)(new Word[] { w[1], w[2] }, new Word[] { w[3] }, new Word[] { }, 1);

            Assert.AreEqual(0, hrs.Count());
        }

        [TestMethod]
        public void WithAnyAndNotAnyWords_MinAny()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWordsMinAny(session)(new Word[] { }, new Word[] { w[4], w[31] }, new Word[] { w[5], w[3] }, 1);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[31]));
        }

        public void WithAnyWords_MinAny()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWordsMinAny(session)(new Word[] { }, new Word[] { w[94], w[3] }, new Word[] { }, 1);

            Assert.AreEqual(3, hrs.Count());
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
            Assert.IsTrue(hrs.Contains(hr[72]));
        }

        #endregion WithAllAnyNotWords_MinAny1 as WithAllAnyNotWords

        #region WithAllAnyNotWords_MinAny2

        [TestMethod]
        public void WithAnyWords_MinAny2()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWordsMinAny(session)(new Word[] { }, new Word[] { w[1], w[31], w[94] }, new Word[] { }, 2);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[31]));
        }

        [TestMethod]
        public void WithAnyWords_MinAny2_AsWithAll()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWordsMinAny(session)(new Word[] { }, new Word[] { w[31], w[94] }, new Word[] { }, 2);

            Assert.AreEqual(1, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[31]));
        }

        [TestMethod]
        public void WithAnyWord_MinAny2_AsWithAll()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWordsMinAny(session)(new Word[] { }, new Word[] { w[31] }, new Word[] { }, 2);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[30]));
            Assert.IsTrue(hrs.Contains(hr[31]));
        }

        [TestMethod]
        public void WithAllAnyWords_MinAny2()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotWordsMinAny(session)(new Word[] { w[1] }, new Word[] { w[3], w[22] }, new Word[] { }, 2);

            Assert.AreEqual(1, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[22]));
        }

        #endregion WithAllAnyNotWords_MinAny2

        #region Confidence

        [TestMethod]
        public void WithAllChios()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotConfWordsMinAny(session)(new Confindencable<Word>[] {
               w[1].AsConfidencable(Confidence.Absent),
               w[22].AsConfidencable()
            },
            Enumerable.Empty<Confindencable<Word>>(),
            Enumerable.Empty<Confindencable<Word>>(),
            1);

            Assert.AreEqual(1, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[22]));
        }

        [TestMethod]
        public void WithAllOnlyAbsent()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotConfWordsMinAny(session)(new Confindencable<Word>[] {
               w[1].AsConfidencable(Confidence.Absent)
            },
            Enumerable.Empty<Confindencable<Word>>(),
            Enumerable.Empty<Confindencable<Word>>(),
            1);

            Assert.AreEqual(1, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[22]));
        }

        [TestMethod]
        public void WithAllNotChios()
        {
            // есть 3 и нет 1(absent)
            var hrs = HealthRecordQuery.WithAllAnyNotConfWordsMinAny(session)(new Confindencable<Word>[] {
               w[3].AsConfidencable()
            },
            Enumerable.Empty<Confindencable<Word>>(),
            new Confindencable<Word>[] {
               w[1].AsConfidencable(Confidence.Absent)
            }, 1);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[32]));
        }

        [TestMethod]
        public void WithAllNotChios2()
        {
            // есть 3 и нет 1 - вернет где 1(absent)
            var hrs = HealthRecordQuery.WithAllAnyNotConfWordsMinAny(session)(new Confindencable<Word>[] {
                w[3].AsConfidencable()
            },
            Enumerable.Empty<Confindencable<Word>>(),
            new Confindencable<Word>[] {
                w[1].AsConfidencable()
            }, 1);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[32]));
        }

        [TestMethod]
        public void WithAnyChio_MinAny2_AsWithAll()
        {
            var hrs = HealthRecordQuery.WithAllAnyNotConfWordsMinAny(session)(
            Enumerable.Empty<Confindencable<Word>>(),
            new Confindencable<Word>[] {
                w[31].AsConfidencable()
            },
            Enumerable.Empty<Confindencable<Word>>(),
            2);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[30]));
            Assert.IsTrue(hrs.Contains(hr[31]));
        }

        #endregion Confidence
    }
}