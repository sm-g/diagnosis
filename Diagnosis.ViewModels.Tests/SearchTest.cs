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
            qb.GroupOperator = QueryGroupOperator.All;
            return qb;
        }

        public static QueryBlockViewModel Any(this QueryBlockViewModel qb)
        {
            qb.GroupOperator = QueryGroupOperator.Any;
            return qb;
        }
        public static QueryBlockViewModel NotAny(this QueryBlockViewModel qb)
        {
            qb.GroupOperator = QueryGroupOperator.NotAny;
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