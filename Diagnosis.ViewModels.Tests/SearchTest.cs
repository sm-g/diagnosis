using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class SearchTest : ViewModelTest
    {
        private SearchViewModel s;
        private int hrsTotal;

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
        public void NotUsedWords()
        {
            s.RootQueryBlock.AutocompleteAll.AddTag(w[6]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(true, s.NothingFound);
            Assert.AreEqual(0, s.Result.Patients.Count);
        }

        [TestMethod]
        public void TwoPatients()
        {
            s.RootQueryBlock.AutocompleteAll.AddTag(w[22]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(2, s.Result.Patients.Count);
        }

        [TestMethod]
        public void WordsInApp()
        {
            s.UseOldMode = true;
            s.RootQueryBlock.AutocompleteAll.AddTag(w[1]);
            s.RootQueryBlock.AutocompleteAll.AddTag(w[4]);
            s.RootQueryBlock.QueryScope = HealthRecordQueryAndScope.Appointment;
            s.SearchCommand.Execute(null);

            Assert.AreEqual(1, s.Result.Statistic.Patients.Count);
            Assert.AreEqual(a[1], s.Result.Patients[0].Children[0].Children[0].Holder);

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[1]));
            Assert.IsTrue(s.Contains(hr[2]));
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

        #region Measure

        [TestMethod]
        public void MeasureAndWord()
        {
            s.RootQueryBlock.AutocompleteAll.AddTag(new MeasureOp(MeasureOperator.Equal, 0.05, uom[1]) { Word = w[3]});
            s.RootQueryBlock.AutocompleteAll.AddTag(w[1]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(hr[22], s.Result.Statistic.HealthRecords.Single());
        }

        [TestMethod]
        public void MeasureOrWord()
        {
            s.RootQueryBlock.AutocompleteAny.AddTag(new MeasureOp(MeasureOperator.Equal, 0.05, uom[1]) { Word = w[3]});
            s.RootQueryBlock.AutocompleteAny.AddTag(w[5]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[22]));
            Assert.IsTrue(s.Contains(hr[30]));
        }

        [TestMethod]
        public void MeasureAndWordOrWords()
        {
            s.RootQueryBlock.AutocompleteAll.AddTag(new MeasureOp(MeasureOperator.GreaterOrEqual, 0.05, uom[1]) { Word = w[3] });
            s.RootQueryBlock.AutocompleteAll.AddTag(w[1]);
            s.RootQueryBlock.AutocompleteAny.AddTag(w[22]);
            s.RootQueryBlock.AutocompleteAny.AddTag(w[94]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[22]));
            Assert.IsTrue(s.Contains(hr[20]));
        }

        [TestMethod]
        public void AnyMeasure()
        {
            s.RootQueryBlock.AutocompleteAny.AddTag(new MeasureOp(MeasureOperator.Equal, 0.05, uom[1]) { Word = w[3]});
            s.SearchCommand.Execute(null);

            Assert.AreEqual(1, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[22]));
        }

        [TestMethod]
        public void MeasureAndWordWhenSameWord()
        {
            s.RootQueryBlock
               .SetAll(w[3], new MeasureOp(MeasureOperator.Equal, 0.05, uom[1]) { Word = w[3] })
               .Search();

            Assert.AreEqual(1, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[22]));
        }

        [TestMethod]
        public void AnyMinTwoMeasureAndWord()
        {
            s.RootQueryBlock
                .SetAny(w[22], new MeasureOp(MeasureOperator.GreaterOrEqual, 0.05, uom[1]) { Word = w[3] })
                .MinAny(2)
                .Search();

            Assert.AreEqual(1, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[22]));
        }

        [TestMethod]
        public void AllMeasure_AnyMeasure()
        {
            s.RootQueryBlock
                .SetAll(new MeasureOp(MeasureOperator.Equal, 0.05, uom[1]) { Word = w[3]})
                .SetAny(new MeasureOp(MeasureOperator.Equal, 0.05, uom[1]) { Word = w[3]})
                .Search();

            Assert.AreEqual(1, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[22]));
        }

        #endregion Measure

        #region Scope

        [TestMethod]
        public void AllInOneHr()
        {
            // в записи 1 и (3 или 4 или 22)  и (94 или 5)
            s.RootQueryBlock
                .Scope(SearchScope.HealthRecord)
                .All()
                .AddChild(x => x
                    .SetAll(w[1])
                    .SetAny(w[22], w[3], w[4]))
                .AddChild(x => x.SetAny(w[94], w[5]))
            .Search();

            Assert.AreEqual(1, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[20]));
        }

        [TestMethod]
        public void AnyInOneHrDistinct()
        {
            // в записи 1 и 22   или (1 и 3)
            s.RootQueryBlock
            .Scope(SearchScope.HealthRecord)
            .Any()
            .AddChild(x => x.SetAll(w[1], w[22]))
            .AddChild(x => x.SetAll(w[1], w[3]))
            .Search();

            Assert.AreEqual(3, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[20]));
            Assert.IsTrue(s.Contains(hr[22]));
            Assert.IsTrue(s.Contains(hr[72]));
        }

        [TestMethod]
        public void AnyInOneHr()
        {
            // в записи 1 и (4 или 22)  или (94 или 5) и нет 3
            s.RootQueryBlock
            .Scope(SearchScope.HealthRecord)
            .Any()
            .AddChild(x => x
                .SetAll(w[1])
                .SetAny(w[22], w[4]))
            .AddChild(x => x
                .SetAny(w[94], w[5])
                .SetNot(w[3]))
            .Search();

            Assert.AreEqual(5, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[2]));
            Assert.IsTrue(s.Contains(hr[22]));
            Assert.IsTrue(s.Contains(hr[30]));
            Assert.IsTrue(s.Contains(hr[31]));
            Assert.IsTrue(s.Contains(hr[72]));
        }

        [TestMethod]
        public void AllInOneHolder()
        {
            s.RootQueryBlock
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.SetAll(w[1]))
            .AddChild(x => x.SetAll(w[4]))
            .Search();

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[1]));
            Assert.IsTrue(s.Contains(hr[2]));
        }

        [TestMethod]
        public void AnyInOneHolder()
        {
            // в списке записи со словами 1 и 3 или записи со словом 22 без 1
            s.RootQueryBlock
            .Scope(SearchScope.Holder)
            .Any()
            .AddChild(x => x.SetAll(w[1], w[3]))
            .AddChild(x => x
                .SetAll(w[22])
                .SetNot(w[1]))
            .Search();

            Assert.AreEqual(6, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[20]));
            Assert.IsTrue(s.Contains(hr[22]));
            Assert.IsTrue(s.Contains(hr[70]));
            Assert.IsTrue(s.Contains(hr[73]));
            Assert.IsTrue(s.Contains(hr[74]));
            Assert.IsTrue(s.Contains(hr[40]));
        }

        [TestMethod]
        public void AllInOneHolder2()
        {
            // в списке записи с 31 или 3 и записи с 1 или 5
            s.RootQueryBlock
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.SetAny(w[31], w[3]))
            .AddChild(x => x.SetAny(w[1], w[5]))
            .Search();

            Assert.AreEqual(6, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[20]));
            Assert.IsTrue(s.Contains(hr[21]));
            Assert.IsTrue(s.Contains(hr[22]));
            Assert.IsTrue(s.Contains(hr[30]));
            Assert.IsTrue(s.Contains(hr[31]));
            Assert.IsTrue(s.Contains(hr[32]));
        }

        [TestMethod]
        public void AllInOnePatient()
        {
            s.RootQueryBlock
            .Scope(SearchScope.Patient)
            .All()
            .AddChild(x => x.SetAll(w[4]))
            .Search();

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[2]));
            Assert.IsTrue(s.Contains(hr[32]));
        }

        [TestMethod]
        public void AllInOnePatient_AllInOneHolder()
        {
            // у пациента записи со словами 4 и в одном списке записи со словами 1 и записи со словами 3

            s.RootQueryBlock
                .Scope(SearchScope.Patient)
                .All()
                .AddChild(x => x.SetAll(w[4]))
                .AddChild(x => x
                    .Scope(SearchScope.Holder)
                    .All()
                    .AddChild(y => y.SetAll(w[3]))
                    .AddChild(y => y.SetAll(w[1])))
                .Search();

            Assert.AreEqual(5, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[2]));
            Assert.IsTrue(s.Contains(hr[32]));
            Assert.IsTrue(s.Contains(hr[20]));
            Assert.IsTrue(s.Contains(hr[21]));
            Assert.IsTrue(s.Contains(hr[22]));
        }

        [TestMethod]
        public void AllInOneHr_WithExcludingOnly()
        {
            // в записи 1 и (3 или 4) и нет 94
            // в записи не должно быть исключаемых слов
            s.RootQueryBlock
            .Scope(SearchScope.HealthRecord)
            .All()
            .AddChild(x => x
                .SetAll(w[1])
                .SetAny(w[3], w[4]))
            .AddChild(x => x.SetNot(w[94]))
            .Search();

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[2]));
            Assert.IsTrue(s.Contains(hr[22]));
        }

        [TestMethod]
        public void AllInOneHr_WithExcludingOnly_WithCats()
        {
            // в записи 1 и (3 или 4) и нет (94 категории 1)
            Load<HrCategory>();
            s.RootQueryBlock
            .Scope(SearchScope.HealthRecord)
            .All()
            .AddChild(x => x
                .SetAll(w[1])
                .SetAny(w[3], w[4]))
            .AddChild(x => x
                .SetNot(w[94])
                .Check(cat[1]))
            .Search();

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[2]));
            Assert.IsTrue(s.Contains(hr[22]));
        }

        [TestMethod]
        public void AllInOneHr_TwoExcludingOnly()
        {
            // в записи нет 1, 31 и нет 22
            s.RootQueryBlock
            .Scope(SearchScope.HealthRecord)
            .All()
            .AddChild(x => x.SetNot(w[1], w[31]))
            .AddChild(x => x.SetNot(w[22]))
            .Search();

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[32]));
            Assert.IsTrue(s.Contains(hr[71]));
        }

        [TestMethod]
        public void AnyInOneHr_WithExcludingOnly()
        {
            // в записи 1 и (3 или 4) или нет 94, 22, 31
            //
            s.RootQueryBlock
            .Scope(SearchScope.HealthRecord)
            .Any()
            .AddChild(x => x
                .SetAll(w[1])
                .SetAny(w[3], w[4]))
            .AddChild(x => x.SetNot(w[94], w[22], w[31]))
            .Search();

            Assert.AreEqual(7, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[1]));
            Assert.IsTrue(s.Contains(hr[2]));
            Assert.IsTrue(s.Contains(hr[20]));
            Assert.IsTrue(s.Contains(hr[21]));
            Assert.IsTrue(s.Contains(hr[22]));
            Assert.IsTrue(s.Contains(hr[32]));
            Assert.IsTrue(s.Contains(hr[71])); // и пустые
        }

        [TestMethod]
        public void AllInOneHolder_WithExcludingOnly()
        {
            // в списке записи со словами 3 или 31 и нет записи со словом 5
            Load<HrCategory>();
            s.RootQueryBlock
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.SetAny(w[31], w[3]))
            .AddChild(x => x.SetNot(w[5]))
            .Search();

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[20]));
            Assert.IsTrue(s.Contains(hr[22]));
        }

        [TestMethod]
        public void AllInOneHolder_WithExcludingOnly_WithCats()
        {
            // в списке записи со словами 3 или 31 и нет записи со словом (5 категории 2)
            Load<HrCategory>();
            s.RootQueryBlock
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.SetAny(w[31], w[3]))
            .AddChild(x => x.SetNot(w[5]).Check(cat[2]))
            .Search();

            Assert.AreEqual(5, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(
                hr[20],
                hr[22],
                hr[30],
                hr[31],
                hr[32]));
        }

        [TestMethod]
        public void AllInOneHolder_WithExcludingOnly_OrderNoMatter()
        {
            // в списке нет записи со словом 5 и записи со словами 3 или 31

            s.RootQueryBlock
                .Scope(SearchScope.Holder)
                .All()
                .AddChild(x => x.SetNot(w[5]))
                .AddChild(x => x.SetAny(w[3], w[31]))
                .Search();

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[20], hr[22]));
        }

        [TestMethod]
        public void AllInOneHolder_ExcludingOnly()
        {
            // в списке нет записей с 5 или 22

            s.RootQueryBlock
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.SetNot(w[5], w[22]))
            .Search();

            Assert.AreEqual(3, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[1]));
            Assert.IsTrue(s.Contains(hr[2]));
            Assert.IsTrue(s.Contains(hr[71]));
        }

        [TestMethod]
        public void AnyInOneHolder_WithExcludingOnly()
        {
            // в списке записи со словами 3 или 31 или _нет записей со словами 5, 22_/(есть запись без слов 5, 22)
            // _по первому + все записи списка без тех, где 5/22_ или только те, где есть слова 3/31?
            s.RootQueryBlock
            .Scope(SearchScope.Holder)
            .Any()
            .AddChild(x => x.SetAny(w[31], w[3]))
            .AddChild(x => x.SetNot(w[5], w[22]))
            .Search();

            Assert.AreEqual(9, s.Result.Statistic.HealthRecords.Count);

            // если все записи списка
            Assert.IsTrue(s.Contains(hr[1]));
            Assert.IsTrue(s.Contains(hr[2]));
            Assert.IsTrue(s.Contains(hr[71])); // нет записей с 5 22
            Assert.IsTrue(s.Contains(hr[21])); // 2x - потому что есть запись c 3

            Assert.IsTrue(s.Contains(hr[20]));
            Assert.IsTrue(s.Contains(hr[22]));
            Assert.IsTrue(s.Contains(hr[30])); // c 31 и 5
            Assert.IsTrue(s.Contains(hr[31]));
            Assert.IsTrue(s.Contains(hr[32]));
        }

        [TestMethod]
        public void AllInOneHolder_TwoExcludingOnly()
        {
            // в списке нет записей с 2 и записей с 22
            s.RootQueryBlock
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.SetNot(w[2]))
            .AddChild(x => x.SetNot(w[22]))
            .Search();

            Assert.AreEqual(4, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[30]));
            Assert.IsTrue(s.Contains(hr[31]));
            Assert.IsTrue(s.Contains(hr[32]));
            Assert.IsTrue(s.Contains(hr[71]));
        }

        [TestMethod]
        public void AnyInOneHolder_TwoExcludingOnly()
        {
            // в списке нашлась запись без 1 или запись без 22
            // с каждого блока только те, где нет любого из этих слов
            s.RootQueryBlock
                .Scope(SearchScope.Holder)
                .Any()
                .AddChild(x => x.SetNot(w[1]))
                .AddChild(x => x.SetNot(w[22]))
                .Search();

            // все записи, кроме
            Assert.AreEqual(hrsTotal - 2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.NotContains(
                hr[22],
                hr[72]));
        }

        [TestMethod]
        public void AllInOneHolder_WithTwoExcludingOnly_WithCats()
        {
            // в списке нет записей с 22 категории 2 или 1 и нет записей с 4 категории 1
            Load<HrCategory>();

            s.RootQueryBlock
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.Check(AllCats().ToArray()))
            .AddChild(x => x
                .SetNot(w[22])
                .Check(cat[1], cat[2]))
            .AddChild(x => x
                .SetNot(w[4])
                .Check(cat[1]))
            .Search();

            Assert.AreEqual(6, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[1]));
            Assert.IsTrue(s.Contains(hr[2]));
            Assert.IsTrue(s.Contains(hr[71]));
            Assert.IsTrue(s.Contains(hr[72]));
            Assert.IsTrue(s.Contains(hr[73]));
            Assert.IsTrue(s.Contains(hr[74]));
        }

        [TestMethod]
        public void AllInOneHolder_WithTwoExcludingOnly_WithCats2()
        {
            // в списке запись с 94 и нет записи с (4 категории 1) и нет записи с 22
            Load<HrCategory>();
            s.RootQueryBlock
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.SetAny(w[94]))
            .AddChild(x => x.SetNot(w[4]).Check(cat[1]))
            .AddChild(x => x.SetNot(w[22]))
            .Search();

            Assert.AreEqual(0, s.Result.Statistic.HealthRecords.Count);
        }

        [TestMethod]
        public void AllInOneHolder_WithTwoExcludingOnly_WithCats3()
        {
            // в списке запись с 3 и нет записи с (94 категории 2) и нет записи с 22
            Load<HrCategory>();
            s.RootQueryBlock
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.SetAny(w[3]))
            .AddChild(x => x.SetNot(w[94]).Check(cat[2]))
            .AddChild(x => x.SetNot(w[22]))
            .Search();
            Assert.AreEqual(0, s.Result.Statistic.HealthRecords.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(AssertFailedException))]
        public void AnyInOneHolder_TwoExcludingOnly_WithCats()
        {
            // в списке нашлась запись без 22 категории (2 или 1) или без 4 категории 1
            // то есть все записи
            Load<HrCategory>();

            s.RootQueryBlock
            .Scope(SearchScope.Holder)
            .Any()
            .AddChild(g => g
                .Scope(SearchScope.Holder)
                .All()
                .AddChild(x => x
                    .SetNot(w[22])
                    .Check(cat[1], cat[2]))
                .AddChild(x => x
                    .Check(AllCats()
                    //  .Except(new[] { cat[1], cat[2] })
                        .ToArray())))
            .AddChild(g => g
                .Scope(SearchScope.Holder)
                .All()
                .AddChild(x => x
                    .SetNot(w[4])
                    .Check(cat[1]))
                .AddChild(x => x
                    .Check(AllCats()
                    //   .Except(cat[1])
                        .ToArray())))
            .Search();

            Assert.AreEqual(hrsTotal, s.Result.Statistic.HealthRecords.Count);
        }

        [TestMethod]
        public void AllInOneHolder_ExcludingOnly_WithCats()
        {
            // список, где есть записи 2 категории без 94
            Load<HrCategory>();

            s.RootQueryBlock
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x
                .Check(cat[2])
                .SetNot(w[94]))
            .Search();

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[40], hr[22]));
        }

        [TestMethod]
        public void AllInOneHolder_WithExcludingOnly_WithCats2()
        {
            // записи 2 категории списка, где нет 94
            Load<HrCategory>();

            s.RootQueryBlock
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.Check(cat[2]))
            .AddChild(x => x.SetNot(w[94]))
            .Search();

            Assert.AreEqual(1, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[40]));
        }

        [TestMethod]
        public void AnyInOneHr_ExcludingOnly_AndCats()
        {
            // записи кроме (1 категории со словом 31 или 22)
            Load<HrCategory>();

            s.RootQueryBlock
            .Scope(SearchScope.HealthRecord)
            .Any()
            .AddChild(x => x
                .Check(AllCats()
                    .Except(cat[1]).ToArray()))
            .AddChild(x => x
                .SetNot(w[22], w[31])
                .Check(cat[1]))
            .Search();

            Assert.AreEqual(hrsTotal - 2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.NotContains(
                hr[70],
                hr[30]));
        }

        [TestMethod]
        public void AllInOneHr_ExcludingOnly_WithCats()
        {
            // записи 2 категории без 1
            Load<HrCategory>();

            s.RootQueryBlock
            .Scope(SearchScope.HealthRecord)
            .All()
            .AddChild(x => x
                .SetNot(w[1])
                .Check(cat[2]))
            .Search();

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[31], hr[40]));
        }

        [TestMethod]
        public void AllInOneHr_WithCats_FoundAllHrs()
        {
            Load<HrCategory>();
            s.RootQueryBlock
            .Scope(SearchScope.HealthRecord)
            .All()
            .AddChild(x => x.Check(cat[2]))
            .Search();

            Assert.AreEqual(3, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[31]));
            Assert.IsTrue(s.Contains(hr[22]));
            Assert.IsTrue(s.Contains(hr[40]));
        }

        [TestMethod]
        public void AllInOneHolder_WithCats()
        {
            // в списке есть записи 1 и 5 кат
            Load<HrCategory>();
            s.RootQueryBlock
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.Check(cat[1]))
            .AddChild(x => x.Check(cat[5]))
            .Search();

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[20]));
            Assert.IsTrue(s.Contains(hr[21]));
        }

        [TestMethod]
        public void AnyInOneHolder_WithCats()
        {
            // в списке записи 3 или 5 кат
            Load<HrCategory>();
            s.RootQueryBlock
            .Scope(SearchScope.Holder)
            .Any()
            .AddChild(x => x.Check(cat[3]))
            .AddChild(x => x.Check(cat[5]))
            .Search();

            Assert.AreEqual(3, s.Result.Statistic.HealthRecords.Count);
            // совпадает с AnyInOneHr_WithCats, смысл - здесь ищем список, показываем только подходящие записи
            Assert.IsTrue(s.Contains(hr[1]));
            Assert.IsTrue(s.Contains(hr[2]));
            Assert.IsTrue(s.Contains(hr[21]));
        }

        [TestMethod]
        public void AllInOneHr_WithCats()
        {
            // у записи вообще не бывает две категории
            Load<HrCategory>();
            s.RootQueryBlock
            .Scope(SearchScope.HealthRecord)
            .All()
            .AddChild(x => x.Check(cat[3]))
            .AddChild(x => x.Check(cat[5]))
            .Search();

            Assert.AreEqual(0, s.Result.Statistic.HealthRecords.Count);
        }

        [TestMethod]
        public void AnyInOneHr_WithCats()
        {
            // записи 3 или 5 кат
            Load<HrCategory>();
            s.RootQueryBlock
            .Scope(SearchScope.HealthRecord)
            .Any()
            .AddChild(x => x.Check(cat[3]))
            .AddChild(x => x.Check(cat[5]))
            .Search();

            Assert.AreEqual(3, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[1]));
            Assert.IsTrue(s.Contains(hr[2]));
            Assert.IsTrue(s.Contains(hr[21]));
        }

        [TestMethod]
        public void RootExcludingOnly_WithCat()
        {
            // записи 1 категории, где нет слов 22
            Load<HrCategory>();

            s.RootQueryBlock
                .Check(cat[1])
                .SetNot(w[22])
                .Search();

            Assert.AreEqual(4, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[20]));
            Assert.IsTrue(s.Contains(hr[30]));
            Assert.IsTrue(s.Contains(hr[32]));
            Assert.IsTrue(s.Contains(hr[71]));
        }

        #region AllAny_WithSingleChild_SameResults

        [TestMethod]
        public void AnyInOneHolder_ExcludingOnly()
        {
            // в списке нашлась запись без слов 5, 22
            // все записи списка или _только те, где нет этих слов_?
            s.RootQueryBlock
            .Scope(SearchScope.Holder)
            .Any()
            .AddChild(x => x.SetNot(w[5], w[22]))
            .Search();

            Assert.AreEqual(7, s.Result.Statistic.HealthRecords.Count);
            // если все записи списка
            //Assert.IsTrue(s.Contains(hr[22]));
            //Assert.IsTrue(s.Contains(hr[30]));
            Assert.IsTrue(s.Contains(hr[1]));
            Assert.IsTrue(s.Contains(hr[2]));
            Assert.IsTrue(s.Contains(hr[20]));
            Assert.IsTrue(s.Contains(hr[21]));
            Assert.IsTrue(s.Contains(hr[31]));
            Assert.IsTrue(s.Contains(hr[32]));
            Assert.IsTrue(s.Contains(hr[71]));
        }

        [TestMethod]
        public void AnyInOnePatient()
        {
            s.RootQueryBlock
            .Scope(SearchScope.Patient)
            .Any()
            .AddChild(x => x.SetAll(w[4]))
            .Search();

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[2]));
            Assert.IsTrue(s.Contains(hr[32]));
        }



        [TestMethod]
        public void AnyInOneHr_WithCats_FoundAllHrs()
        {
            Load<HrCategory>();
            s.RootQueryBlock
            .Scope(SearchScope.HealthRecord)
            .Any()
            .AddChild(x => x.Check(cat[2]))
            .Search();

            Assert.AreEqual(3, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[31]));
            Assert.IsTrue(s.Contains(hr[22]));
            Assert.IsTrue(s.Contains(hr[40]));
        }

        [TestMethod]
        public void AnyInOneHr_ExcludingOnly_WithCats()
        {
            // записи 2 категории без 1
            Load<HrCategory>();

            s.RootQueryBlock
            .Scope(SearchScope.HealthRecord)
            .Any()
            .AddChild(x => x
                .SetNot(w[1])
                .Check(cat[2]))
            .Search();

            Assert.AreEqual(2, s.Result.Statistic.HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[31], hr[40]));
        }

        #endregion AllAny_WithSingleChild_SameResults

        #endregion Scope

        private System.Collections.Generic.IEnumerable<HrCategory> AllCats()
        {
            return cat.Values.Union(HrCategory.Null.ToEnumerable());
        }
    }

    public static class QbExtensions
    {
        public static QueryBlockViewModel AddChild(this QueryBlockViewModel parent, Action<QueryBlockViewModel> onChild)
        {
            parent.AddChildQbCommand.Execute(null);
            var child = parent.Children.Last();
            onChild(child);
            return parent;
        }

        public static QueryBlockViewModel All(this QueryBlockViewModel qb)
        {
            qb.All = true;
            return qb;
        }

        public static QueryBlockViewModel Any(this QueryBlockViewModel qb)
        {
            qb.All = false;
            return qb;
        }

        public static QueryBlockViewModel Scope(this QueryBlockViewModel qb, SearchScope scope)
        {
            qb.SearchScope = scope;
            return qb;
        }

        public static QueryBlockViewModel SetAll(this QueryBlockViewModel qb, params IHrItemObject[] all)
        {
            qb.AutocompleteAll.ReplaceTagsWith(all);
            return qb;
        }

        public static QueryBlockViewModel SetAny(this QueryBlockViewModel qb, params IHrItemObject[] any)
        {
            qb.AutocompleteAny.ReplaceTagsWith(any);
            return qb;
        }

        public static QueryBlockViewModel SetNot(this QueryBlockViewModel qb, params IHrItemObject[] not)
        {
            qb.AutocompleteNot.ReplaceTagsWith(not);
            return qb;
        }

        public static QueryBlockViewModel MinAny(this QueryBlockViewModel qb, int min)
        {
            qb.AnyMin = min;
            return qb;
        }

        public static QueryBlockViewModel Check(this QueryBlockViewModel qb, params HrCategory[] cats)
        {
            qb.SelectCategory(cats);
            return qb;
        }

        public static QueryBlockViewModel Search(this QueryBlockViewModel qb)
        {
            qb.SearchCommand.Execute(null);
            return qb;
        }

        public static bool Contains(this SearchViewModel s, params HealthRecord[] hrs)
        {
            return hrs.All(x => s.Result.Statistic.HealthRecords.Contains(x));
        }

        public static bool NotContains(this SearchViewModel s, params HealthRecord[] hrs)
        {
            return hrs.All(x => !s.Result.Statistic.HealthRecords.Contains(x));
        }
    }
}