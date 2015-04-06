using Diagnosis.Common;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Diagnosis.Tests.Model
{
    [TestClass]
    public class DateOffsetTest
    {
        private static DateOffset date;
        private static Func<DateTime> getNow = () => new DateTime(2014, 4, 1);

        private const int offset = 5;
        private DateTime now;

        [TestInitialize]
        public void TestInit()
        {
            now = DateTime.Today;
        }

        #region constructors

        [TestMethod]
        public void TestConstructorOffsetDay()
        {
            date = new DateOffset(offset, DateUnit.Day);

            var d = now.AddDays(-offset);

            Assert.IsTrue(date.Offset == offset);
            Assert.IsTrue(date.Unit == DateUnit.Day);
            Assert.IsTrue(date.Year == d.Year);
            Assert.IsTrue(date.Month == d.Month);
            Assert.IsTrue(date.Day == d.Day);
        }

        [TestMethod]
        public void TestConstructorOffsetWeek()
        {
            date = new DateOffset(offset, DateUnit.Week);

            var d = now.AddDays(-offset * 7);

            Assert.IsTrue(date.Offset == offset);
            Assert.IsTrue(date.Unit == DateUnit.Week);
            Assert.IsTrue(date.Year == d.Year);
            Assert.IsTrue(date.Month == d.Month);
            Assert.IsTrue(date.Day == d.Day);
        }

        [TestMethod]
        public void TestConstructorOffsetMonth()
        {
            date = new DateOffset(offset, DateUnit.Month);

            var d = now.AddMonths(-offset);

            Assert.IsTrue(date.Offset == offset);
            Assert.IsTrue(date.Unit == DateUnit.Month);
            Assert.IsTrue(date.Year == d.Year);
            Assert.IsTrue(date.Month == d.Month);
            Assert.IsTrue(date.Day == null);
        }

        [TestMethod]
        public void TestConstructorOffsetYear()
        {
            date = new DateOffset(offset, DateUnit.Year);

            var d = now.AddYears(-offset);

            Assert.IsTrue(date.Offset == offset);
            Assert.IsTrue(date.Unit == DateUnit.Year);
            Assert.IsTrue(date.Year == d.Year);
            Assert.IsTrue(date.Month == null);
            Assert.IsTrue(date.Day == null);
        }

        [TestMethod]
        public void TestConstructorOffsetNull()
        {
            date = new DateOffset(null, DateUnit.Day);

            Assert.IsTrue(date.Offset == null);
            Assert.IsTrue(date.Unit == DateUnit.Day);
            Assert.IsTrue(date.Year == null);
            Assert.IsTrue(date.Month == null);
            Assert.IsTrue(date.Day == null);
        }

        [TestMethod]
        public void TestConstructorDateTime()
        {
            date = new DateOffset(now);

            Assert.IsTrue(date.Offset == 0);
            Assert.IsTrue(date.Unit == DateUnit.Day);
            Assert.IsTrue(date.Year == now.Year);
            Assert.IsTrue(date.Month == now.Month);
            Assert.IsTrue(date.Day == now.Day);
        }

        [TestMethod]
        public void TestConstructorDateFull()
        {
            date = new DateOffset(now.Year, now.Month, now.Day);

            Assert.IsTrue(date.Offset == 0);
            Assert.IsTrue(date.Unit == DateUnit.Day);
            Assert.IsTrue(date.Year == now.Year);
            Assert.IsTrue(date.Month == now.Month);
            Assert.IsTrue(date.Day == now.Day);
        }

        [TestMethod]
        public void TestConstructorDateYear()
        {
            date = new DateOffset(now.Year, null, null);

            Assert.IsTrue(date.Offset == 0);
            Assert.IsTrue(date.Unit == DateUnit.Year);
            Assert.IsTrue(date.Year == now.Year);
            Assert.IsTrue(date.Month == null);
            Assert.IsTrue(date.Day == null);
        }

        [TestMethod]
        public void TestConstructorDateYearMonth()
        {
            date = new DateOffset(now.Year, now.Month, null);

            Assert.IsTrue(date.Offset == 0);
            Assert.IsTrue(date.Unit == DateUnit.Month);
            Assert.IsTrue(date.Year == now.Year);
            Assert.IsTrue(date.Month == now.Month);
            Assert.IsTrue(date.Day == null);
        }

        [TestMethod]
        public void TestConstructorDateYearDay()
        {
            date = new DateOffset(now.Year, null, now.Day);

            Assert.IsTrue(date.Offset == 0);
            Assert.IsTrue(date.Unit == DateUnit.Year);
            Assert.IsTrue(date.Year == now.Year);
            Assert.IsTrue(date.Month == null);
            Assert.IsTrue(date.Day == null);
        }

        [TestMethod]
        public void TestConstructorDateMonth()
        {
            date = new DateOffset(null, now.Month, null);

            Assert.IsTrue(date.Offset == 0);
            Assert.IsTrue(date.Unit == DateUnit.Month);
            Assert.IsTrue(date.Year == now.Year);
            Assert.IsTrue(date.Month == now.Month);
            Assert.IsTrue(date.Day == null);
        }

        [TestMethod]
        public void TestConstructorDateMonthDay()
        {
            date = new DateOffset(null, now.Month, now.Day);

            Assert.IsTrue(date.Offset == 0);
            Assert.IsTrue(date.Unit == DateUnit.Day);
            Assert.IsTrue(date.Year == now.Year);
            Assert.IsTrue(date.Month == now.Month);
            Assert.IsTrue(date.Day == now.Day);
        }

        [TestMethod]
        public void TestConstructorDateDay()
        {
            var now = DateOffsetTest.getNow();

            date = new DateOffset(null, null, now.Day + 1, getNow);

            Assert.IsTrue(date.Offset == -1);
            Assert.IsTrue(date.Unit == DateUnit.Day);
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
            var now = DateOffsetTest.getNow();

            date = new DateOffset(now.Year, 3, 20, getNow); // 10 дней назад
            Assert.IsTrue(date.Unit == DateUnit.Day);
        }

        [TestMethod]
        public void TestConstructorWorngDate()
        {
            date = new DateOffset(2009, 2, 30, getNow);
            Assert.IsTrue(date.Day == 28); // autocorrection default == true
        }

        #endregion constructors

        #region Setters

        [TestMethod]
        public void TestSetDay()
        {
            date = new DateOffset(offset, DateUnit.Day, getNow);

            date.Day = getNow().Day;

            Assert.IsTrue(date.Offset == 31); // days in 3 month
            Assert.IsTrue(date.Unit == DateUnit.Day);
        }

        [TestMethod]
        public void TestSetMonth()
        {
            date = new DateOffset(offset, DateUnit.Day, getNow);
            var d = getNow().AddMonths(-2);
            date.Month = d.Month;

            Assert.AreEqual(DateTime.DaysInMonth(getNow().Year, d.Month) + offset, date.Offset);
            Assert.IsTrue(date.Unit == DateUnit.Day);
        }

        [TestMethod]
        public void TestSetMonthNull()
        {
            date = new DateOffset(offset, DateUnit.Day, getNow);
            date.Month = null;

            Assert.IsTrue(date.Offset == 0);
            Assert.IsTrue(date.Unit == DateUnit.Year);
        }

        [TestMethod]
        public void TestSetUnitWider()
        {
            date = new DateOffset(offset, DateUnit.Day, getNow);
            date.CutsDate = true;
            date.Unit = DateUnit.Month;

            Assert.IsTrue(date.Offset == offset);
            Assert.IsTrue(date.Unit == DateUnit.Month);
            Assert.IsTrue(date.Year == 2013);
            Assert.IsTrue(date.Month == 11);
            Assert.IsTrue(date.Day == null);
        }

        [TestMethod]
        public void TestSetUnitWiderWeek()
        {
            date = new DateOffset(offset, DateUnit.Day, getNow);

            date.Unit = DateUnit.Week;

            Assert.IsTrue(date.Offset == offset);
            Assert.IsTrue(date.Unit == DateUnit.Week);
            Assert.IsTrue(date.Year == 2014);
            Assert.IsTrue(date.Month == 2);
            Assert.IsTrue(date.Day == 25);
        }

        [TestMethod]
        public void TestSetUnitNarrower()
        {
            date = new DateOffset(offset, DateUnit.Year, getNow);

            date.Unit = DateUnit.Month;

            Assert.IsTrue(date.Offset == offset);
            Assert.IsTrue(date.Unit == DateUnit.Month);
            Assert.IsTrue(date.Year == 2013);
            Assert.IsTrue(date.Month == 11);
            Assert.IsTrue(date.Day == null);
        }

        [TestMethod]
        public void TestSetMonth2()
        {
            date = new DateOffset(1, DateUnit.Year, getNow);

            date.Month = 4;

            Assert.IsTrue(date.Offset == 12);
            Assert.IsTrue(date.Unit == DateUnit.Month);
            Assert.IsTrue(date.Year == 2013);
            Assert.IsTrue(date.Month == 4);
            Assert.IsTrue(date.Day == null);
        }

        [TestMethod]
        public void SetNow()
        {
            var d = new DateOffset(null, null, null);
            d.Month = 1;
            d.Year = 2014;
            d.Now = new DateTime(2014, 11, 4);

            Assert.AreEqual(1, d.Month);
            Assert.AreEqual(2014, d.Year);
            Assert.AreEqual(10, d.Offset);
        }

        [TestMethod]
        public void CutsYearNull()
        {
            var date = new DateOffset(2014, 3, 31, getNow);
            date.CutsDate = true;
            date.Year = null;
            Assert.IsTrue(date.IsEmpty);

            var date2 = new DateOffset(2014, 3, null, getNow);
            date2.CutsDate = true;
            date2.Year = null;
            Assert.IsTrue(date2.IsEmpty);
        }

        [TestMethod]
        public void CutsMonthNull()
        {
            var date = new DateOffset(2014, 3, 31, getNow);
            date.CutsDate = true;
            date.Month = null;
            Assert.IsTrue(date.Year == 2014);
            Assert.IsTrue(date.Month == null);
            Assert.IsTrue(date.Day == null);
        }

        [TestMethod]
        public void NoCutsYearNull()
        {
            var date = new DateOffset(2014, 3, 31, getNow);
            date.CutsDate = false;
            date.Year = null;
            Assert.IsTrue(date.Year == null);
            Assert.IsTrue(date.Month == 3);
            Assert.IsTrue(date.Day == 31);

            var date2 = new DateOffset(2014, 3, null, getNow);
            date2.CutsDate = false;
            date2.Year = null;
            Assert.IsTrue(date2.Year == null);
            Assert.IsTrue(date2.Month == 3);
            Assert.IsTrue(date2.Day == null);
        }

        [TestMethod]
        public void NoCutsMonthNull()
        {
            var date = new DateOffset(2014, 3, 31, getNow);
            date.CutsDate = false;
            date.Month = null;
            Assert.IsTrue(date.Year == 2014);
            Assert.IsTrue(date.Month == null);
            Assert.IsTrue(date.Day == 31);
        }

        #endregion Setters

        #region Fill Empty

        // начинаем заполнять

        [TestMethod]
        public void FillDayByNow()
        {
            var date = new DateOffset(null, null, null, getNow);
            date.Day = 5;

            Assert.AreEqual(5, date.Day);
            Assert.AreEqual(getNow().Month, date.Month);
            Assert.AreEqual(getNow().Year, date.Year);
        }

        [TestMethod]
        public void FillMonthByNow()
        {
            var date = new DateOffset(null, null, null, getNow);
            date.Month = 5;

            Assert.AreEqual(null, date.Day);
            Assert.AreEqual(5, date.Month);
            Assert.AreEqual(getNow().Year, date.Year);
        }

        [TestMethod]
        public void FillYear()
        {
            var date = new DateOffset(null, null, null, getNow);
            date.Year = 2014;

            Assert.AreEqual(null, date.Day);
            Assert.AreEqual(null, date.Month);
            Assert.AreEqual(2014, date.Year);
        }

        [TestMethod]
        public void FillOffset()
        {
            var date = new DateOffset(null, null, null, getNow);
            date.Offset = 5;

            Assert.AreEqual(5, date.Offset);
            Assert.AreEqual(DateUnit.Day, date.Unit);
        }

        #endregion Fill Empty

        #region compare

        [TestMethod]
        public void TestLtDayMonth()
        {
            // 40 дней назад < сейчас
            var date1 = new DateOffset(40, DateUnit.Day, getNow);
            var date2 = new DateOffset(0, DateUnit.Month, getNow);

            Assert.IsTrue(date1 < date2);
        }

        [TestMethod]
        public void TestGtDayYear()
        {
            // 40 дней назад > год назад
            var date1 = new DateOffset(40, DateUnit.Day, getNow);
            var date2 = new DateOffset(1, DateUnit.Year, getNow);

            Assert.IsTrue(date1 > date2);
        }
        [TestMethod]
        public void TestLtMonthYear()
        {
            // 40 месяцев назад < 2 года назад
            var date1 = new DateOffset(40, DateUnit.Month, getNow);
            var date2 = new DateOffset(2, DateUnit.Year, getNow);

            Assert.IsTrue(date1 < date2);
        }

        [TestMethod]
        public void TestLtSameUnitDay()
        {
            // 40 дней назад < сейчас
            var date1 = new DateOffset(40, DateUnit.Day, getNow);
            var date2 = new DateOffset(0, DateUnit.Day, getNow);

            Assert.IsTrue(date1 < date2);
        }

        [TestMethod]
        public void TestLtSameUnitMonth()
        {
            // 2013.12 < 2014.02
            var date1 = new DateOffset(2013, 12, null, getNow);
            var date2 = new DateOffset(2014, 02, 0, getNow);

            Assert.IsTrue(date1 < date2);
        }

        [TestMethod]
        public void TestLtGtNull()
        {
            // 5 лет назад < пустая дата
            var date1 = new DateOffset(null, DateUnit.Month, getNow);
            var date2 = new DateOffset(5, DateUnit.Year, getNow);

            Assert.IsFalse(date1 < date2);
            Assert.IsTrue(date1 > date2);
        }

        [TestMethod]
        public void TestLtOrEqual()
        {
            // 2013.12 <= 2014.02
            var date1 = new DateOffset(2013, 12, null, getNow);
            var date2 = new DateOffset(2014, 02, 0, getNow);

            Assert.IsTrue(date1 <= date2);
        }

        [TestMethod]
        public void TestEqualSameDay()
        {
            var date1 = new DateOffset(2013, 12, 1, getNow);
            var date2 = new DateOffset(2013, 12, 1, getNow);

            Assert.IsTrue(date1 == date2);
            Assert.AreEqual(date1, date2);
        }

        [TestMethod]
        public void TestEqualSameMonth()
        {
            var date1 = new DateOffset(2, DateUnit.Month, getNow);
            var date2 = new DateOffset(2014, 2, null, getNow);

            Assert.IsTrue(date1 == date2);
            Assert.AreEqual(date1, date2);
        }

        [TestMethod]
        public void TestEqualPartialMonthDay()
        {
            // 2014.02 !=  2014.02.15
            var date1 = new DateOffset(2, DateUnit.Month, getNow);
            var date2 = new DateOffset(2014, 2, 15, getNow);

            // всегда больше
            Assert.IsTrue(date1 > date2);
            Assert.IsTrue(date2 > date1);
            Assert.IsTrue(date1 != date2);
            Assert.AreNotEqual(date1, date2);
        }

        [TestMethod]
        public void TestEqualPartialYearDay()
        {
            // 2014.04.1 != 2014
            var date1 = new DateOffset(3, DateUnit.Day, getNow);
            var date2 = new DateOffset(0, DateUnit.Year, getNow);

            // всегда больше
            Assert.IsTrue(date1 > date2);
            Assert.IsTrue(date2 > date1);
            Assert.IsTrue(date1 != date2);
            Assert.AreNotEqual(date1, date2);
        }

        [TestMethod]
        public void TestLtSameMonth()
        {
            var date1 = new DateOffset(2013, 5, 5, getNow);
            var date2 = new DateOffset(2014, 5, 1, getNow);

            Assert.IsTrue(date1 < date2);
        }

        [TestMethod]
        public void TestCompareWithWeek()
        {
            // сейчас > 2 недели назад
            var date1 = new DateOffset(getNow(), getNow);
            var date2 = new DateOffset(2, DateUnit.Week, getNow);

            Assert.IsTrue(date1 > date2);
        }

        #endregion compare
    }
}