using Diagnosis.Common;
using Diagnosis.Data.Search;
using Diagnosis.Models;
using Diagnosis.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.Data.Tests
{
    [TestClass]
    public class SearcherConfTest : InMemoryDatabaseTest
    {
        private SearchOptions o;
        private int hrsTotal;

        [TestInitialize]
        public void Init()
        {
            Load<HealthRecord>();
            Load<Word>();
            Load<Appointment>();
            Load<Uom>();

            o = new SearchOptions(true);
            Searcher.logOn = true;
            hrsTotal = hr.Count;
        }

        [TestCleanup]
        public void Clean()
        {
            Searcher.logOn = false;
        }

        [TestMethod]
        public void AllInHr_Conf()
        {
            var hrs = o
           .Scope(SearchScope.HealthRecord)
           .All()
           .AddChild(x => x
               .WithConf()
               .SetAll(w[22])
               .SetNot(w[1].AsConfidencable(Confidence.Absent)))
           .Search(session);

            // нет 22
            Assert.AreEqual(5, hrs.Count());
            Assert.IsTrue(hrs.IsSuperSetOf(
                hr[40],
                hr[70],
                hr[72],
                hr[73],
                hr[74]));
        }

        [TestMethod]
        public void AnyInHr_Conf()
        {
            var hrs = o
           .Scope(SearchScope.HealthRecord)
           .Any()
           .AddChild(x => x
               .WithConf()
               .SetAll(w[22])
               .SetNot(w[1].AsConfidencable(Confidence.Absent)))
           .Search(session);

            // нет 22
            Assert.AreEqual(5, hrs.Count());
            Assert.IsTrue(hrs.IsSuperSetOf(
                hr[40],
                hr[70],
                hr[72],
                hr[73],
                hr[74]));
        }

        [TestMethod]
        public void AllInHr_WithExcluding_Conf()
        {
            var hrs = o
           .Scope(SearchScope.HealthRecord)
           .All()
           .AddChild(x => x
               .SetAll(w[22]))
           .AddChild(x => x
               .WithConf()
               .SetNot(w[1].AsConfidencable(Confidence.Absent)))
           .Search(session);

            // нет 22
            Assert.AreEqual(5, hrs.Count());
            Assert.IsTrue(hrs.IsSuperSetOf(
                hr[40],
                hr[70],
                hr[72],
                hr[73],
                hr[74]));
        }

        [TestMethod]
        public void AllInHolder_WithExcluding_Conf()
        {
            var hrs = o
           .Scope(SearchScope.Holder)
           .All()
           .AddChild(x => x
               .SetAll(w[22]))
           .AddChild(x => x
               .WithConf()
               .SetNot(w[1].AsConfidencable(Confidence.Absent)))
           .Search(session);

            // нет 22
            Assert.AreEqual(5, hrs.Count());
            Assert.IsTrue(hrs.IsSuperSetOf(
                hr[40],
                hr[70],
                hr[72],
                hr[73],
                hr[74]));
        }

        [TestMethod]
        public void AllInPatient_WithExcluding_Conf()
        {
            var hrs = o
           .Scope(SearchScope.Patient)
           .All()
           .AddChild(x => x
               .SetAll(w[22]))
           .AddChild(x => x
               .WithConf()
               .SetNot(w[1].AsConfidencable(Confidence.Absent)))
           .Search(session);

            // из-за 22
            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.IsSuperSetOf(
                hr[73],
                hr[74]));
        }
    }
}