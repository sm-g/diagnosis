using Diagnosis.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Tests
{
    [TestClass]
    public class DateOffsetTest
    {
        private static DateOffset date;
        private static Func<DateTime> now = () => new DateTime(2014, 4, 1);

        private const int offset = 5;

        #region constructors

        [TestMethod]
        public void TestConstructorOffsetDay()
        {
            date = new DateOffset(offset, DateUnits.Day);

            var now = DateTime.Today;
            var d = now.AddDays(-offset);

            Assert.IsTrue(date.Offset == offset);
            Assert.IsTrue(date.Unit == DateUnits.Day);
            Assert.IsTrue(date.Year == d.Year);
            Assert.IsTrue(date.Month == d.Month);
            Assert.IsTrue(date.Day == d.Day);
        }

        [TestMethod]
        public void TestConstructorOffsetWeek()
        {
            date = new DateOffset(offset, DateUnits.Week);

            var now = DateTime.Today;
            var d = now.AddDays(-offset * 7);

            Assert.IsTrue(date.Offset == offset);
            Assert.IsTrue(date.Unit == DateUnits.Week);
            Assert.IsTrue(date.Year == d.Year);
            Assert.IsTrue(date.Month == d.Month);
            Assert.IsTrue(date.Day == d.Day);
        }

        [TestMethod]
        public void TestConstructorOffsetMonth()
        {
            date = new DateOffset(offset, DateUnits.Month);

            var now = DateTime.Today;
            var d = now.AddMonths(-offset);

            Assert.IsTrue(date.Offset == offset);
            Assert.IsTrue(date.Unit == DateUnits.Month);
            Assert.IsTrue(date.Year == d.Year);
            Assert.IsTrue(date.Month == d.Month);
            Assert.IsTrue(date.Day == null);
        }

        [TestMethod]
        public void TestConstructorOffsetYear()
        {
            date = new DateOffset(offset, DateUnits.Year);

            var now = DateTime.Today;
            var d = now.AddYears(-offset);

            Assert.IsTrue(date.Offset == offset);
            Assert.IsTrue(date.Unit == DateUnits.Year);
            Assert.IsTrue(date.Year == d.Year);
            Assert.IsTrue(date.Month == null);
            Assert.IsTrue(date.Day == null);
        }

        [TestMethod]
        public void TestConstructorOffsetNull()
        {
            date = new DateOffset(null, DateUnits.Day);

            var now = DateTime.Today;

            Assert.IsTrue(date.Offset == null);
            Assert.IsTrue(date.Unit == DateUnits.Day);
            Assert.IsTrue(date.Year == null);
            Assert.IsTrue(date.Month == null);
            Assert.IsTrue(date.Day == null);
        }

        [TestMethod]
        public void TestConstructorDateTime()
        {
            var now = DateTime.Today;

            date = new DateOffset(now);

            Assert.IsTrue(date.Offset == 0);
            Assert.IsTrue(date.Unit == DateUnits.Day);
            Assert.IsTrue(date.Year == now.Year);
            Assert.IsTrue(date.Month == now.Month);
            Assert.IsTrue(date.Day == now.Day);
        }

        [TestMethod]
        public void TestConstructorDateFull()
        {
            var now = DateTime.Today;

            date = new DateOffset(now.Year, now.Month, now.Day);

            Assert.IsTrue(date.Offset == 0);
            Assert.IsTrue(date.Unit == DateUnits.Day);
            Assert.IsTrue(date.Year == now.Year);
            Assert.IsTrue(date.Month == now.Month);
            Assert.IsTrue(date.Day == now.Day);
        }

        [TestMethod]
        public void TestConstructorDateYear()
        {
            var now = DateTime.Today;

            date = new DateOffset(now.Year, null, null);

            Assert.IsTrue(date.Offset == 0);
            Assert.IsTrue(date.Unit == DateUnits.Year);
            Assert.IsTrue(date.Year == now.Year);
            Assert.IsTrue(date.Month == null);
            Assert.IsTrue(date.Day == null);
        }

        [TestMethod]
        public void TestConstructorDateYearMonth()
        {
            var now = DateTime.Today;

            date = new DateOffset(now.Year, now.Month, null);

            Assert.IsTrue(date.Offset == 0);
            Assert.IsTrue(date.Unit == DateUnits.Month);
            Assert.IsTrue(date.Year == now.Year);
            Assert.IsTrue(date.Month == now.Month);
            Assert.IsTrue(date.Day == null);
        }

        [TestMethod]
        public void TestConstructorDateYearDay()
        {
            var now = DateTime.Today;

            date = new DateOffset(now.Year, null, now.Day);

            Assert.IsTrue(date.Offset == 0);
            Assert.IsTrue(date.Unit == DateUnits.Year);
            Assert.IsTrue(date.Year == now.Year);
            Assert.IsTrue(date.Month == null);
            Assert.IsTrue(date.Day == null);
        }

        [TestMethod]
        public void TestConstructorDateMonth()
        {
            var now = DateTime.Today;

            date = new DateOffset(null, now.Month, null);

            Assert.IsTrue(date.Offset == 0);
            Assert.IsTrue(date.Unit == DateUnits.Month);
            Assert.IsTrue(date.Year == now.Year);
            Assert.IsTrue(date.Month == now.Month);
            Assert.IsTrue(date.Day == null);
        }

        [TestMethod]
        public void TestConstructorDateMonthDay()
        {
            var now = DateTime.Today;

            date = new DateOffset(null, now.Month, now.Day);

            Assert.IsTrue(date.Offset == 0);
            Assert.IsTrue(date.Unit == DateUnits.Day);
            Assert.IsTrue(date.Year == now.Year);
            Assert.IsTrue(date.Month == now.Month);
            Assert.IsTrue(date.Day == now.Day);
        }

        [TestMethod]
        public void TestConstructorDateDay()
        {
            var now = DateOffsetTest.now();

            date = new DateOffset(null, null, now.Day + 1, DateOffsetTest.now);

            Assert.IsTrue(date.Offset == -1);
            Assert.IsTrue(date.Unit == DateUnits.Day);
            Assert.IsTrue(date.Year == now.Year);
            Assert.IsTrue(date.Month == now.Month);
            Assert.IsTrue(date.Day == now.Day + 1);
        }

        [TestMethod]
        public void TestConstructorDateNone()
        {
            date = new DateOffset(null, null, null);
            Assert.IsNull(date.Offset);
            Assert.IsNull(date.Year);
            Assert.IsNull(date.Month);
            Assert.IsNull(date.Day);
        }

        [TestMethod]
        public void TestConstructorDepthDays()
        {
            var now = DateOffsetTest.now();

            date = new DateOffset(now.Year, 3, 20, DateOffsetTest.now); // 10 дней назад
            Assert.IsTrue(date.Unit == DateUnits.Day);
        }

        [TestMethod]
        public void TestConstructorWorngDate()
        {
            date = new DateOffset(2009, 2, 30);
            Assert.IsTrue(date.Day == 28); // autocorrection default == true
        }

        #endregion constructors

        #region Setters

        [TestMethod]
        public void TestSetDay()
        {
            date = new DateOffset(offset, DateUnits.Day);
            var now = DateTime.Today;
            date.Day = now.Day;

            Assert.IsTrue(date.Offset == 0);
            Assert.IsTrue(date.Unit == DateUnits.Day);
        }

        [TestMethod]
        public void TestSetMonth()
        {
            date = new DateOffset(offset, DateUnits.Day);
            var d = DateTime.Today.AddMonths(-1);
            date.Month = d.Month;

            Assert.IsTrue(date.Offset == DateTime.DaysInMonth(
                DateTime.Today.Year, d.Month) + offset);
            Assert.IsTrue(date.Unit == DateUnits.Day);
        }

        [TestMethod]
        public void TestSetMonthNull()
        {
            date = new DateOffset(offset, DateUnits.Day);
            date.Month = null;

            Assert.IsTrue(date.Offset == 0);
            Assert.IsTrue(date.Unit == DateUnits.Year);
        }

        [TestMethod]
        public void TestSetUnitWider()
        {
            date = new DateOffset(offset, DateUnits.Day, now);
            date.CutsDate = true;
            date.Unit = DateUnits.Month;

            Assert.IsTrue(date.Offset == offset);
            Assert.IsTrue(date.Unit == DateUnits.Month);
            Assert.IsTrue(date.Year == 2013);
            Assert.IsTrue(date.Month == 11);
            Assert.IsTrue(date.Day == null);
        }

        [TestMethod]
        public void TestSetUnitWiderWeek()
        {
            date = new DateOffset(offset, DateUnits.Day, now);

            date.Unit = DateUnits.Week;

            Assert.IsTrue(date.Offset == offset);
            Assert.IsTrue(date.Unit == DateUnits.Week);
            Assert.IsTrue(date.Year == 2014);
            Assert.IsTrue(date.Month == 2);
            Assert.IsTrue(date.Day == 25);
        }

        [TestMethod]
        public void TestSetUnitNarrower()
        {
            date = new DateOffset(offset, DateUnits.Year, now);

            date.Unit = DateUnits.Month;

            Assert.IsTrue(date.Offset == offset);
            Assert.IsTrue(date.Unit == DateUnits.Month);
            Assert.IsTrue(date.Year == 2013);
            Assert.IsTrue(date.Month == 11);
            Assert.IsTrue(date.Day == null);
        }

        [TestMethod]
        public void TestSetMonth2()
        {
            date = new DateOffset(1, DateUnits.Year, now);

            date.Month = 4;

            Assert.IsTrue(date.Offset == 12);
            Assert.IsTrue(date.Unit == DateUnits.Month);
            Assert.IsTrue(date.Year == 2013);
            Assert.IsTrue(date.Month == 4);
            Assert.IsTrue(date.Day == null);
        }

        #endregion Setters

        #region compare

        [TestMethod]
        public void TestLtDayMonth()
        {
            var date1 = new DateOffset(40, DateUnits.Day);
            var date2 = new DateOffset(0, DateUnits.Month);

            Assert.IsTrue(date1 < date2);
        }

        [TestMethod]
        public void TestGtDayYear()
        {
            var date1 = new DateOffset(40, DateUnits.Day);
            var date2 = new DateOffset(1, DateUnits.Year);

            Assert.IsTrue(date1 > date2);
        }

        [TestMethod]
        public void TestLtMonthYear()
        {
            var date1 = new DateOffset(40, DateUnits.Month);
            var date2 = new DateOffset(2, DateUnits.Year);

            Assert.IsTrue(date1 < date2);
        }

        [TestMethod]
        public void TestLtSameUnit()
        {
            var date1 = new DateOffset(40, DateUnits.Day);
            var date2 = new DateOffset(0, DateUnits.Day);

            Assert.IsTrue(date1 < date2);
        }

        [TestMethod]
        public void TestLtSameUnit2()
        {
            var date1 = new DateOffset(2013, 12, null);
            var date2 = new DateOffset(2014, 02, 0);

            Assert.IsTrue(date1 < date2);
        }

        [TestMethod]
        public void TestLtGtNull()
        {
            var date1 = new DateOffset(null, DateUnits.Month);
            var date2 = new DateOffset(5, DateUnits.Year);

            Assert.IsFalse(date1 < date2);
            Assert.IsFalse(date1 > date2);
        }

        [TestMethod]
        public void TestLtOrEqual()
        {
            var date1 = new DateOffset(2013, 12, null);
            var date2 = new DateOffset(2014, 02, 0);

            Assert.IsTrue(date1 <= date2);
        }

        [TestMethod]
        public void TestEqual()
        {
            var date1 = new DateOffset(2013, 12, 1);
            var date2 = new DateOffset(2013, 12, 1);

            Assert.IsTrue(date1 == date2);
        }

        [TestMethod]
        public void TestEqualSameYear()
        {
            var date1 = new DateOffset(3, DateUnits.Day);
            var date2 = new DateOffset(0, DateUnits.Year);

            Assert.IsTrue(date1 <= date2);
            Assert.IsTrue(date1 >= date2);
        }

        [TestMethod]
        public void TestLtSameMonth()
        {
            var date1 = new DateOffset(2013, 5, 5);
            var date2 = new DateOffset(2014, 5, 1);

            Assert.IsTrue(date1 < date2);
        }

        [TestMethod]
        public void TestCompareWithWeek()
        {
            var date1 = new DateOffset(now(), now);
            var date2 = new DateOffset(2, DateUnits.Week, now);

            Assert.IsTrue(date1 > date2);
        }

        #endregion compare

        #region Add

        [TestMethod]
        public void AddYear()
        {
            var date = new DateOffset(2010, 2, null);
            date.Add(-2, DateUnits.Year);

            Assert.IsTrue(date.Year == 2008);
            Assert.IsTrue(date.Month == 2);
            Assert.IsTrue(date.Day == null);
        }
        [TestMethod]
        public void AddYear29Feb()
        {
            var date = new DateOffset(2012, 2, 29);
            date.Add(1, DateUnits.Year);

            Assert.IsTrue(date.Year == 2013);
            Assert.IsTrue(date.Month == 2);
            Assert.IsTrue(date.Day == 28);
        }

        [TestMethod]
        public void AddMonth()
        {
            var date = new DateOffset(2012, 2, 29);
            date.Add(1, DateUnits.Month);

            Assert.IsTrue(date.Year == 2012);
            Assert.IsTrue(date.Month == 3);
            Assert.IsTrue(date.Day == 29);
        }

        [TestMethod]
        public void AddWeek()
        {
            var date = new DateOffset(2012, 1, 1);
            date.Add(5, DateUnits.Week);

            Assert.IsTrue(date.Year == 2012);
            Assert.IsTrue(date.Month == 2);
            Assert.IsTrue(date.Day == 5);
        }

        [TestMethod]
        public void AddDay()
        {
            var date = new DateOffset(2012, 2, 29);
            date.Add(-68, DateUnits.Day);

            Assert.IsTrue(date.Year == 2011);
            Assert.IsTrue(date.Month == 12);
            Assert.IsTrue(date.Day == 23);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddWrong()
        {
            var date = new DateOffset(2012, null, null);
            date.Add(-68, DateUnits.Day);
        }
        #endregion

        #region Setting
        [TestMethod]
        public void UnitSettingSetsDate()
        {
            var date = new DateOffset(2014, 3, 31, now);
            Assert.IsTrue(date.Offset == 1);
            date.UnitSettingStrategy = DateOffset.UnitSetting.SetsDate;
            date.CutsDate = false;

            date.Unit = DateUnits.Year;
            Assert.IsTrue(date.Year == 2013);
            Assert.IsTrue(date.Month == 3);
            Assert.IsTrue(date.Day == 31);
        }
        [TestMethod]
        public void UnitSettingSetsAndCutsDate()
        {
            var date = new DateOffset(2014, 3, 31, now);
            Assert.IsTrue(date.Offset == 1);
            date.UnitSettingStrategy = DateOffset.UnitSetting.SetsDate;
            date.CutsDate = true;

            date.Unit = DateUnits.Year;
            Assert.IsTrue(date.Year == 2013);
            Assert.IsTrue(date.Month == null);
            Assert.IsTrue(date.Day == null);
        }
        [TestMethod]
        public void NoCutsDate()
        {
            var date = new DateOffset(2014, 3, 31, now);
            date.CutsDate = false;
            date.Month = null;
            Assert.IsTrue(date.Year == 2014);
            Assert.IsTrue(date.Month == null);
            Assert.IsTrue(date.Day == 31);
        }
        [TestMethod]
        public void UnitSettingRoundsOffset()
        {
            var date = new DateOffset(2014, 3, 31, now);
            Assert.IsTrue(date.Offset == 1);
            date.UnitSettingStrategy = DateOffset.UnitSetting.RoundsOffset;
            date.CutsDate = false;

            date.Unit = DateUnits.Year;
            Assert.IsTrue(date.Offset == 0);
            Assert.IsTrue(date.Year == 2014);
            Assert.IsTrue(date.Month == 3);
            Assert.IsTrue(date.Day == 31);
        }

        [TestMethod]
        public void OffsetSettingSetsDateWhenUnitSettingRoundsOffset()
        {
            var date = new DateOffset(now(), now, new DateOffset.DateOffsetSettings(
                DateOffset.UnitSetting.RoundsOffset, DateOffset.DateSetting.RoundsUnit)); // или DateSetting.SavesUnit

            date.Offset = 5;
            Assert.IsTrue(date.Year == 2014);
            Assert.IsTrue(date.Month == 3);
            Assert.IsTrue(date.Day == 27);
        }

        [TestMethod]
        public void DateSettingRoundsUnit()
        {
            var date = new DateOffset(5, DateUnits.Day, now);
            Assert.IsTrue(date.Offset == 5);
            date.DateSettingStrategy = DateOffset.DateSetting.RoundsUnit;

            date.Year = 2013;
            Assert.IsTrue(date.Offset == 1);
            Assert.IsTrue(date.Unit == DateUnits.Year);
            Assert.IsTrue(date.Month == 3);
            Assert.IsTrue(date.Day == 27);
        }
        [TestMethod]
        public void DateSettingSavesUnit()
        {
            var date = new DateOffset(5, DateUnits.Day, now, new DateOffset.DateOffsetSettings(
                DateOffset.UnitSetting.RoundsOffset, DateOffset.DateSetting.SavesUnit)); // только так UnitSetting.RoundsOffset

            // юнит подстраивается
            date.Year = 2013;
            Assert.IsTrue(date.Offset == 1);
            Assert.IsTrue(date.Unit == DateUnits.Year);
            Assert.IsTrue(date.Month == 3);
            Assert.IsTrue(date.Day == 27);
            // юнит сохраняется
            date.Unit = DateUnits.Month;
            Assert.IsTrue(date.UnitFixed);
            Assert.IsTrue(date.Offset == 13);
            Assert.IsTrue(date.Year == 2013);
            Assert.IsTrue(date.Month == 3);
            Assert.IsTrue(date.Day == 27);

            // юнит не меняется
            date.Year = 2014;
            Assert.IsTrue(date.Unit == DateUnits.Month);
            Assert.IsTrue(date.Offset == 13);
        }
        #endregion

        [TestMethod]
        public void GetTotalMonths()
        {
            var date = new DateOffset(5, DateUnits.Day, now);
            Assert.AreEqual(1, date.GetTotalMonths()); // 31 и 1 другого месяца отличаются на месяц            
        }
    }
}