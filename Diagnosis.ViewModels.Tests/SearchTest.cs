using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class SearchTest : ViewModelTest
    {
        private SearchViewModel s;
        int hrsTotal;

        [TestInitialize]
        public void Init()
        {
            Load<HealthRecord>();
            Load<Word>();
            Load<Appointment>();
            Load<Uom>();

            s = new SearchViewModel();
            hrsTotal = hr.Count;
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
            s.RootQueryBlock.AutocompleteAll.AddTag(w[6]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(true, s.NothingFound);
            Assert.AreEqual(0, s.Result.Patients.Count);
        }

        [TestMethod]
        public void SearchTwoPatients()
        {
            s.RootQueryBlock.AutocompleteAll.AddTag(w[22]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(2, s.Result.Patients.Count);
        }

        [TestMethod]
        public void SearchWordsInApp()
        {
            s.UseOldMode = true;
            s.RootQueryBlock.AutocompleteAll.AddTag(w[1]);
            s.RootQueryBlock.AutocompleteAll.AddTag(w[4]);
            s.RootQueryBlock.QueryScope = HealthRecordQueryAndScope.Appointment;
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
            s.RootQueryBlock.AutocompleteAll.AddTag(w[5]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(3, s.Result.Statistic.Words.Count); // 10 - все слова пациента
            Assert.AreEqual(0, s.Result.Statistic.WordsWithMeasure.Count);
        }

        [TestMethod]
        public void WordsWithMeasureInStat()
        {
            s.RootQueryBlock.AutocompleteAll.AddTag(w[1]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(1, s.Result.Statistic.WordsWithMeasure.Count);
            Assert.AreEqual(w[3], s.Result.Statistic.WordsWithMeasure[0]);
        }

        [TestMethod]
        public void FoundHrs()
        {
            s.UseOldMode = true;
            s.RootQueryBlock.AutocompleteAll.AddTag(w[4]);
            s.RootQueryBlock.AutocompleteAll.AddTag(w[22]);
            s.RootQueryBlock.QueryScope = HealthRecordQueryAndScope.Course;
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
            s.RootQueryBlock.AutocompleteAll.AddTag(w[4]);
            s.RootQueryBlock.AutocompleteAll.AddTag(w[22]);
            s.RootQueryBlock.QueryScope = HealthRecordQueryAndScope.Course;
            s.SearchCommand.Execute(null);

            Assert.AreEqual(0, s.Result.Patients[0].FoundHealthRecords.Count);
            Assert.AreEqual(1, s.Result.Patients[0].Children[0].FoundHealthRecords.Count); // 7-14
            Assert.AreEqual(2, s.Result.Patients[0].Children[0].Children[0].HealthRecords.Count); // 14
        }

        [TestMethod]
        public void SearchMeasureAndWord()
        {
            s.RootQueryBlock.AutocompleteAll.AddTag(new MeasureOp(0.05, uom[1]) { Word = w[3], Operator = MeasureOperator.Equal });
            s.RootQueryBlock.AutocompleteAll.AddTag(w[1]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(hr[22], s.Result.Statistic.HealthRecords.Single());
        }

        [TestMethod]
        public void SearchMeasureOrWord()
        {
            s.RootQueryBlock.AutocompleteAny.AddTag(new MeasureOp(0.05, uom[1]) { Word = w[3], Operator = MeasureOperator.Equal });
            s.RootQueryBlock.AutocompleteAny.AddTag(w[5]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[22]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[30]));
        }

        [TestMethod]
        public void SearchMeasureAndWordOrWords()
        {
            s.RootQueryBlock.AutocompleteAll.AddTag(new MeasureOp(0.05, uom[1]) { Word = w[3], Operator = MeasureOperator.GreaterOrEqual });
            s.RootQueryBlock.AutocompleteAll.AddTag(w[1]);
            s.RootQueryBlock.AutocompleteAny.AddTag(w[22]);
            s.RootQueryBlock.AutocompleteAny.AddTag(w[94]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[22]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[20]));
        }

        [TestMethod]
        public void SearchAnyMeasure()
        {
            s.RootQueryBlock.AutocompleteAny.AddTag(new MeasureOp(0.05, uom[1]) { Word = w[3], Operator = MeasureOperator.Equal });
            s.SearchCommand.Execute(null);

            Assert.AreEqual(1, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[22]));
        }

        [TestMethod]
        public void SearchMeasureAndWordWhenSameWord()
        {
            s.RootQueryBlock.AutocompleteAll.AddTag(new MeasureOp(0.05, uom[1]) { Word = w[3], Operator = MeasureOperator.Equal });
            s.RootQueryBlock.AutocompleteAll.AddTag(w[3]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(1, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[22]));
        }

        [TestMethod]
        public void SearchAllInOneHr()
        {
            // в записи 1 и (3 или 4 или 22)  и (94 или 5)
            s.RootQueryBlock.SearchScope = SearchScope.HealthRecord;
            s.RootQueryBlock.All = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].AutocompleteAll.AddTag(w[1]);
            s.RootQueryBlock.Children[0].AutocompleteAny.AddTag(w[22]);
            s.RootQueryBlock.Children[0].AutocompleteAny.AddTag(w[3]);
            s.RootQueryBlock.Children[0].AutocompleteAny.AddTag(w[4]);
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].AutocompleteAny.AddTag(w[94]);
            s.RootQueryBlock.Children[1].AutocompleteAny.AddTag(w[5]);

            s.SearchCommand.Execute(null);

            Assert.AreEqual(1, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[20]));
        }
        [TestMethod]
        public void SearchAnyInOneHrDistinct()
        {
            // в записи 1 и 22   или (1 и 3)
            s.RootQueryBlock.SearchScope = SearchScope.HealthRecord;
            s.RootQueryBlock.All = false;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].AutocompleteAll.AddTag(w[1]);
            s.RootQueryBlock.Children[0].AutocompleteAll.AddTag(w[22]);
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].AutocompleteAll.AddTag(w[1]);
            s.RootQueryBlock.Children[1].AutocompleteAll.AddTag(w[3]);

            s.SearchCommand.Execute(null);

            Assert.AreEqual(3, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[20]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[22]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[72]));
        }
        [TestMethod]
        public void SearchAnyInOneHr()
        {
            // в записи 1 и (4 или 22)  или (94 или 5) и нет 3
            s.RootQueryBlock.SearchScope = SearchScope.HealthRecord;
            s.RootQueryBlock.All = false;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].AutocompleteAll.AddTag(w[1]);
            s.RootQueryBlock.Children[0].AutocompleteAny.AddTag(w[22]);
            s.RootQueryBlock.Children[0].AutocompleteAny.AddTag(w[4]);
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].AutocompleteAny.AddTag(w[94]);
            s.RootQueryBlock.Children[1].AutocompleteAny.AddTag(w[5]);
            s.RootQueryBlock.Children[1].AutocompleteNot.AddTag(w[3]);

            s.SearchCommand.Execute(null);

            Assert.AreEqual(5, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[2]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[22]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[30]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[31]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[72]));
        }

        [TestMethod]
        public void SearchAllInOneHolder()
        {
            s.RootQueryBlock.SearchScope = SearchScope.Holder;
            s.RootQueryBlock.All = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].AutocompleteAll.AddTag(w[1]);
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].AutocompleteAll.AddTag(w[4]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[1]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[2]));
        }

        [TestMethod]
        public void SearchAnyInOneHolder()
        {
            // в списке записи со словами 1 и 3 или записи со словом 22 без 1
            s.RootQueryBlock.SearchScope = SearchScope.Holder;
            s.RootQueryBlock.All = false;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].AutocompleteAll.AddTag(w[1]);
            s.RootQueryBlock.Children[0].AutocompleteAll.AddTag(w[3]);
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].AutocompleteAll.AddTag(w[22]);
            s.RootQueryBlock.Children[1].AutocompleteNot.AddTag(w[1]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(6, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[20]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[22]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[70]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[73]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[74]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[40]));
        }

        [TestMethod]
        public void SearchAllInOneHolder2()
        {
            // в спсике записи с 31 или 3 и записи с 1 или 5
            s.RootQueryBlock.SearchScope = SearchScope.Holder;
            s.RootQueryBlock.All = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].AutocompleteAny.AddTag(w[31]);
            s.RootQueryBlock.Children[0].AutocompleteAny.AddTag(w[3]);
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].AutocompleteAny.AddTag(w[1]);
            s.RootQueryBlock.Children[1].AutocompleteAny.AddTag(w[5]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(6, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[20]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[21]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[22]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[30]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[31]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[32]));
        }

        [TestMethod]
        public void SearchAllInOnePatient()
        {
            s.RootQueryBlock.SearchScope = SearchScope.Patient;
            s.RootQueryBlock.All = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].AutocompleteAll.AddTag(w[4]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[2]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[32]));
        }

        [TestMethod]
        public void SearchInOnePatient2()
        {
            // у пациента записи со словами 4 и в одном списке записи со словами 1 и записи со словами 3

            s.RootQueryBlock.SearchScope = SearchScope.Patient;
            s.RootQueryBlock.All = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].AutocompleteAll.AddTag(w[4]);
            s.RootQueryBlock.AddChildGroupQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].SearchScope = SearchScope.Holder;
            s.RootQueryBlock.Children[1].All = true;
            s.RootQueryBlock.Children[1].AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].Children[0].AutocompleteAll.AddTag(w[3]);
            s.RootQueryBlock.Children[1].AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].Children[1].AutocompleteAll.AddTag(w[1]);

            s.SearchCommand.Execute(null);

            Assert.AreEqual(5, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[2]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[32]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[20]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[21]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[22]));
        }

        [TestMethod]
        public void SearchAllInOneHr_WithExcludingOnly()
        {
            // в записи 1 и (3 или 4) и нет 94
            // в записи не должно быть исключаемых слов
            s.RootQueryBlock.SearchScope = SearchScope.HealthRecord;
            s.RootQueryBlock.All = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].AutocompleteAll.AddTag(w[1]);
            s.RootQueryBlock.Children[0].AutocompleteAny.AddTag(w[3]);
            s.RootQueryBlock.Children[0].AutocompleteAny.AddTag(w[4]);
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].AutocompleteNot.AddTag(w[94]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[2]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[22]));
        }
        [TestMethod]
        public void SearchAllInOneHr_WithExcludingOnly2()
        {
            // в записи нет 1,31 и нет 22
            s.RootQueryBlock.SearchScope = SearchScope.HealthRecord;
            s.RootQueryBlock.All = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].AutocompleteNot.AddTag(w[1]);
            s.RootQueryBlock.Children[0].AutocompleteNot.AddTag(w[31]);
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].AutocompleteNot.AddTag(w[22]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[32]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[71]));
        }
        [TestMethod]
        public void SearchAnyInOneHr_WithExcludingOnly()
        {
            // в записи 1 и (3 или 4) или нет 94, 22, 31
            // 
            s.RootQueryBlock.SearchScope = SearchScope.HealthRecord;
            s.RootQueryBlock.All = false;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].AutocompleteAll.AddTag(w[1]);
            s.RootQueryBlock.Children[0].AutocompleteAny.AddTag(w[3]);
            s.RootQueryBlock.Children[0].AutocompleteAny.AddTag(w[4]);
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].AutocompleteNot.AddTag(w[94]);
            s.RootQueryBlock.Children[1].AutocompleteNot.AddTag(w[22]);
            s.RootQueryBlock.Children[1].AutocompleteNot.AddTag(w[31]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(7, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[1]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[2]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[20]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[21]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[22]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[32]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[71])); // и пустые
        }

        [TestMethod]
        public void SearchAllInOneHolder_WithExcludingOnly()
        {
            // в списке записи со словами 3 или 31 и нет записи со словом 5
            s.RootQueryBlock.SearchScope = SearchScope.Holder;
            s.RootQueryBlock.All = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].AutocompleteAny.AddTag(w[31]);
            s.RootQueryBlock.Children[0].AutocompleteAny.AddTag(w[3]);
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].AutocompleteNot.AddTag(w[5]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[20]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[22]));
        }
        [TestMethod]
        public void SearchAllInOneHolder_WithExcludingOnly2()
        {
            // как SearchAnyInOneHolder_WithExcludingOnly2

            s.RootQueryBlock.SearchScope = SearchScope.Holder;
            s.RootQueryBlock.All = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].AutocompleteNot.AddTag(w[5]);
            s.RootQueryBlock.Children[0].AutocompleteNot.AddTag(w[22]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(7, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[1]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[2]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[20]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[21]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[31]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[32]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[71]));
        }
        [TestMethod]
        public void SearchAnyInOneHolder_WithExcludingOnly()
        {
            // в списке записи со словами 3 или 31 или (нет записей со словами 5, 22)/_(есть запись без слов 5, 22)_
            // _по первому + все записи списка без тех, где 5/22_ или только те, где есть слова 3/31?
            s.RootQueryBlock.SearchScope = SearchScope.Holder;
            s.RootQueryBlock.All = false;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].AutocompleteAny.AddTag(w[31]);
            s.RootQueryBlock.Children[0].AutocompleteAny.AddTag(w[3]);
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].AutocompleteNot.AddTag(w[5]);
            s.RootQueryBlock.Children[1].AutocompleteNot.AddTag(w[22]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(9, s.Result.Statistic.HealthRecords.Count);

            // если все записи списка
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[1]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[2]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[71]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[21])); // потому что есть запись без 22


            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[20]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[22]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[30]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[31]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[32]));
        }

        [TestMethod]
        public void SearchAnyInOneHolder_WithExcludingOnly2()
        {
            // в списке нашлась запись без слов 5, 22
            // все записи списка или _только те, где нет этих слов_?
            s.RootQueryBlock.SearchScope = SearchScope.Holder;
            s.RootQueryBlock.All = false;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].AutocompleteNot.AddTag(w[5]);
            s.RootQueryBlock.Children[0].AutocompleteNot.AddTag(w[22]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(7, s.Result.Statistic.HealthRecords.Count);
            // если все записи списка
            //Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[22]));
            //Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[30]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[1]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[2]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[20]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[21]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[31]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[32]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[71]));
        }
        [TestMethod]
        public void SearchAllInOneHolder_TwoExcludingOnly()
        {
            // в спсике нет записей с 1 и записей с 22
            s.RootQueryBlock.SearchScope = SearchScope.Holder;
            s.RootQueryBlock.All = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].AutocompleteNot.AddTag(w[1]);
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].AutocompleteNot.AddTag(w[22]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(4, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[30]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[31]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[32]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[71]));
        }
        [TestMethod]
        public void SearchAllInOneHolder_TwoExcludingOnly_WithCats()
        {
            // в спсике нет запись с 22 категории 2 или 1 и нет запись с 4 категории 1
            Load<HrCategory>();

            s.RootQueryBlock.SearchScope = SearchScope.Holder;
            s.RootQueryBlock.All = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].AutocompleteNot.AddTag(w[22]);
            s.RootQueryBlock.Children[0].Categories.Where(x => x.category == cat[1]).First().IsChecked = true;
            s.RootQueryBlock.Children[0].Categories.Where(x => x.category == cat[2]).First().IsChecked = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].AutocompleteNot.AddTag(w[4]);
            s.RootQueryBlock.Children[1].Categories.Where(x => x.category == cat[1]).First().IsChecked = true;
            s.SearchCommand.Execute(null);

            Assert.AreEqual(6, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[1]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[2]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[71]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[72]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[73]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[74]));
        }
        [TestMethod]
        public void SearchAnyInOneHolder_TwoExcludingOnly_WithCats()
        {
            // в спсике нашлась запись без 22 категории 2 или 1 или без 4 категории 1
            // то есть все записи
            Load<HrCategory>();

            s.RootQueryBlock.SearchScope = SearchScope.Holder;
            s.RootQueryBlock.All = false;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].AutocompleteNot.AddTag(w[22]);
            s.RootQueryBlock.Children[0].Categories.Where(x => x.category == cat[1]).First().IsChecked = true;
            s.RootQueryBlock.Children[0].Categories.Where(x => x.category == cat[2]).First().IsChecked = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].AutocompleteNot.AddTag(w[4]);
            s.RootQueryBlock.Children[1].Categories.Where(x => x.category == cat[1]).First().IsChecked = true;
            s.SearchCommand.Execute(null);

            Assert.AreEqual(hrsTotal, s.Result.Statistic.HealthRecords.Count);
        }
        [TestMethod]
        public void SearchAllInOneHolder_ExcludingOnly_AndCat()
        {
            // записи 2 категории списка, где нет слов 22
            Load<HrCategory>();

            s.RootQueryBlock.SearchScope = SearchScope.Holder;
            s.RootQueryBlock.All = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].Categories.Where(x => x.category == cat[2]).First().IsChecked = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].AutocompleteNot.AddTag(w[22]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(1, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[31]));
        }
        [TestMethod]
        public void SearchAnyInOneHolder_TwoExcludingOnly_As()
        {
            // в спсике нашлась запись без 1 или запись без 22
            // с каждого блока только те, где нет любого из этих слов
            s.RootQueryBlock.SearchScope = SearchScope.Holder;
            s.RootQueryBlock.All = false;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].AutocompleteNot.AddTag(w[1]);
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].AutocompleteNot.AddTag(w[22]);
            s.SearchCommand.Execute(null);

            // все записи, кроме
            Assert.AreEqual(hrsTotal - 2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(!s.Result.Statistic.HealthRecords.Contains(hr[22]));
            Assert.IsTrue(!s.Result.Statistic.HealthRecords.Contains(hr[72]));
        }
        [TestMethod]
        public void AllAnyWIthSingleBlockSameResults()
        {
        }

        [TestMethod]
        public void SearchAllInOneHolder_WithExcludingOnly_OrderNoMatter()
        {
            // в списке нет записи со словом 5 и записи со словами 3 или 31
            s.RootQueryBlock.SearchScope = SearchScope.Holder;
            s.RootQueryBlock.All = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].AutocompleteNot.AddTag(w[5]);
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].AutocompleteAny.AddTag(w[3]);
            s.RootQueryBlock.Children[1].AutocompleteAny.AddTag(w[31]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[20]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[22]));
        }

        [TestMethod]
        public void SearchByCategoryFoundAllHrs()
        {
            Load<HrCategory>();
            s.RootQueryBlock.Categories.Where(x => x.category == cat[2]).First().IsChecked = true;
            s.SearchCommand.Execute(null);

            Assert.AreEqual(3, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[31]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[22]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[40]));
        }

        [TestMethod]
        public void SearchByCategoryFoundAllHrs2()
        {
            // записи 3 или 5 кат
            Load<HrCategory>();
            s.RootQueryBlock.Categories.Where(x => x.category == cat[3]).First().IsChecked = true;
            s.RootQueryBlock.Categories.Where(x => x.category == cat[5]).First().IsChecked = true;
            s.SearchCommand.Execute(null);

            Assert.AreEqual(3, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[1]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[2]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[21]));
        }

        [TestMethod]
        public void SearchAllInOneHolder_ByCategory()
        {
            // в списке есть записи 1 и 5 кат 
            Load<HrCategory>();
            s.RootQueryBlock.SearchScope = SearchScope.Holder;
            s.RootQueryBlock.All = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].Categories.Where(x => x.category == cat[1]).First().IsChecked = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].Categories.Where(x => x.category == cat[5]).First().IsChecked = true;
            s.SearchCommand.Execute(null);

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[20]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[21]));
        }

        [TestMethod]
        public void SearchAnyInOneHolder_ByCategory()
        {
            // в списке записи 3 или 5 кат
            Load<HrCategory>();
            s.RootQueryBlock.SearchScope = SearchScope.Holder;
            s.RootQueryBlock.All = false;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].Categories.Where(x => x.category == cat[3]).First().IsChecked = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].Categories.Where(x => x.category == cat[5]).First().IsChecked = true;
            s.SearchCommand.Execute(null);

            Assert.AreEqual(3, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[1]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[2]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[21]));
        }

        [TestMethod]
        public void SearchAllInOneHr_ByCategory()
        {
            // у записи вообще не бывает 2 категории
            Load<HrCategory>();
            s.RootQueryBlock.SearchScope = SearchScope.HealthRecord;
            s.RootQueryBlock.All = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].Categories.Where(x => x.category == cat[3]).First().IsChecked = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].Categories.Where(x => x.category == cat[5]).First().IsChecked = true;
            s.SearchCommand.Execute(null);

            Assert.AreEqual(0, s.Result.Statistic.HealthRecords.Count);
        }
        [TestMethod]
        public void SearchAnyInOneHr_ByCategory()
        {
            // записи 3 или 5 кат
            Load<HrCategory>();
            s.RootQueryBlock.SearchScope = SearchScope.HealthRecord;
            s.RootQueryBlock.All = false;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[0].Categories.Where(x => x.category == cat[3]).First().IsChecked = true;
            s.RootQueryBlock.AddChildQbCommand.Execute(null);
            s.RootQueryBlock.Children[1].Categories.Where(x => x.category == cat[5]).First().IsChecked = true;
            s.SearchCommand.Execute(null);

            Assert.AreEqual(3, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[1]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[2]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[21]));
        }

        [TestMethod]
        public void Search_RootExcludingOnly_WithCat()
        {
            // записи 1 категории, где нет слов 22
            Load<HrCategory>();

            s.RootQueryBlock.Categories.Where(x => x.category == cat[1]).First().IsChecked = true;
            s.RootQueryBlock.AutocompleteNot.AddTag(w[22]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(4, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[20]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[30]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[32]));
            Assert.IsTrue(s.Result.Statistic.HealthRecords.Contains(hr[71]));
        }
    }
}