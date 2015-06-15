using Diagnosis.Models;
using Diagnosis.ViewModels.Controls.Autocomplete;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class HrsSearchResultTest : ViewModelTest
    {
        SearchViewModel s;

        [TestInitialize]
        public void Init()
        {
            Load<HealthRecord>();
            Load<Appointment>();
            Load<Word>();
            s = new SearchViewModel();
        }

        [TestCleanup]
        public void Clean()
        {
            if (s != null)
                s.Dispose();
        }

        [TestMethod]
        public void RemoveDeletedHrFromResultHealthRecords()
        {
            (s.RootQueryBlock.AutocompleteAll as AutocompleteViewModel).AddTag(w[1]);
            s.SearchCommand.Execute(null);
            var a1Item = s.Result.Patients.Cast<HrsResultItemViewModel>().FindHolderKeeperOf(a[1]);
            Assert.IsTrue(a1Item.HealthRecords.Count() > 1);

            using (var tr = session.BeginTransaction())
            {
                hr[1].OnDelete();
                session.Delete(hr[1]);
                tr.Commit();
            }

            Assert.IsFalse(a1Item.FoundHealthRecords.Contains(hr[1]));
            Assert.IsFalse(a1Item.HealthRecords.Contains(hr[1]));
        }

        [TestMethod]
        public void RemoveHolderWithoutFoundHrsFromResult()
        {
            (s.RootQueryBlock.AutocompleteAll as AutocompleteViewModel).AddTag(w[4]);
            s.SearchCommand.Execute(null);
            var a1Item = s.Result.Patients.Cast<HrsResultItemViewModel>().FindHolderKeeperOf(a[1]);
            Assert.IsTrue(a1Item.FoundHealthRecords.Single() == hr[2]);

            using (var tr = session.BeginTransaction())
            {
                hr[2].OnDelete();
                session.Delete(hr[2]);
                tr.Commit();
            }

            a1Item = s.Result.Patients.Cast<HrsResultItemViewModel>().FindHolderKeeperOf(a[1]);
            Assert.IsNull(a1Item);
        }

        [TestMethod]
        public void RemoveDeletedHolderFromResult()
        {
            (s.RootQueryBlock.AutocompleteAll as AutocompleteViewModel).AddTag(w[4]);
            s.SearchCommand.Execute(null);

            var a1Item = s.Result.Patients.Cast<HrsResultItemViewModel>().FindHolderKeeperOf(a[1]);
            Assert.IsNotNull(a1Item);

            using (var tr = session.BeginTransaction())
            {
                a[1].GetAllHrs().ToList().ForEach(x =>
                    a[1].RemoveHealthRecord(x));

                a[1].Course.RemoveAppointment(a[1]);
                session.Save(a[1].Course);
                tr.Commit();
            }

            a1Item = s.Result.Patients.Cast<HrsResultItemViewModel>().FindHolderKeeperOf(a[1]);
            Assert.IsNull(a1Item);
        }
    }
}