using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.Tests.ViewModels
{
    [TestClass]
    public class DateOffsetVMTest : ViewModelTest
    {
        DateOffsetViewModel vm;
        HealthRecord h;
        DateTime now;
        [TestInitialize]
        public void Init()
        {
            Load<HealthRecord>();
            h = hr[1];
            h.FromDate.Now = h.CreatedAt; // 
            now = h.FromDate.Now;
            vm = DateOffsetViewModel.FromHr(hr[1]);
        }
        [TestCleanup]
        public void Clean()
        {
            vm.Dispose();
        }

        [TestMethod]
        public void DataConditions()
        {
            Assert.AreEqual(2013, h.FromDate.Year);
            Assert.AreEqual(11, h.FromDate.Month);
            Assert.AreEqual(null, h.FromDate.Day);
            Assert.AreEqual(new DateTime(2014, 11, 19), h.FromDate.Now);


        }
        [TestMethod]
        public void CreateFromHr()
        {
            Assert.AreEqual(h.FromDate.Day, vm.Day);
            Assert.AreEqual(h.FromDate.Unit, vm.Unit);
            Assert.AreEqual(h.FromDate.Offset, vm.Offset);
            Assert.IsTrue(!vm.IsClosedInterval);

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
            vm.ToYear = 2014;
            Assert.AreEqual(true, vm.IsClosedInterval);
            Assert.AreEqual(2014, h.ToDate.Year);
            Assert.AreEqual(null, h.ToDate.Month);
            Assert.AreEqual(null, h.ToDate.Day);
        }
        [TestMethod]
        public void SetToMonth()
        {
            vm.ToMonth = 5;
            Assert.AreEqual(h.DescribedAt.Year, h.ToDate.Year); // год момента
            Assert.AreEqual(5, h.ToDate.Month);
            Assert.AreEqual(null, h.ToDate.Day);

            Assert.AreEqual(DateUnit.Month, vm.Unit);
        }

        [TestMethod]
        public void ClosedIntervalOffsetIsDifference()
        {
            // юнит тоже меняем
            vm.ToYear = 2014;
            Assert.AreEqual(1, vm.Offset);
            Assert.AreEqual(DateUnit.Year, vm.Unit);
        }
        [TestMethod]
        public void ClosedIntervalOffsetIsDifference2()
        {
            vm.ToYear = 2014;
            vm.ToMonth = 3;
            Assert.AreEqual(4, vm.Offset);
            Assert.AreEqual(DateUnit.Month, vm.Unit);
        }
        [TestMethod]
        public void ClosedIntervalOffsetFromIsFixedSide()
        {
            vm.ToYear = 2014;
            Assert.AreEqual(h.ToDate.GetSortingDate(), vm.OffsetFrom);
        }

        [TestMethod]
        public void NonClosedIntervalOffsetFromIsNow()
        {
            vm.IsInterval = false;
            Assert.AreEqual(h.FromDate.Now, vm.OffsetFrom);

            vm.IsInterval = true;
            Assert.AreEqual(h.FromDate.Now, vm.OffsetFrom);
        }


        [TestMethod]
        public void ClosedIntervalOffsetChangeNonFixedSide()
        {
            vm.ToYear = 2014;
            vm.Offset = 5;

            Assert.AreEqual(2009, vm.Year);
        }
        [TestMethod]
        public void ClosedIntervalUnitChangeNonFixedSide()
        {
            vm.ToYear = 2015;
            vm.Unit = DateUnit.Day;
            // (2015-2013) days from 2015.1.1

            Assert.AreEqual(2, vm.Offset);
            Assert.AreEqual(2014, vm.Year);
            Assert.AreEqual(12, vm.Month);
            Assert.AreEqual(30, vm.Day);
            Assert.AreEqual(2015, vm.ToYear);
            Assert.AreEqual(1, vm.ToMonth);
            Assert.AreEqual(1, vm.ToDay);
        }
        [TestMethod]
        public void RemoveToDateRecoverFromUnitOffset()
        {
            var oldUnit = vm.Unit;
            var oldOffset = vm.Offset;

            vm.ToYear = 2014;
            vm.ToYear = null;

            Assert.AreEqual(oldUnit, vm.Unit);
            Assert.AreEqual(oldOffset, vm.Offset);
        }

        [TestMethod]
        public void NowIsShared()
        {
            vm.ToYear = 2014;

            Assert.AreEqual(h.DescribedAt, h.FromDate.Now);
            Assert.AreEqual(h.DescribedAt, h.ToDate.Now);

            vm.IsInterval = false;
            vm.OffsetFrom = DateTime.Now.Date;

            Assert.AreEqual(DateTime.Now.Date, h.FromDate.Now);
            Assert.AreEqual(DateTime.Now.Date, h.ToDate.Now);
        }
        [TestMethod]
        public void UnitIsShared()
        {
            vm.ToYear = 2014;

            vm.Unit = DateUnit.Month;
            Assert.AreEqual(h.FromDate.Unit, h.ToDate.Unit);
            vm.Day = 1;
            Assert.AreEqual(DateUnit.Day, h.FromDate.Unit);
            Assert.AreEqual(DateUnit.Month, h.ToDate.Unit);

        }

        [TestMethod]
        public void IntervalUnitChangeNonFixedSide()
        {
            vm.ToYear = 2014;
            var offset = vm.Offset;
            vm.Unit = DateUnit.Day;

            Assert.AreEqual(1, h.ToDate.Day);
            Assert.AreEqual(1, h.ToDate.Month);
            Assert.AreEqual(offset, vm.Offset);
        }
        [TestMethod]
        public void IntervalUnitChangeNonFixedSide2()
        {
            vm.ToYear = 2014;
            vm.ToMonth = 3;

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
        public void HideIntervalEditor()
        {
            vm.ToYear = 2015;
            vm.IsInterval = false;
            // скрыт редактор второй - дата осатется пока запись не сохранена
            Assert.IsFalse(h.ToDate.IsEmpty);

        }
    }
}
