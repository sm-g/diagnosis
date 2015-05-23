using Diagnosis.Common;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Diagnosis.Tests;

namespace Diagnosis.Models.Tests
{
    [TestClass]
    public class ExtenstionsTest : InMemoryDatabaseTest
    {
        [TestInitialize]
        public void Init()
        {
            Load<HealthRecord>();
        }
        [TestMethod]
        public void GetAllWords_WithRepeats()
        {
            Load<Patient>();
            Load<Word>();

            var allW = p[2].GetAllWords();
            var expected = new[] { w[22], w[22] };
            Assert.IsTrue(allW.ScrambledEquals(expected));
        }
        [TestMethod]
        public void GetAllWords_FromMeasures()
        {
            Load<Appointment>();
            Load<Word>();

            var allW = a[2].GetAllWords();
            var expected = new[] { w[1], w[1], w[1], w[3], w[3], w[94], w[22] };
            Assert.IsTrue(allW.ScrambledEquals(expected));
        }
        [TestMethod]
        public void GetCWords()
        {
            Load<Word>();

            var allW = hr[20].GetCWords().Select(x => x.HIO);
            var expected = new[] { w[1], w[3], w[94] };
            Assert.IsTrue(allW.ScrambledEquals(expected));
        }
        [TestMethod]
        public void GetCWordsNotFromMeasure()
        {
            Load<Word>();

            var cwordsNotInMeasures = hr[22].GetCWordsNotFromMeasure();
            var expected = new[] { w[1].AsConfidencable(Confidence.Absent), w[22].AsConfidencable() };
            Assert.IsTrue(cwordsNotInMeasures.ScrambledEquals(expected));
        }
        [TestMethod]
        public void GetAllHrs()
        {
            Load<Patient>();
            var hrs = p[1].GetAllHrs();
            var expected = new[] { hr[1], hr[2], hr[20], hr[21], hr[22], hr[30], hr[31], hr[32], hr[40], hr[70], hr[71], hr[72] };
            Assert.IsTrue(hrs.ScrambledEquals(expected));
        }
    }
}