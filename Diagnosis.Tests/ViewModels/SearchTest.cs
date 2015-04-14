using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Diagnosis.Tests.ViewModels
{
    [TestClass]
    public class SearchTest : ViewModelTest
    {
        private SearchViewModel s;

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
        public void StateOnCreated()
        {
            Assert.AreEqual(1, s.QueryBlocks.Count);
            Assert.IsTrue(s.AllEmpty);

        }
        [TestMethod]
        public void CannotSearchWithoutOptions()
        {
            s.QueryBlocks.Clear();
            Assert.IsTrue(s.AllEmpty);
            Assert.IsFalse(s.SearchCommand.CanExecute(null));
        }
        [TestMethod]
        public void SearchNotUsedWords()
        {
            s.QueryBlocks[0].AutocompleteAll.AddTag(w[6]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(true, s.NothingFound);
            Assert.AreEqual(0, s.Result.Patients.Count);
        }

        [TestMethod]
        public void SearchTwoPatients()
        {
            s.QueryBlocks[0].AutocompleteAll.AddTag(w[22]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(2, s.Result.Patients.Count);
        }

        [TestMethod]
        public void SearchWordsInApp()
        {
            s.UseOldMode = true;
            s.QueryBlocks[0].AutocompleteAll.AddTag(w[1]);
            s.QueryBlocks[0].AutocompleteAll.AddTag(w[4]);
            s.QueryBlocks[0].QueryScope = HealthRecordQueryAndScope.Appointment;
            s.SearchCommand.Execute(null);

            Assert.AreEqual(1, s.Result.Statistic.Patients.Count);
            Assert.AreEqual(a[1], s.Result.Patients[0].Children[0].Children[0].Holder);

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[1]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[2]));
        }

        [TestMethod]
        public void WordsFromHrsInStat()
        {
            s.QueryBlocks[0].AutocompleteAll.AddTag(w[5]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(3, s.Result.Statistic.Words.Count); // 10 - все слова пациента
            Assert.AreEqual(0, s.Result.Statistic.WordsWithMeasure.Count);
        }

        [TestMethod]
        public void WordsWithMeasureInStat()
        {
            s.QueryBlocks[0].AutocompleteAll.AddTag(w[1]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(1, s.Result.Statistic.WordsWithMeasure.Count);
            Assert.AreEqual(w[3], s.Result.Statistic.WordsWithMeasure[0]);
        }

        [TestMethod]
        public void FoundHrs()
        {
            s.UseOldMode = true;
            s.QueryBlocks[0].AutocompleteAll.AddTag(w[4]);
            s.QueryBlocks[0].AutocompleteAll.AddTag(w[22]);
            s.QueryBlocks[0].QueryScope = HealthRecordQueryAndScope.Course;
            s.SearchCommand.Execute(null);

            Assert.AreEqual(0, s.Result.Patients[0].FoundHealthRecords.Count); // найденные — только слова в области
            Assert.AreEqual(1, s.Result.Patients[0].Children[0].FoundHealthRecords.Count); // 7-14
            Assert.AreEqual(1, s.Result.Patients[0].Children[0].Children[0].FoundHealthRecords.Count);
            Assert.AreEqual(1, s.Result.Patients[0].Children[0].Children[1].FoundHealthRecords.Count);
        }

        [TestMethod]
        public void AppOrder()
        {
            s.UseOldMode = true;
            s.QueryBlocks[0].AutocompleteAll.AddTag(w[4]);
            s.QueryBlocks[0].AutocompleteAll.AddTag(w[22]);
            s.QueryBlocks[0].QueryScope = HealthRecordQueryAndScope.Course;
            s.SearchCommand.Execute(null);

            Assert.AreEqual(0, s.Result.Patients[0].FoundHealthRecords.Count);
            Assert.AreEqual(1, s.Result.Patients[0].Children[0].FoundHealthRecords.Count); // 7-14
            Assert.AreEqual(2, s.Result.Patients[0].Children[0].Children[0].HealthRecords.Count); // 14
        }

        [TestMethod]
        public void SearchMeasureAndWord()
        {
            s.QueryBlocks[0].AutocompleteAll.AddTag(new MeasureOp(0.05, uom[1]) { Word = w[3], Operator = MeasureOperator.Equal });
            s.QueryBlocks[0].AutocompleteAll.AddTag(w[1]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(hr[22], s.Result.Statistic.HealthRecords.Single());
        }

        [TestMethod]
        public void SearchMeasureOrWord()
        {
            s.QueryBlocks[0].AutocompleteAny.AddTag(new MeasureOp(0.05, uom[1]) { Word = w[3], Operator = MeasureOperator.Equal });
            s.QueryBlocks[0].AutocompleteAny.AddTag(w[5]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[22]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[30]));

        }

        [TestMethod]
        public void SearchMeasureAndWordOrWords()
        {
            s.QueryBlocks[0].AutocompleteAll.AddTag(new MeasureOp(0.05, uom[1]) { Word = w[3], Operator = MeasureOperator.GreaterOrEqual });
            s.QueryBlocks[0].AutocompleteAll.AddTag(w[1]);
            s.QueryBlocks[0].AutocompleteAny.AddTag(w[22]);
            s.QueryBlocks[0].AutocompleteAny.AddTag(w[94]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[22]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[20]));

        }

        [TestMethod]
        public void SearchAnyMeasure()
        {
            s.QueryBlocks[0].AutocompleteAny.AddTag(new MeasureOp(0.05, uom[1]) { Word = w[3], Operator = MeasureOperator.Equal });
            s.SearchCommand.Execute(null);

            Assert.AreEqual(1, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[22]));

        }

        [TestMethod]
        public void SearchMeasureAndWordWhenSameWord()
        {
            s.QueryBlocks[0].AutocompleteAll.AddTag(new MeasureOp(0.05, uom[1]) { Word = w[3], Operator = MeasureOperator.Equal });
            s.QueryBlocks[0].AutocompleteAll.AddTag(w[3]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(1, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[22]));

        }

        [TestMethod]
        public void SearchInOneHolder()
        {
            s.QueryBlocks[0].AutocompleteAll.AddTag(w[1]);
            s.AddQbCommand.Execute(null);
            s.QueryBlocks[1].AutocompleteAll.AddTag(w[4]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[1]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[2]));

        }

        [TestMethod]
        public void SearchByCategoryFoundAllHrs()
        {
            Load<HrCategory>();
            s.QueryBlocks[0].Categories.Where(x => x.category == cat[2]).First().IsChecked = true;
            s.SearchCommand.Execute(null);

            Assert.AreEqual(3, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[31]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[22]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[40]));
        }
    }
}