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

        #region WithAny

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

        #endregion WithAny

        [TestMethod]
        [ExpectedException(typeof(System.NotImplementedException))]
        public void WithoutAllWords() // TODO WithoutAllWords
        {
            var hrs = HealthRecordQuery.WithoutAllWords(session)(new Word[] { w[1], w[22] });

            Assert.AreEqual(12, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[21]));
            Assert.IsTrue(hrs.Contains(hr[70]));
            Assert.IsTrue(hrs.Contains(hr[30]));
            Assert.IsTrue(hrs.Contains(hr[31]));
            Assert.IsTrue(hrs.Contains(hr[32]));
            Assert.IsTrue(hrs.Contains(hr[40]));
            Assert.IsTrue(hrs.Contains(hr[71]));
            Assert.IsTrue(hrs.Contains(hr[73]));
            Assert.IsTrue(hrs.Contains(hr[74]));
        }

        [TestMethod]
        public void WithoutAnyWord()
        {
            var hrs = HealthRecordQuery.WithoutAnyWord(session)(new Word[] { w[1], w[3], w[22] });

            Assert.IsTrue(hrs.Contains(hr[71]));
            Assert.IsTrue(hrs.Contains(hr[31]));
            Assert.IsTrue(hrs.Contains(hr[30]));
        }

        #region WithAll

        [TestMethod]
        public void WithAllWords()
        {
            var hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[1], w[22] });

            Assert.IsTrue(hrs.Count() == 2);
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[72]));
        }

        [TestMethod]
        public void WithAllWordsEmpty()
        {
            var hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { });

            Assert.IsTrue(hrs.Count() == 0);
        }

        [TestMethod]
        public void WithAllWordsRepeat()
        {
            var hrs = HealthRecordQuery.WithAllWords(session)(new Word[] { w[22], w[22] });

            Assert.IsTrue(hrs.Count() == 1);
            Assert.IsTrue(hrs.Contains(hr[70]));
        }

        #endregion WithAll

        #region Confidence

        [TestMethod]
        public void WithAllChio()
        {
            var hrs = HealthRecordQuery.WithAllConfWords(session)(new Confindencable<Word>[] {
                w[4].AsConfidencable(),
            });

            Assert.IsTrue(hrs.Count() == 2);
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[32]));
        }

        [TestMethod]
        public void WithAllChio2()
        {
            var hrs = HealthRecordQuery.WithAllConfWords(session)(new Confindencable<Word>[] {
                w[1].AsConfidencable(Confidence.Absent),
                w[22].AsConfidencable()
            });

            Assert.IsTrue(hrs.Count() == 1);
            Assert.IsTrue(hrs.Contains(hr[22]));
        }

        [TestMethod]
        public void WithAllChio3()
        {
            var hrs = HealthRecordQuery.WithAllConfWords(session)(new Confindencable<Word>[] {
                w[22].AsConfidencable(Confidence.Absent),
                w[22].AsConfidencable()
            });

            Assert.IsTrue(hrs.Count() == 1);
            Assert.IsTrue(hrs.Contains(hr[70]));
        }

        [TestMethod]
        public void WithAnyChio()
        {
            var hrs = HealthRecordQuery.WithAnyConfWord(session)(new Confindencable<Word>[] {
                w[1].AsConfidencable(Confidence.Absent),
            });

            Assert.IsTrue(hrs.Count() == 1);
            Assert.IsTrue(hrs.Contains(hr[22]));
        }

        [TestMethod]
        public void WithAnyChios()
        {
            var hrs = HealthRecordQuery.WithAnyConfWords(session)(new Confindencable<Word>[] {
                w[1].AsConfidencable(Confidence.Absent),
                w[22].AsConfidencable(Confidence.Absent),
                w[22].AsConfidencable()
            }, 2);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[70]));
        }

        [TestMethod]
        public void WithAnyOnlyAbsent()
        {
            var hrs = HealthRecordQuery.WithAnyConfWords(session)(new Confindencable<Word>[] {
                w[1].AsConfidencable(Confidence.Absent),
                w[22].AsConfidencable(Confidence.Absent)
            }, 1);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[70]));
        }

        [TestMethod]
        public void WithoutAnyConfWord()
        {
            // есть 22, но нет 70
            var hrs = HealthRecordQuery.WithoutAnyConfWord(session)(new Confindencable<Word>[] {
                w[1].AsConfidencable(),
                w[22].AsConfidencable(Confidence.Absent) });

            Assert.AreEqual(8, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[30]));
            Assert.IsTrue(hrs.Contains(hr[31]));
            Assert.IsTrue(hrs.Contains(hr[32]));
            Assert.IsTrue(hrs.Contains(hr[40]));
            Assert.IsTrue(hrs.Contains(hr[71]));
            Assert.IsTrue(hrs.Contains(hr[73]));
            Assert.IsTrue(hrs.Contains(hr[74]));
        }

        #endregion Confidence
    }
}