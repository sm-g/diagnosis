using Diagnosis.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Diagnosis.Common.Tests
{
    [TestClass]
    public class DateHelperTest
    {
        [TestMethod]
        public void GetTotalMonths()
        {
            var now = new DateTime(2014, 12, 10);
            Assert.AreEqual(1, DateHelper.GetTotalMonthsBetween(now, 2014, 11));
        }

        [TestMethod]
        public void GetAgeBeforeNowMD()
        {
            // 5 лет в 2015 если родился в 2010
            var now = new DateTime(2015, 5, 1);
            Assert.AreEqual(5, DateHelper.GetAge(2010, 4, 1, now));
        }

        [TestMethod]
        public void GetAgeAfterNowMD()
        {
            var now = new DateTime(2015, 5, 1);
            Assert.AreEqual(4, DateHelper.GetAge(2010, 6, 1, now));
        }

        [TestMethod]
        public void GetAgeNoMD()
        {
            var now = new DateTime(2015, 5, 1);
            Assert.AreEqual(5, DateHelper.GetAge(2010, null, null, now));
        }

        [TestMethod]
        public void GetAgeNoYear()
        {
            var now = new DateTime(2015, 5, 1);
            Assert.AreEqual(null, DateHelper.GetAge(null, null, null, now));
        }

        [TestMethod]
        public void GetAgeNoDay()
        {
            var now = new DateTime(2015, 4, 1);
            Assert.AreEqual(4, DateHelper.GetAge(2010, 5, null, now));
        }

        [TestMethod]
        public void GetBirthYear()
        {
            // 5 лет в 2015 если родился в 2010
            var now = new DateTime(2015, 5, 1);
            Assert.AreEqual(2010, DateHelper.GetBirthYearByAge(5, null, null, now));
        }

        [TestMethod]
        public void GetBirthYearAfterNowMD()
        {
            var now = new DateTime(2015, 5, 1);
            Assert.AreEqual(2010, DateHelper.GetBirthYearByAge(4, 6, 1, now));
        }

        [TestMethod]
        public void GetBirthYearBeforeNowMD()
        {
            var now = new DateTime(2015, 5, 1);
            Assert.AreEqual(2010, DateHelper.GetBirthYearByAge(5, 4, 1, now));
        }

        [TestMethod]
        public void GetDateForAge()
        {
            // 5 лет наступит в момент
            Assert.AreEqual(new DateTime(2015, 4, 10), DateHelper.GetDateForAge(5, 2010, 4, 10));
        }

        [TestMethod]
        public void GetDateForAgePartial()
        {
            Assert.AreEqual(new DateTime(2015, 4, 1), DateHelper.GetDateForAge(5, 2010, 4, null));
        }

        [TestMethod]
        public void GetYearForAge()
        {
            // 2016.3.1 бдует 5 лет
            var now = new DateTime(2013, 3, 1);

            Assert.AreEqual(2016, DateHelper.GetYearForAge(5, 2010, 4, null, now));
        }

        [TestMethod]
        public void GetYearForAge2()
        {
            var now = new DateTime(2013, 5, 1);

            Assert.AreEqual(2015, DateHelper.GetYearForAge(5, 2010, 4, null, now));
        }
    }
}