using Diagnosis.Common;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public class SearchTest : InMemoryDatabaseTest
    {
        private SearchViewModel s;

        [TestInitialize]
        public void Init()
        {
            hrIds.ForAll((id) => hr[id] = session.Get<HealthRecord>(IntToGuid<HealthRecord>(id)));
            wIds.ForAll((id) => w[id] = session.Get<Word>(IntToGuid<Word>(id)));
            aIds.ForAll((id) => a[id] = session.Get<Appointment>(IntToGuid<Appointment>(id)));

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
        }

        [TestMethod]
        public void AllPatientWordsInStat()
        {
            s.Autocomplete.AddTag(w[5]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(10, s.Result.Statistic.Words.Count);
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