using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class StatisticTest : ViewModelTest
    {
        private SearchViewModel s;

        public HrsStatistic Stats { get { return s.Result.Statistic as HrsStatistic; } }

        [TestInitialize]
        public void Init()
        {
            Load<HealthRecord>();
            Load<Word>();
            Load<Appointment>();
            Load<Uom>();
            

            s = new SearchViewModel();
        }
        [TestCleanup]
        public void Clean()
        {
            if (s != null)
                s.Dispose();
        }
        [TestMethod]
        public void AllWordsFromFoundHrs()
        {
            (s.RootQueryBlock.AutocompleteAll as AutocompleteViewModel).AddTag(w[5]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(hr[30], Stats.HealthRecords.Single());

            Assert.IsTrue(hr[30].Words.ScrambledEquals(Stats.Words));
            Assert.AreEqual(0, Stats.WordsWithMeasure.Count);
        }

        [TestMethod]
        public void WordsWithMeasureInStat()
        {
            (s.RootQueryBlock.AutocompleteAll as AutocompleteViewModel).AddTag(w[1]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(1, Stats.WordsWithMeasure.Count);
            Assert.AreEqual(w[3], Stats.WordsWithMeasure[0]);
        }

        [TestMethod]
        public void MeasuresInStat()
        {
            var wWithM = w[3];
            var wWithoutM = w[1];
            (s.RootQueryBlock.AutocompleteAll as AutocompleteViewModel).AddTag(wWithM);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(1, Stats.GridValues[hr[20]][wWithM].Measures.Count());
            Assert.AreEqual(1, Stats.GridValues[hr[22]][wWithM].Measures.Count());

            Assert.AreEqual(null, Stats.GridValues[hr[32]][wWithM].Measures);
            Assert.AreEqual(null, Stats.GridValues[hr[20]][wWithoutM].Measures);
            Assert.AreEqual(null, Stats.GridValues[hr[22]][wWithoutM].Measures);
        }

        [TestMethod]
        public void IcdInStats()
        {
            var icd1 = session.Get<IcdDisease>(1);
            (s.RootQueryBlock.AutocompleteAll as AutocompleteViewModel).AddTag(icd1);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(1, Stats.Icds.Count);
        }
    }
}