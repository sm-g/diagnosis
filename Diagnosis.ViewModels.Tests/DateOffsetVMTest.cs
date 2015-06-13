using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class DateOffsetVMTest : ViewModelTest
    {
        private EventDateViewModel vm;
        private HealthRecord h;
        private HealthRecord emptyH;

        [TestInitialize]
        public void Init()
        {
            EventDateViewModel.ClearDict();
            h = session.Load<HealthRecord>(IntToGuid<HealthRecord>(1));
            emptyH = session.Load<HealthRecord>(IntToGuid<HealthRecord>(71));
            vm = EventDateViewModel.FromHr(h);
        }

        [TestCleanup]
        public void Clean()
        {
            EventDateViewModel.ClearDict();
            if (vm != null)
                vm.Dispose();
        }

        [TestMethod]
        public void DataConditions()
        {
            Assert.IsTrue(h.ToDate.IsEmpty);
            Assert.AreEqual(2013, h.FromDate.Year);
            Assert.AreEqual(11, h.FromDate.Month);
            Assert.AreEqual(null, h.FromDate.Day);
            Assert.AreEqual(new DateTime(2014, 11, 19), h.FromDate.Now);
            Assert.IsTrue(emptyH.GetPatient().BirthYear != null);
        }

        [TestMethod]
        public void CreateFromHr()
        {
            Assert.AreEqual(h.FromDate.Day, vm.From.Day);
            Assert.AreEqual(h.FromDate.Unit, vm.Unit);
            Assert.AreEqual(h.FromDate.Offset, vm.Offset);
            Assert.IsTrue(!vm.IsClosedInterval);
            Assert.AreEqual(EventDateViewModel.ShowAs.Date, vm.FirstSet);
        }

        [TestMethod]
        public void SetRoundedUnitBig()
        {
            vm.RoundedUnit = DateUnit.Year;
            Assert.AreEqual(1, vm.RoundedOffset);

            Assert.AreEqual(12, vm.Offset);
            Assert.AreEqual(DateUnit.Month, vm.Unit);
        }

        [TestMethod]
        public void SetRoundedUnitSmall()
        {
            vm.RoundedUnit = DateUnit.Day;
            var days = 383;
            Assert.AreEqual(days, vm.RoundedOffset);

            Assert.AreEqual(12, vm.Offset);
            Assert.AreEqual(DateUnit.Month, vm.Unit);
        }

        [TestMethod]
        public void SetToYear()
        {
            vm.to.Year = 2014;
            Assert.AreEqual(true, vm.IsClosedInterval);
            Assert.AreEqual(2014, h.ToDate.Year);
            Assert.AreEqual(null, h.ToDate.Month);
            Assert.AreEqual(null, h.ToDate.Day);
        }

        [TestMethod]
        public void SetToMonth()
        {
            vm.to.Month = 5;
            Assert.AreEqual(h.DescribedAt.Year, h.ToDate.Year); // год момента
            Assert.AreEqual(5, h.ToDate.Month);
            Assert.AreEqual(null, h.ToDate.Day);

            Assert.AreEqual(DateUnit.Month, vm.Unit);
        }

        [TestMethod]
        public void SetToSameAsFrom()
        {
            h.ToDate.FillDateFrom(h.FromDate);

            Assert.IsTrue(!vm.IsClosedInterval);
            Assert.IsTrue(!vm.IsOpenedInterval);
        }

        [TestMethod]
        public void ClosedIntervalOffsetIsDifference()
        {
            // юнит тоже меняем
            vm.to.Year = 2014;
            Assert.AreEqual(1, vm.Offset);
            Assert.AreEqual(DateUnit.Year, vm.Unit);
        }

        [TestMethod]
        public void ClosedIntervalOffsetIsDifference2()
        {
            vm.to.Year = 2014;
            vm.to.Month = 3;
            Assert.AreEqual(4, vm.Offset);
            Assert.AreEqual(DateUnit.Month, vm.Unit);
        }

        [TestMethod]
        public void ClosedIntervalOffsetFromIsFixedSide()
        {
            vm.to.Year = 2014;
            Assert.AreEqual(h.ToDate.GetSortingDate(), vm.OffsetFrom);
        }

        [TestMethod]
        public void NonClosedIntervalOffsetFromIsNow()
        {
            Assert.IsTrue(vm.IsOpenedInterval);
            Assert.AreEqual(h.FromDate.Now, vm.OffsetFrom);

            h.ToDate.FillDateFrom(h.FromDate);
            Assert.IsTrue(!vm.IsOpenedInterval && !vm.IsClosedInterval);
            Assert.AreEqual(h.FromDate.Now, vm.OffsetFrom);
        }

        [TestMethod]
        public void ClosedIntervalOffsetChangeNonFixedSide()
        {
            vm.to.Year = 2014;
            vm.Offset = 5;

            Assert.AreEqual(2009, vm.From.Year);
        }

        [TestMethod]
        public void ClosedIntervalUnitChangeNonFixedSide()
        {
            vm.to.Year = 2015;
            vm.Unit = DateUnit.Day;
            // (2015-2013) days from 2015.1.1

            Assert.AreEqual(2, vm.Offset);
            Assert.AreEqual(2014, vm.From.Year);
            Assert.AreEqual(12, vm.From.Month);
            Assert.AreEqual(30, vm.From.Day);
            Assert.AreEqual(2015, vm.to.Year);
            Assert.AreEqual(1, vm.to.Month);
            Assert.AreEqual(1, vm.to.Day);
        }

        [TestMethod]
        public void RemoveToDateRecoverFromUnitOffset()
        {
            var oldUnit = vm.Unit;
            var oldOffset = vm.Offset;

            vm.to.Year = 2014;
            vm.to.Year = null;

            Assert.AreEqual(oldUnit, vm.Unit);
            Assert.AreEqual(oldOffset, vm.Offset);
        }

        [TestMethod]
        public void NowIsShared()
        {
            vm.to.Year = 2014;

            Assert.AreEqual(h.DescribedAt, h.FromDate.Now);
            Assert.AreEqual(h.DescribedAt, h.ToDate.Now);

            // при закрытии редактора второй даты
            h.ToDate.FillDateFrom(h.FromDate);
            vm.OffsetFrom = DateTime.Now.Date;

            Assert.AreEqual(DateTime.Now.Date, h.FromDate.Now);
            Assert.AreEqual(DateTime.Now.Date, h.ToDate.Now);
        }

        [TestMethod]
        public void UnitIsShared()
        {
            vm.to.Year = 2014;

            vm.Unit = DateUnit.Month;
            Assert.AreEqual(h.FromDate.Unit, h.ToDate.Unit);
            vm.From.Day = 1;
            Assert.AreEqual(DateUnit.Day, h.FromDate.Unit);
            Assert.AreEqual(DateUnit.Month, h.ToDate.Unit);
        }

        [TestMethod]
        public void IntervalUnitChangeNonFixedSide()
        {
            vm.to.Year = 2014;
            var offset = vm.Offset;
            vm.Unit = DateUnit.Day;

            Assert.AreEqual(1, h.ToDate.Day);
            Assert.AreEqual(1, h.ToDate.Month);
            Assert.AreEqual(offset, vm.Offset);
        }

        [TestMethod]
        public void IntervalUnitChangeNonFixedSide2()
        {
            vm.to.Year = 2014;
            vm.to.Month = 3;

            var offset = vm.Offset; // 4 мес
            vm.Unit = DateUnit.Year;

            Assert.AreEqual(null, h.ToDate.Day);
            Assert.AreEqual(3, h.ToDate.Month);
            Assert.AreEqual(2014, h.ToDate.Year);
            Assert.AreEqual(null, h.FromDate.Day);
            Assert.AreEqual(null, h.FromDate.Month);
            Assert.AreEqual(2010, h.FromDate.Year);
            Assert.AreEqual(offset, vm.Offset);
        }

        [TestMethod]
        public void ClosedIntervalClearNonFixed_FixedNotEmpty()
        {
            vm.to.Year = 2014;
            vm.From.Year = null;

            Assert.AreEqual(false, vm.ToIsEmpty);
        }

        [TestMethod]
        public void EmptySetMonth()
        {
            var vm = EventDateViewModel.FromHr(emptyH);
            vm.From.Month = 5;

            Assert.AreEqual(EventDateViewModel.ShowAs.Date, vm.FirstSet);
        }

        [TestMethod]
        public void EmptySetToMonth()
        {
            var vm = EventDateViewModel.FromHr(emptyH);
            vm.to.Month = 5;

            Assert.AreEqual(EventDateViewModel.ShowAs.Date, vm.FirstSet);
        }

        [TestMethod]
        public void EmptySetUnit()
        {
            var vm = EventDateViewModel.FromHr(emptyH);
            vm.Unit = DateUnit.Month;

            Assert.IsTrue(vm.IsEmpty);
            Assert.AreEqual(null, vm.FirstSet);
        }

        [TestMethod]
        public void EmptySetOffset()
        {
            var vm = EventDateViewModel.FromHr(emptyH);
            vm.Offset = 5;

            Assert.AreEqual(EventDateViewModel.ShowAs.Offset, vm.FirstSet);
        }

        [TestMethod]
        public void EmptySetAge()
        {
            var vm = EventDateViewModel.FromHr(emptyH);
            vm.AtAge = 5;

            Assert.AreEqual(EventDateViewModel.ShowAs.AtAge, vm.FirstSet);
        }

        [TestMethod]
        public void ClearAtAge()
        {
            vm.AtAge = null;

            Assert.AreEqual(true, h.FromDate.IsEmpty);
            Assert.AreEqual(null, vm.From.Year);
        }

        [TestMethod]
        public void ClearFromYear()
        {
            vm.From.Year = null;

            Assert.AreEqual(true, h.FromDate.IsEmpty);
        }
    }
}