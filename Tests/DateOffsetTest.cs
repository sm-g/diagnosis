using System;
using Diagnosis.App.ViewModels;
using Diagnosis.App;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class DateOffsetTest
    {
        static DateOffset date;
        static Func<DateTime> now = () => new DateTime(2014, 4, 1);

        const int offset = 5;

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
            Assert.IsTrue(date.Day == now.Day);
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
            var now = DateTime.Today;

            date = new DateOffset(null, null, now.Day);

            Assert.IsTrue(date.Offset == 0);
            Assert.IsTrue(date.Unit == DateUnits.Day);
            Assert.IsTrue(date.Year == now.Year);
            Assert.IsTrue(date.Month == now.Month);
            Assert.IsTrue(date.Day == now.Day);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestConstructorDateNone()
        {
            date = new DateOffset(null, null, null);
        }

        #endregion

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
        public void TestSetUnitLess()
        {
            date = new DateOffset(offset, DateUnits.Day, now);

            date.Unit = DateUnits.Month;

            Assert.IsTrue(date.Offset == offset);
            Assert.IsTrue(date.Unit == DateUnits.Month);
            Assert.IsTrue(date.Year == 2013);
            Assert.IsTrue(date.Month == 11);
            Assert.IsTrue(date.Day == null);
        }

        [TestMethod]
        public void TestSetUnitGreater()
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
    }
}
