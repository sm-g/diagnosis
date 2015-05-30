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
    public class SearcherTest : InMemoryDatabaseTest
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
        public void AllInHr_AllAnySame_Case1()
        {
            // к записи подходит: 22 и (22 или 1) = 22
            var hrs = o
                  .Scope(SearchScope.HealthRecord)
                  .All()
                  .SetAll(w[22])
                  .SetAny(w[22], w[1])
                  .Search(session);

            Assert.AreEqual(6, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[70]));
            Assert.IsTrue(hrs.Contains(hr[72]));
            Assert.IsTrue(hrs.Contains(hr[73]));
            Assert.IsTrue(hrs.Contains(hr[74]));
            Assert.IsTrue(hrs.Contains(hr[40]));
        }

        [ExpectedException(typeof(AssertFailedException))]
        [TestMethod]
        public void AllInHr_AllAnySame_Case2()
        {
            // в записи 22 и еще есть (22 или 1)
            var hrs = o
                  .Scope(SearchScope.HealthRecord)
                  .All()
                  .SetAll(w[22])
                  .SetAny(w[22], w[1])
                  .Search(session);

            Assert.AreEqual(3, hrs.Count());
            Assert.IsTrue(hrs.IsSuperSetOf(
               hr[22],
               hr[70],
               hr[72]));
        }

        [TestMethod]
        public void AllInHr()
        {
            // в записи 1 и (3 или 4 или 22)  и (94 или 5)
            var hrs = o
                  .Scope(SearchScope.HealthRecord)
                  .All()
                  .AddChild(x => x
                      .SetAll(w[1])
                      .SetAny(w[22], w[3], w[4]))
                  .AddChild(x => x.SetAny(w[94], w[5]))
                  .Search(session);

            Assert.AreEqual(1, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[20]));
        }

        [TestMethod]
        public void AnyInHrDistinct()
        {
            // в записи 1 и 22   или (1 и 3)
            var hrs = o
            .Scope(SearchScope.HealthRecord)
            .Any()
            .AddChild(x => x.SetAll(w[1], w[22]))
            .AddChild(x => x.SetAll(w[1], w[3]))
            .Search(session);

            Assert.AreEqual(3, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[72]));
        }

        [TestMethod]
        public void AnyInHr()
        {
            // в записи 1 и (4 или 22)  или (94 или 5) и нет 3
            var hrs = o
            .Scope(SearchScope.HealthRecord)
            .Any()
            .AddChild(x => x
                .SetAll(w[1])
                .SetAny(w[22], w[4]))
            .AddChild(x => x
                .SetAny(w[94], w[5])
                .SetNot(w[3]))
            .Search(session);

            Assert.AreEqual(5, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[30]));
            Assert.IsTrue(hrs.Contains(hr[31]));
            Assert.IsTrue(hrs.Contains(hr[72]));
        }

        [TestMethod]
        public void AnyInHr_MinAny()
        {
            var hrs = o
                .Scope(SearchScope.HealthRecord)
                .MinAny(2)
                .SetAny(w[1], w[22], w[3])
                .Search(session);

            Assert.AreEqual(3, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[72]));
        }

        [TestMethod]
        public void AllInHolder()
        {
            var hrs = o
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.SetAll(w[1]))
            .AddChild(x => x.SetAll(w[4]))
            .Search(session);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
        }

        [TestMethod]
        public void AnyInHolder()
        {
            // в списке записи со словами 1 и 3 или записи со словом 22 без 1
            var hrs = o
            .Scope(SearchScope.Holder)
            .Any()
            .AddChild(x => x.SetAll(w[1], w[3]))
            .AddChild(x => x
                .SetAll(w[22])
                .SetNot(w[1]))
            .Search(session);

            Assert.AreEqual(6, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[70]));
            Assert.IsTrue(hrs.Contains(hr[73]));
            Assert.IsTrue(hrs.Contains(hr[74]));
            Assert.IsTrue(hrs.Contains(hr[40]));
        }

        [TestMethod]
        public void AllInHolder2()
        {
            // в списке записи с 31 или 3 и записи с 1 или 5
            var hrs = o
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.SetAny(w[31], w[3]))
            .AddChild(x => x.SetAny(w[1], w[5]))
            .Search(session);

            Assert.AreEqual(6, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[21]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[30]));
            Assert.IsTrue(hrs.Contains(hr[31]));
            Assert.IsTrue(hrs.Contains(hr[32]));
        }

        [TestMethod]
        public void AllInPatient()
        {
            var hrs = o
            .Scope(SearchScope.Patient)
            .All()
            .AddChild(x => x.SetAll(w[4]))
            .Search(session);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[32]));
        }

        [TestMethod]
        public void AllInPatient_AllInHolder()
        {
            // у пациента записи со словами 4 и в одном списке записи со словами 1 и записи со словами 3

            var hrs = o
                .Scope(SearchScope.Patient)
                .All()
                .AddChild(x => x.SetAll(w[4]))
                .AddChild(x => x
                    .Scope(SearchScope.Holder)
                    .All()
                    .AddChild(y => y.SetAll(w[3]))
                    .AddChild(y => y.SetAll(w[1])))
                .Search(session);

            Assert.AreEqual(5, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[32]));
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[21]));
            Assert.IsTrue(hrs.Contains(hr[22]));
        }

        [TestMethod]
        public void AllInHr_WithExcludingOnly()
        {
            // в записи 1 и (3 или 4) и нет 94
            // в записи не должно быть исключаемых слов
            var hrs = o
            .Scope(SearchScope.HealthRecord)
            .All()
            .AddChild(x => x
                .SetAll(w[1])
                .SetAny(w[3], w[4]))
            .AddChild(x => x.SetNot(w[94]))
            .Search(session);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[22]));
        }

        [TestMethod]
        public void AllInHr_WithExcludingOnly_WithCats()
        {
            // в записи 1 и (3 или 4) и нет (94 категории 1)
            Load<HrCategory>();
            var hrs = o
            .Scope(SearchScope.HealthRecord)
            .All()
            .AddChild(x => x
                .SetAll(w[1])
                .SetAny(w[3], w[4]))
            .AddChild(x => x
                .SetNot(w[94])
                .Check(cat[1]))
            .Search(session);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[22]));
        }

        [TestMethod]
        public void AllInHr_TwoExcludingOnly()
        {
            // в записи нет 1, 31 и нет 22
            var hrs = o
            .Scope(SearchScope.HealthRecord)
            .All()
            .AddChild(x => x.SetNot(w[1], w[31]))
            .AddChild(x => x.SetNot(w[22]))
            .Search(session);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[32]));
            Assert.IsTrue(hrs.Contains(hr[71])); // только 71 если в списке
        }

        [TestMethod]
        public void AnyInHr_WithExcludingOnly()
        {
            // в записи 1 и (3 или 4) или нет 94, 22, 31
            //
            var hrs = o
            .Scope(SearchScope.HealthRecord)
            .Any()
            .AddChild(x => x
                .SetAll(w[1])
                .SetAny(w[3], w[4]))
            .AddChild(x => x.SetNot(w[94], w[22], w[31]))
            .Search(session);

            Assert.AreEqual(7, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[21]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[32]));
            Assert.IsTrue(hrs.Contains(hr[71])); // и пустые
        }

        [TestMethod]
        public void AllInHolder_WithExcludingOnly()
        {
            // в списке записи со словами 3 или 31 и нет записи со словом 5
            Load<HrCategory>();
            var hrs = o
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.SetAny(w[31], w[3]))
            .AddChild(x => x.SetNot(w[5]))
            .Search(session);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[22]));
        }

        [TestMethod]
        public void AllInHolder_WithExcludingOnly_WithCats()
        {
            // в списке записи со словами 3 или 31 и нет записи со словом (5 категории 2)
            Load<HrCategory>();
            var hrs = o
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.SetAny(w[31], w[3]))
            .AddChild(x => x.SetNot(w[5]).Check(cat[2]))
            .Search(session);

            Assert.AreEqual(5, hrs.Count());
            Assert.IsTrue(hrs.IsSuperSetOf(
                hr[20],
                hr[22],
                hr[30],
                hr[31],
                hr[32]));
        }

        [TestMethod]
        public void AllInHolder_WithExcludingOnly_OrderNoMatter()
        {
            // в списке нет записи со словом 5 и есть записи со словами 3 или 31

            var hrs = o
                .Scope(SearchScope.Holder)
                .All()
                .AddChild(x => x.SetNot(w[5]))
                .AddChild(x => x.SetAny(w[3], w[31]))
                .Search(session);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.IsSuperSetOf(hr[20], hr[22]));
        }

        [TestMethod]
        public void AllInHolder_ExcludingOnly()
        {
            // в списке нет записей с 5 или 22

            var hrs = o
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.SetNot(w[5], w[22]))
            .Search(session);

            Assert.AreEqual(3, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[71]));
        }

        [TestMethod]
        public void InHolder_AnyInHr_TwoExcludingOnly()
        {
            // в списке нашлась запись без 5 или запись без 22
            var hrs = o
                .Any()
                .Scope(SearchScope.Holder)
                .AddChild(y => y
                    .Scope(SearchScope.HealthRecord) // можно просто InHolder_TwoExcludingOnly
                    .Any()
                    .AddChild(x => x.SetNot(w[5]))
                    .AddChild(x => x.SetNot(w[22])))
                .Search(session);

            Assert.AreEqual(hrsTotal, hrs.Count());
        }

        [TestMethod]
        public void AnyInHolder_WithExcludingOnly()
        {
            // в списке записи со словами 3 или 31 или нет записей со словами 5, 22
            // по первому + все записи списка без тех, где 5/22
            var hrs = o
            .Scope(SearchScope.Holder)
            .Any()
            .AddChild(x => x.SetAny(w[31], w[3]))
            .AddChild(x => x.SetNot(w[5], w[22]))
            .Search(session);

            Assert.AreEqual(9, hrs.Count());

            // если все записи списка
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[71])); // нет записей с 5 22
            Assert.IsTrue(hrs.Contains(hr[21])); // 2_ - потому что есть запись c 3
            // 21 - как (без 5 22)
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[30])); // c 31 и 5
            Assert.IsTrue(hrs.Contains(hr[31]));
            Assert.IsTrue(hrs.Contains(hr[32]));
        }

        [TestMethod]
        public void AllInHolder_TwoExcludingOnly()
        {
            // в списке нет записей с 2 и записей с 22
            var hrs = o
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.SetNot(w[2]))
            .AddChild(x => x.SetNot(w[22]))
            .Search(session);

            Assert.AreEqual(4, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[30]));
            Assert.IsTrue(hrs.Contains(hr[31]));
            Assert.IsTrue(hrs.Contains(hr[32]));
            Assert.IsTrue(hrs.Contains(hr[71]));
        }

        [TestMethod]
        public void AnyInHolder_TwoExcludingOnly()
        {
            // в списке нашлась запись без 1 или запись без 22
            // с каждого блока только те, где нет любого из этих слов
            var hrs = o
                .Scope(SearchScope.Holder)
                .Any()
                .AddChild(x => x.SetNot(w[1]))
                .AddChild(x => x.SetNot(w[22]))
                .Search(session);

            // все записи, кроме
            Assert.AreEqual(hrsTotal - 2, hrs.Count());
            Assert.IsTrue(hrs.NotContains(
                hr[22],
                hr[72]));
        }

        [TestMethod]
        public void AllInHolder_All_ExcludingOnly_WithCats()
        {
            // список, где нет (записи 2 категории с 94)
            Load<HrCategory>();

            var hrs = o
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => { })
            .AddChild(x => x.Check(cat[2]).SetNot(w[94]))
            .Search(session);

            Assert.AreEqual(hrsTotal - 3, hrs.Count());
            Assert.IsTrue(hrs.NotContains(
                hr[30],
                hr[31],
                hr[32]));
        }

        [TestMethod]
        public void AllInHolder_All_WithTwoExcludingOnly_WithCats()
        {
            // в списке нет записей с (22 категории 2 или 1) и нет записей с (4 категории 1)
            Load<HrCategory>();

            var hrs = o
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => { })
            .AddChild(x => x
                .SetNot(w[22])
                .Check(cat[1], cat[2]))
            .AddChild(x => x
                .SetNot(w[4])
                .Check(cat[1]))
            .Search(session);

            Assert.AreEqual(6, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[71]));
            Assert.IsTrue(hrs.Contains(hr[72]));
            Assert.IsTrue(hrs.Contains(hr[73]));
            Assert.IsTrue(hrs.Contains(hr[74]));
        }

        [TestMethod]
        public void NotAnyInHolder_WithCats()
        {
            // в списке нет записей с (22 категории 2 или 1) и нет записей с (4 категории 1)
            // AllInHolder_WithTwoExcludingOnly_WithCats
            Load<HrCategory>();

            var hrs = o
            .Scope(SearchScope.Holder)
            .NotAny()
            .AddChild(x => x
                .SetAll(w[22])
                .Check(cat[1], cat[2]))
            .AddChild(x => x
                .SetAll(w[4])
                .Check(cat[1]))
            .Search(session);

            Assert.AreEqual(6, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[71]));
            Assert.IsTrue(hrs.Contains(hr[72]));
            Assert.IsTrue(hrs.Contains(hr[73]));
            Assert.IsTrue(hrs.Contains(hr[74]));
        }

        [TestMethod]
        public void AllInHolder_WithTwoExcludingOnly_WithCats2()
        {
            // в списке запись с 94 и нет записи с (4 категории 1) и нет записи с 22
            Load<HrCategory>();
            var hrs = o
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.SetAll(w[94]))
            .AddChild(x => x.SetNot(w[4]).Check(cat[1]))
            .AddChild(x => x.SetNot(w[22]))
            .Search(session);

            Assert.AreEqual(0, hrs.Count());
        }

        [TestMethod]
        public void AllInHolder_WithTwoExcludingOnly_WithCats3()
        {
            // в списке запись с 94 и есть запись без (4 категории 1) и нет записи с 22
            Load<HrCategory>();
            var hrs = o
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.SetAll(w[94]))
            .AddChild(x => x
                .Scope(SearchScope.HealthRecord)
                .Any()
                .AddChild(y => y
                    .Check(AllCats()
                        .Except(cat[1]).ToArray()))
                .AddChild(y => y
                    .SetNot(w[4])
                    .Check(cat[1])))
            .AddChild(x => x.SetNot(w[22]))
            .Search(session);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[30]));
            Assert.IsTrue(hrs.Contains(hr[31]));
        }

        [TestMethod]
        public void AllInHolder_WithTwoExcludingOnly_WithCats4()
        {
            // в списке запись с 3 и нет записи с (94 категории 2) и нет записи с 22
            Load<HrCategory>();
            var hrs = o
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.SetAll(w[3]))
            .AddChild(x => x.SetNot(w[94]).Check(cat[2]))
            .AddChild(x => x.SetNot(w[22]))
            .Search(session);
            Assert.AreEqual(0, hrs.Count());
        }

        [TestMethod]
        public void AnyInHolder_All_TwoExcludingOnly_WithCats()
        {
            // в списке нашлась запись без (22 категории 2 или 1) или без (4 категории 1)
            // то есть все записи
            Load<HrCategory>();

            var hrs = o
            .Scope(SearchScope.Holder)
            .Any()
            .AddChild(g => g
                .Scope(SearchScope.Holder)
                .Any()
                .AddChild(x => x
                    .SetNot(w[22])
                    .Check(cat[1], cat[2]))
                .AddChild(x => { }))
            .AddChild(g => g
                .Scope(SearchScope.Holder)
                .Any()
                .AddChild(x => x
                    .SetNot(w[4])
                    .Check(cat[1]))
                .AddChild(x => { }))
            .Search(session);

            Assert.AreEqual(hrsTotal, hrs.Count());
        }

        [TestMethod]
        public void AllInHolder_TwoExcludingOnly_WithCats()
        {
            // в списке есть запись без 22 категории (2 или 1) и без 4 категории 1
            Load<HrCategory>();

            var hrs = o
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(g => g
                .Scope(SearchScope.Holder)
                .Any()
                .AddChild(x => x
                    .SetNot(w[22])
                    .Check(cat[1], cat[2]))
                .AddChild(x => x
                    .Check(AllCats()
                        .Except(new[] { cat[1], cat[2] })
                        .ToArray())))
            .AddChild(g => g
                .Scope(SearchScope.Holder)
                .Any()
                .AddChild(x => x
                    .SetNot(w[4])
                    .Check(cat[1]))
                .AddChild(x => x
                    .Check(AllCats()
                       .Except(cat[1])
                        .ToArray())))
            .Search(session);

            Assert.AreEqual(hrsTotal - 2, hrs.Count());
            Assert.IsTrue(hrs.NotContains(
                hr[70],
                hr[40]));
        }

        [TestMethod]
        public void AllInHolder_ExcludingOnly_WithCats()
        {
            // список, где есть записи 2 категории без 94
            Load<HrCategory>();

            var hrs = o
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.Check(cat[2]).SetNot(w[94]))
            .Search(session);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.IsSuperSetOf(hr[40], hr[22]));
        }

        [TestMethod]
        public void AllInHolder_WithExcludingOnly_WithCats2()
        {
            // записи 2 категории списка, где нет 94
            Load<HrCategory>();

            var hrs = o
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.Check(cat[2]))
            .AddChild(x => x.SetNot(w[94]))
            .Search(session);

            Assert.AreEqual(1, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[40]));
        }

        [TestMethod]
        public void AnyInHr_ExcludingOnly_AndCats()
        {
            // записи кроме (1 категории со словом 31 или 22)
            Load<HrCategory>();

            var hrs = o
            .Scope(SearchScope.HealthRecord)
            .Any()
            .AddChild(x => x
                .Check(AllCats()
                    .Except(cat[1]).ToArray()))
            .AddChild(x => x
                .SetNot(w[22], w[31])
                .Check(cat[1]))
            .Search(session);

            Assert.AreEqual(hrsTotal - 2, hrs.Count());
            Assert.IsTrue(hrs.NotContains(
                hr[70],
                hr[30]));
        }

        [TestMethod]
        public void AnyInHolder_ExcludingOnly_AndCats()
        {
            // список, где есть запись, не равная (со словом 4 категории 1)
            // есть запись без 4 или не категории 1
            // похоже на AllInHolder_All_ExcludingOnly_WithCats
            Load<HrCategory>();

            var hrs = o
            .Scope(SearchScope.Holder)
            .Any()
            .AddChild(x => x
                .Check(AllCats()
                    .Except(cat[1]).ToArray()))
            .AddChild(x => x.SetNot(w[4]))
            .Search(session);

            Assert.AreEqual(hrsTotal - 1, hrs.Count());
            Assert.IsTrue(hrs.NotContains(
                hr[32]));
        }

        [TestMethod]
        public void AllInHr_ExcludingOnly_WithCats()
        {
            // записи 2 категории без 1
            Load<HrCategory>();

            var hrs = o
            .Scope(SearchScope.HealthRecord)
            .All()
            .AddChild(x => x
                .SetNot(w[1])
                .Check(cat[2]))
            .Search(session);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.IsSuperSetOf(hr[31], hr[40]));
        }

        [TestMethod]
        public void AllInHr_TwoExcludingOnly_WithCats()
        {
            // запись должна быть (2 категории без 1) и (1 или 2 категории без 22)
            Load<HrCategory>();

            var hrs = o
            .Scope(SearchScope.HealthRecord)
            .All()
            .AddChild(x => x
                .SetNot(w[1])
                .Check(cat[2]))
            .AddChild(x => x
                .SetNot(w[22])
                .Check(cat[2], cat[1]))
            .Search(session);

            Assert.AreEqual(1, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[31]));
        }

        [TestMethod]
        public void AllInHr_WithCats0()
        {
            Load<HrCategory>();
            var hrs = o
            .Scope(SearchScope.HealthRecord)
            .All()
            .AddChild(x => x.Check(cat[2]))
            .Search(session);

            Assert.AreEqual(3, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[31]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[40]));
        }

        [TestMethod]
        public void AllInHolder_WithCats()
        {
            // в списке есть записи 1 и 5 кат
            Load<HrCategory>();
            var hrs = o
            .Scope(SearchScope.Holder)
            .All()
            .AddChild(x => x.Check(cat[1]))
            .AddChild(x => x.Check(cat[5]))
            .Search(session);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[21]));
        }

        [TestMethod]
        public void AnyInHolder_WithCats()
        {
            // в списке записи 3 или 5 кат
            Load<HrCategory>();
            var hrs = o
            .Scope(SearchScope.Holder)
            .Any()
            .AddChild(x => x.Check(cat[3]))
            .AddChild(x => x.Check(cat[5]))
            .Search(session);

            Assert.AreEqual(3, hrs.Count());
            // совпадает с AnyInHr_WithCats, смысл - здесь ищем список, показываем только подходящие записи
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[21]));
        }

        [TestMethod]
        public void AllInHr_WithCats()
        {
            // у записи вообще не бывает две категории
            Load<HrCategory>();
            var hrs = o
            .Scope(SearchScope.HealthRecord)
            .All()
            .AddChild(x => x.Check(cat[3]))
            .AddChild(x => x.Check(cat[5]))
            .Search(session);

            Assert.AreEqual(0, hrs.Count());
        }

        [TestMethod]
        public void AnyInHr_WithCats()
        {
            // записи 3 или 5 кат
            Load<HrCategory>();
            var hrs = o
            .Scope(SearchScope.HealthRecord)
            .Any()
            .AddChild(x => x.Check(cat[3]))
            .AddChild(x => x.Check(cat[5]))
            .Search(session);

            Assert.AreEqual(3, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[21]));
        }

        [TestMethod]
        public void RootExcludingOnly_WithCat()
        {
            // записи 1 категории, где нет слов 22
            Load<HrCategory>();

            var hrs = o
                .Check(cat[1])
                .SetNot(w[22])
                .Search(session);

            Assert.AreEqual(4, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[30]));
            Assert.IsTrue(hrs.Contains(hr[32]));
            Assert.IsTrue(hrs.Contains(hr[71]));
        }

        [TestMethod]
        public void NotAnyInHr()
        {
            // записи без 1 и без (22 или 31)
            Load<HrCategory>();

            var hrs = o
                .Scope(SearchScope.HealthRecord)
                .NotAny()
                .AddChild(x => x.SetAny(w[22], w[31]))
                .AddChild(x => x.SetAll(w[1]))
                .Search(session);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[32]));
            Assert.IsTrue(hrs.Contains(hr[71]));
        }

        [TestMethod]
        public void NotAnyInHolder()
        {
            // все записи списков без 1,31,кат3
            Load<HrCategory>();

            var hrs = o
                .Scope(SearchScope.Holder)
                .NotAny()
                .AddChild(x => x.SetAny(w[22], w[31]))
                .AddChild(x => x.Check(cat[3]))
                .Search(session);

            Assert.AreEqual(1, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[71]));
        }

        [TestMethod]
        public void NotAnyInHolder_NotAnyHr()
        {
            // списки без 4 и без записей (без 94 или не кат 1)
            // похоже на AllInHolder_AnyHr
            Load<HrCategory>();

            var hrs = o
                .Scope(SearchScope.Holder)
                .NotAny()
                .AddChild(x => x
                    .Scope(SearchScope.HealthRecord)
                    .NotAny()
                    .AddChild(y => y.SetAll(w[94]))
                    .AddChild(y => y.Check(cat[1])))
                .AddChild(x => x.SetAll(w[4]))
                .Search(session);

            Assert.AreEqual(2, hrs.Count());
            // 20 нет, потому что в списке есть записи (без 94 или не кат 1) - 21 и 22
            Assert.IsTrue(hrs.Contains(hr[70]));
            Assert.IsTrue(hrs.Contains(hr[71]));
        }

        [TestMethod]
        public void AllInHolder_AnyHr()
        {
            // списки без 4 и с любым из (94, кат 1)
            Load<HrCategory>();

            var hrs = o
                .Scope(SearchScope.Holder)
                .All()
                .AddChild(x => x
                    .Scope(SearchScope.HealthRecord)
                    .Any()
                    .AddChild(y => y.SetAll(w[94]))
                    .AddChild(y => y.Check(cat[1])))
                .AddChild(x => x.SetNot(w[4]))
                .Search(session);

            Assert.AreEqual(3, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[20]));
            Assert.IsTrue(hrs.Contains(hr[70]));
            Assert.IsTrue(hrs.Contains(hr[71]));
        }

        [TestMethod]
        public void NotAnyInHodler_AnyHr()
        {
            // списки где нет ни одной (записи с 22) или (с любым из (94, кат 1))
            Load<HrCategory>();

            var hrs = o
                .Scope(SearchScope.Holder)
                .NotAny()
                .AddChild(x => x
                    .Scope(SearchScope.HealthRecord)
                    .Any()
                    .AddChild(y => y.SetAll(w[94]))
                    .AddChild(y => y.Check(cat[1])))
                .AddChild(x => x.SetAny(w[22]))
                .Search(session);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
        }

        [TestMethod]
        public void NotAnyInHodler_AnyHr2()
        {
            // списки где нет ни одной (записи с 1) или (с любым из (94, кат 1))
            Load<HrCategory>();

            var hrs = o
                .Scope(SearchScope.Holder)
                .NotAny()
                .AddChild(x => x
                    .Scope(SearchScope.HealthRecord)
                    .Any()
                    .AddChild(y => y.SetAll(w[94]))
                    .AddChild(y => y.Check(cat[1])))
                .AddChild(x => x.SetAll(w[1]))
                .Search(session);

            Assert.AreEqual(3, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[40]));
            Assert.IsTrue(hrs.Contains(hr[73]));
            Assert.IsTrue(hrs.Contains(hr[74]));
        }

        [TestMethod]
        public void NotAnyInHr_nested3()
        {
            // списки где нет ни одной (записи с 4 или 22) или (с любым из (94, кат 1))
            Load<HrCategory>();

            var hrs = o
                .Scope(SearchScope.Holder)
                .All()
                .AddChild(x => x
                    .Scope(SearchScope.HealthRecord)
                    .All()
                    .AddChild(y => y.SetNot(w[94]))
                    .AddChild(y => y.Check(AllCats().Except(cat[1]).ToArray())))
                .AddChild(x => x.SetNot(w[4], w[22]))
                .Search(session);

            Assert.AreEqual(0, hrs.Count());
        }

        #region AllAny_WithSingleChild_SameResults

        [TestMethod]
        public void AnyInHolder_ExcludingOnly()
        {
            // в списке нет записей с 5 или 22
            var hrs = o
            .Scope(SearchScope.Holder)
            .Any()
            .AddChild(x => x.SetNot(w[5], w[22]))
            .Search(session);

            Assert.AreEqual(7, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[1]));
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[71]));
        }

        [TestMethod]
        public void AnyInPatient()
        {
            var hrs = o
            .Scope(SearchScope.Patient)
            .Any()
            .AddChild(x => x.SetAll(w[4]))
            .Search(session);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[2]));
            Assert.IsTrue(hrs.Contains(hr[32]));
        }

        [TestMethod]
        public void AnyInHr_WithCats_FoundAllHrs()
        {
            Load<HrCategory>();
            var hrs = o
            .Scope(SearchScope.HealthRecord)
            .Any()
            .AddChild(x => x.Check(cat[2]))
            .Search(session);

            Assert.AreEqual(3, hrs.Count());
            Assert.IsTrue(hrs.Contains(hr[31]));
            Assert.IsTrue(hrs.Contains(hr[22]));
            Assert.IsTrue(hrs.Contains(hr[40]));
        }

        [TestMethod]
        public void AnyInHr_ExcludingOnly_WithCats()
        {
            // записи 2 категории без 1
            Load<HrCategory>();

            var hrs = o
            .Scope(SearchScope.HealthRecord)
            .Any()
            .AddChild(x => x
                .SetNot(w[1])
                .Check(cat[2]))
            .Search(session);

            Assert.AreEqual(2, hrs.Count());
            Assert.IsTrue(hrs.IsSuperSetOf(hr[31], hr[40]));
        }

        #endregion AllAny_WithSingleChild_SameResults

        private System.Collections.Generic.IEnumerable<HrCategory> AllCats()
        {
            return cat.Values.Union(HrCategory.Null.ToEnumerable());
        }
    }
}