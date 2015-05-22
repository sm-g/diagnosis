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
            Assert.AreEqual(1, s.QueryEditor.QueryBlocks.Count);
            Assert.IsTrue(s.QueryEditor.AllEmpty);
        }

        [TestMethod]
        public void CannotSearchWithoutOptions()
        {
            s.QueryEditor.QueryBlocks.Clear();
            Assert.IsTrue(s.QueryEditor.AllEmpty);
            Assert.IsFalse(s.SearchCommand.CanExecute(null));
        }

        [TestMethod]
        public void NotUsedWords()
        {
            (s.RootQueryBlock.AutocompleteAll as AutocompleteViewModel).AddTag(w[6]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(true, s.NothingFound);
            Assert.AreEqual(0, s.Result.Patients.Count);
        }

        [TestMethod]
        public void TwoPatients()
        {
            (s.RootQueryBlock.AutocompleteAll as AutocompleteViewModel).AddTag(w[22]);
            s.SearchCommand.Execute(null);

            Assert.AreEqual(2, s.Result.Patients.Count);
        }

        [TestMethod]
        public void WordsInApp()
        {
            s.UseOldMode = true;
            (s.RootQueryBlock.AutocompleteAll as AutocompleteViewModel).AddTag(w[1]);
            (s.RootQueryBlock.AutocompleteAll as AutocompleteViewModel).AddTag(w[4]);
            s.RootQueryBlock.QueryScope = HealthRecordQueryAndScope.Appointment;
            s.SearchCommand.Execute(null);

            Assert.AreEqual(1, (s.Result.Statistic as HrsStatistic).Patients.Count);
            Assert.AreEqual(a[1], (s.Result.Patients[0] as HrsResultItemViewModel).Children[0].Children[0].Holder);

            Assert.AreEqual(2, (s.Result.Statistic as HrsStatistic).HealthRecords.Count);
            Assert.IsTrue(s.Contains(hr[1]));
            Assert.IsTrue(s.Contains(hr[2]));
        }

       
        [TestMethod]
        public void FoundHrs()
        {
            s.UseOldMode = true;
            (s.RootQueryBlock.AutocompleteAll as AutocompleteViewModel).AddTag(w[4]);
            (s.RootQueryBlock.AutocompleteAll as AutocompleteViewModel).AddTag(w[22]);
            s.RootQueryBlock.QueryScope = HealthRecordQueryAndScope.Course;
            s.SearchCommand.Execute(null);

            Assert.AreEqual(0, (s.Result.Patients[0] as HrsResultItemViewModel).FoundHealthRecords.Count); // найденные — только слова в области
            Assert.AreEqual(1, (s.Result.Patients[0] as HrsResultItemViewModel).Children[0].FoundHealthRecords.Count); // 7-14
            Assert.AreEqual(1, (s.Result.Patients[0] as HrsResultItemViewModel).Children[0].Children[0].FoundHealthRecords.Count);
            Assert.AreEqual(1, (s.Result.Patients[0] as HrsResultItemViewModel).Children[0].Children[1].FoundHealthRecords.Count);
        }

        [TestMethod]
        public void AppOrder()
        {
            s.UseOldMode = true;
            (s.RootQueryBlock.AutocompleteAll as AutocompleteViewModel).AddTag(w[4]);
            (s.RootQueryBlock.AutocompleteAll as AutocompleteViewModel).AddTag(w[22]);
            s.RootQueryBlock.QueryScope = HealthRecordQueryAndScope.Course;
            s.SearchCommand.Execute(null);

            Assert.AreEqual(0, (s.Result.Patients[0] as HrsResultItemViewModel).FoundHealthRecords.Count);
            Assert.AreEqual(1, (s.Result.Patients[0] as HrsResultItemViewModel).Children[0].FoundHealthRecords.Count); // 7-14
            Assert.AreEqual(2, (s.Result.Patients[0] as HrsResultItemViewModel).Children[0].Children[0].HealthRecords.Count); // 14
        }
        [TestMethod]
        public void ReceiverIsNullBeforeSend()
        {
            Assert.IsTrue(s.LastRecieverQueryBlock == null);
        }
        [TestMethod]
        public void RecieveChios()
        {
            var chios = new[] { w[1].AsConfidencable(), w[2].AsConfidencable(Confidence.Absent) };
            this.Send(Event.SendToSearch, chios.AsParams(MessageKeys.ToSearchPackage));

            Assert.IsTrue(s.LastRecieverQueryBlock.Options.CWordsAll.ScrambledEquals(chios));
        }
        [TestMethod]
        public void RecieveHr()
        {
            var hrs = new[] { hr[70] };
            this.Send(Event.SendToSearch, hrs.AsParams(MessageKeys.ToSearchPackage));

            Assert.IsTrue(s.LastRecieverQueryBlock.Options.CWordsAll.ScrambledEquals(hr[70].GetCWords()));
            Assert.IsTrue(s.LastRecieverQueryBlock.Options.Categories.Single() == hr[70].Category);

        }
        [TestMethod]
        public void RecieveHrs()
        {
            var hrs = new[] { hr[1], hr[2] };
            this.Send(Event.SendToSearch, hrs.AsParams(MessageKeys.ToSearchPackage));

            var cats = hrs.Select(x => x.Category).Distinct();
            var words = hrs.SelectMany(x => x.Words).Distinct();

            Assert.IsTrue(s.LastRecieverQueryBlock.Options.WordsAll.ScrambledEquals(words));
            Assert.IsTrue(s.LastRecieverQueryBlock.Options.Categories.ScrambledEquals(cats));

        }
        [TestMethod]
        public void RecieveMeasure()
        {
            var hrs = new[] { hr[20] };
            this.Send(Event.SendToSearch, hrs.AsParams(MessageKeys.ToSearchPackage));

            var ms = hrs.SelectMany(x => x.Measures).Distinct();

            Assert.IsTrue(s.LastRecieverQueryBlock.Options.MeasuresAll.Select(x => x.AsMeasure()).ScrambledEquals(ms));

        }

        [TestMethod]
        public void RecieveOptions_DoSearch()
        {
            var options = new SearchOptions(true);
            options.MinAny = 5;
            this.Send(Event.SendToSearch, options.AsParams(MessageKeys.ToSearchPackage));

            Assert.AreEqual(options, s.LastRecieverQueryBlock.Options);
            Assert.IsTrue(s.Result != null);
            Assert.IsTrue(!s.ControlsVisible);

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
            qb.MinAny = min;
            return qb;
        }

        public static QueryBlockViewModel Check(this QueryBlockViewModel qb, params HrCategory[] cats)
        {
            qb.SelectCategory(cats);
            return qb;
        }

        public static bool Contains(this SearchViewModel s, params HealthRecord[] hrs)
        {
            return hrs.All(x => (s.Result.Statistic as HrsStatistic).HealthRecords.Contains(x));
        }

        public static bool NotContains(this SearchViewModel s, params HealthRecord[] hrs)
        {
            return hrs.All(x => !(s.Result.Statistic as HrsStatistic).HealthRecords.Contains(x));
        }
    }
}