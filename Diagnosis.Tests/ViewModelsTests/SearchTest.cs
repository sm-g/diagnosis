﻿using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class SearchTest : InMemoryDatabaseTest
    {
        private SearchViewModel s;

        [TestInitialize]
        public void Init()
        {
            Load<HealthRecord>();
            Load<Word>();
            Load<Appointment>();

            s = new SearchViewModel();
        }

        [TestMethod]
        public void SearchNoWords()
        {
            s.Autocomplete.AddTag(w[6]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(true, s.NothingFound);
            Assert.AreEqual(0, s.Result.Patients.Count);
        }

        [TestMethod]
        public void SearchTwoPatients()
        {
            s.Autocomplete.AddTag(w[22]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(2, s.Result.Patients.Count);
        }

        [TestMethod]
        public void SearchWordsInApp()
        {
            s.Autocomplete.AddTag(w[1]);
            s.Autocomplete.AddTag(w[4]);
            s.QueryScope = HealthRecordQueryAndScope.Appointment;
            s.SearchCommand.Execute(null);

            Assert.AreEqual(1, s.Result.Statistic.Patients.Count);
            Assert.AreEqual(a[1], s.Result.Patients[0].Children[0].Children[0].Holder);
            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
        }

        [TestMethod]
        public void WordsFromHrsInStat()
        {
            s.Autocomplete.AddTag(w[5]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(3, s.Result.Statistic.Words.Count); // 10 - все слова пациента
            Assert.AreEqual(0, s.Result.Statistic.WordsWithMeasure.Count);
        }

        [TestMethod]
        public void WordsWithMeasureInStat()
        {
            s.Autocomplete.AddTag(w[1]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(1, s.Result.Statistic.WordsWithMeasure.Count);
            Assert.AreEqual(w[3], s.Result.Statistic.WordsWithMeasure[0]);
        }

        [TestMethod]
        public void FoundHrs()
        {
            s.Autocomplete.AddTag(w[4]);
            s.Autocomplete.AddTag(w[22]);
            s.QueryScope = HealthRecordQueryAndScope.Course;
            s.SearchCommand.Execute(null);

            Assert.AreEqual(0, s.Result.Patients[0].FoundHealthRecords.Count); // найденные — только слова в области
            Assert.AreEqual(1, s.Result.Patients[0].Children[0].FoundHealthRecords.Count); // 7-14
            Assert.AreEqual(1, s.Result.Patients[0].Children[0].Children[0].FoundHealthRecords.Count);
            Assert.AreEqual(1, s.Result.Patients[0].Children[0].Children[1].FoundHealthRecords.Count);
        }

        [TestMethod]
        public void AppOrder()
        {
            s.Autocomplete.AddTag(w[4]);
            s.Autocomplete.AddTag(w[22]);
            s.QueryScope = HealthRecordQueryAndScope.Course;
            s.SearchCommand.Execute(null);

            Assert.AreEqual(0, s.Result.Patients[0].FoundHealthRecords.Count);
            Assert.AreEqual(1, s.Result.Patients[0].Children[0].FoundHealthRecords.Count); // 7-14
            Assert.AreEqual(2, s.Result.Patients[0].Children[0].Children[0].HealthRecords.Count); // 14
        }
    }
}