using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Diagnosis.Common;
using System.Globalization;

namespace Tests
{
    [TestClass]
    public class TimeSpanFormatterTest
    {
        [TestMethod]
        public void Common()
        {
            DateTime one = new DateTime(2014, 1, 1, 0, 0, 0);
            DateTime two = new DateTime(2014, 1, 3, 6, 5, 0);
            var ts = two - one;
            var str = TimeSpanFormatter.GetTimeSpanString(ts, 3);

            Assert.AreEqual("2 дня 6 ч 5 м", str);
        }
        [TestMethod]
        public void ManyDays()
        {
            DateTime one = new DateTime(2014, 1, 1);
            DateTime two = new DateTime(2014, 1, 5);
            var ts = two - one;
            var str = TimeSpanFormatter.GetTimeSpanString(ts, 3);

            Assert.AreEqual("4 дня", str);
        }
        [TestMethod]
        public void LessThanDay()
        {
            DateTime one = new DateTime(2014, 1, 1, 19, 5, 0);
            DateTime two = new DateTime(2014, 1, 2, 15, 6, 0);
            var ts = two - one;
            var str = TimeSpanFormatter.GetTimeSpanString(ts, 3);

            Assert.AreEqual("20 ч 1 м", str);
        }
        [TestMethod]
        public void SameMinutes()
        {
            DateTime one = new DateTime(2014, 1, 1, 0, 5, 0);
            DateTime two = new DateTime(2014, 1, 3, 6, 5, 0);
            var ts = two - one;
            var str = TimeSpanFormatter.GetTimeSpanString(ts, 3);

            Assert.AreEqual("2 дня 6 ч", str);
        }

        [TestMethod]
        public void SameTime()
        {
            DateTime one = new DateTime(2014, 1, 1, 15, 5, 0);
            DateTime two = new DateTime(2014, 1, 3, 15, 5, 0);
            var ts = two - one;
            var str = TimeSpanFormatter.GetTimeSpanString(ts, 3);

            Assert.AreEqual("2 дня 0 ч", str);
        }
        [TestMethod]
        public void LessThanDaySameMinutes()
        {
            DateTime one = new DateTime(2014, 1, 1, 19, 5, 0);
            DateTime two = new DateTime(2014, 1, 1, 20, 5, 0);
            var ts = two - one;
            var str = TimeSpanFormatter.GetTimeSpanString(ts, 3);

            Assert.AreEqual("1 ч", str);
        }
        [TestMethod]
        public void LessThanDaySameHours()
        {
            DateTime one = new DateTime(2014, 1, 1, 20, 5, 0);
            DateTime two = new DateTime(2014, 1, 1, 20, 6, 0);
            var ts = two - one;
            var str = TimeSpanFormatter.GetTimeSpanString(ts, 3);

            Assert.AreEqual("0 ч 1 м", str);
        }
        [TestMethod]
        public void Equal()
        {
            DateTime one = new DateTime(2014, 1, 1, 19, 5, 0);
            DateTime two = one;
            var ts = two - one;
            var str = TimeSpanFormatter.GetTimeSpanString(ts, 3, "equal");

            Assert.AreEqual("equal", str);
        }
        [TestMethod]
        public void Negative()
        {
            DateTime one = new DateTime(2014, 1, 2, 15, 6, 0);
            DateTime two = new DateTime(2014, 1, 1, 19, 5, 0);
            var ts = two - one;
            var str = TimeSpanFormatter.GetTimeSpanString(ts, 3, "equal");

            Assert.AreEqual("equal", str);
        }
    }
}
