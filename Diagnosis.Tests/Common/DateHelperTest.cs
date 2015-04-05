using Diagnosis.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Tests.Common
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
    }
}