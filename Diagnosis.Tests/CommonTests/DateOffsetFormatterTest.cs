﻿using Diagnosis.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Tests
{
    [TestClass]
    public class DateOffsetFormatterTest
    {
        private DateOffset date;
        private int offset = 5;
        private DateTime now;

        [TestInitialize]
        public void Init()
        {
            now = DateTime.Today;
        }

        [TestMethod]
        public void Unit()
        {
            var str = DateOffsetFormatter.FormatUnit(1, DateUnits.Day);

            Assert.AreEqual(Plurals.days[0], str);
        }

        [TestMethod]
        public void OffsetNull()
        {
            var str = DateOffsetFormatter.FormatUnit(null, DateUnits.Day);

            Assert.AreEqual(Plurals.days[2], str);
        }

        [TestMethod]
        public void Full()
        {
            date = new DateOffset(now.Year, now.Month, now.Day);

            var str = DateOffsetFormatter.GetPartialDateString(date);
            Assert.AreEqual(now.ToString("d MMMM yyyy"), str);
        }

        [TestMethod]
        public void Year()
        {
            date = new DateOffset(now.Year, null, null);

            var str = DateOffsetFormatter.GetPartialDateString(date);
            Assert.AreEqual(now.ToString("yyyy"), str);
        }

        [TestMethod]
        public void YearMonth()
        {
            date = new DateOffset(now.Year, now.Month, null);

            var str = DateOffsetFormatter.GetPartialDateString(date);
            Assert.AreEqual(now.ToString(System.Globalization.DateTimeFormatInfo.CurrentInfo.MonthNames[now.Month - 1].ToLower() + " yyyy"), str);
        }

        [TestMethod]
        public void YearDay()
        {
            date = new DateOffset(now.Year, null, now.Day);

            var str = DateOffsetFormatter.GetPartialDateString(date);
            Assert.AreEqual(now.ToString("yyyy"), str);
        }

        [TestMethod]
        public void Month()
        {
            date = new DateOffset(null, now.Month, null);

            var str = DateOffsetFormatter.GetPartialDateString(date);
            Assert.AreEqual(now.ToString(System.Globalization.DateTimeFormatInfo.CurrentInfo.MonthNames[now.Month - 1].ToLower() + " yyyy"), str);
        }

        [TestMethod]
        public void MonthDay()
        {
            date = new DateOffset(null, now.Month, now.Day);

            var str = DateOffsetFormatter.GetPartialDateString(date);

            Assert.AreEqual(now.ToString("d MMMM yyyy"), str);
        }

        [TestMethod]
        public void Day()
        {
            date = new DateOffset(null, null, now.Day);
            var str = DateOffsetFormatter.GetPartialDateString(date);

            Assert.AreEqual(now.ToString("d MMMM yyyy"), str);
        }

        [TestMethod]
        public void None()
        {
            date = new DateOffset(null, null, null);
            var str = DateOffsetFormatter.GetPartialDateString(date);

            Assert.AreEqual("", str);
        }
    }
}