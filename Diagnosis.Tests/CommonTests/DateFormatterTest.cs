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
    public class DateFormatterTest
    {
        // совпадения в двух датах: дня, месяца, года и совпадение года первой даты с текущим
        // возможные случаи
        //   | D   M    Y   Y=Now.Y
        // --+----------------
        // 1 | -   0/1  0   0/1
        // 2 | -   0    1   0
        // 3 | -   0    1   1
        // 4 | 0   1    1   0    
        // 5 | 0   1    1   1    
        // 6 | 1   1    1   1  
        // 7 | 1   1    1   0  
        Tuple<string, string> first = new Tuple<string, string>("d MMMM yyyy", "d MMMM yyyy");
        Tuple<string, string> second = new Tuple<string, string>("d MMMM", "d MMMM yyyy");
        Tuple<string, string> third = new Tuple<string, string>("d MMMM", "d MMMM");
        Tuple<string, string> fourth = new Tuple<string, string>("%d", "d MMMM yyyy");
        Tuple<string, string> fifth = new Tuple<string, string>("%d", "d MMMM");
        Tuple<string, string> sixth = new Tuple<string, string>("d MMMM", "");
        Tuple<string, string> seventh = new Tuple<string, string>("d MMMM yyyy", "");

        [TestMethod]
        public void TestFirstCase()
        {
            DateTime one = new DateTime(DateTime.Now.Year - 1, 1, 1);
            DateTime two = new DateTime(DateTime.Now.Year, 1, 1);

            var result1 = DateFormatter.GetFormat(one, two);

            one = new DateTime(DateTime.Now.Year - 1, 1, 1);
            two = new DateTime(DateTime.Now.Year, 2, 1);

            var result2 = DateFormatter.GetFormat(one, two);

            one = new DateTime(DateTime.Now.Year, 1, 1);
            two = new DateTime(DateTime.Now.Year - 1, 2, 1);

            var result3 = DateFormatter.GetFormat(one, two);

            one = new DateTime(DateTime.Now.Year, 1, 1);
            two = new DateTime(DateTime.Now.Year - 1, 1, 1);

            var result4 = DateFormatter.GetFormat(one, two);

            Assert.AreEqual(first, result1);
            Assert.AreEqual(first, result2);
            Assert.AreEqual(first, result3);
            Assert.AreEqual(first, result4);
        }

        [TestMethod]
        public void TestSecondCase()
        {
            DateTime one = new DateTime(DateTime.Now.Year - 1, 1, 1);
            DateTime two = new DateTime(DateTime.Now.Year - 1, 2, 1);

            var result1 = DateFormatter.GetFormat(one, two);

            Assert.AreEqual(second, result1);
        }

        [TestMethod]
        public void TestThirdCase()
        {
            DateTime one = new DateTime(DateTime.Now.Year, 1, 1);
            DateTime two = new DateTime(DateTime.Now.Year, 2, 1);

            var result1 = DateFormatter.GetFormat(one, two);

            Assert.AreEqual(third, result1);
        }

        [TestMethod]
        public void TestFourthCase()
        {
            DateTime one = new DateTime(DateTime.Now.Year - 1, 1, 1);
            DateTime two = new DateTime(DateTime.Now.Year - 1, 1, 5);

            var result1 = DateFormatter.GetFormat(one, two);

            Assert.AreEqual(fourth, result1);
        }

        [TestMethod]
        public void TestFifthCase()
        {
            DateTime one = new DateTime(DateTime.Now.Year, 1, 1);
            DateTime two = new DateTime(DateTime.Now.Year, 1, 5);

            var result1 = DateFormatter.GetFormat(one, two);

            Assert.AreEqual(fifth, result1);
        }

        [TestMethod]
        public void TestSixthCase()
        {
            DateTime one = new DateTime(DateTime.Now.Year, 1, 1);

            var result1 = DateFormatter.GetFormat(one, one);
            var result2 = DateFormatter.GetFormat(one, null);

            Assert.AreEqual(sixth, result1);
            Assert.AreEqual(sixth, result2);
        }

        [TestMethod]
        public void TestSeventhCase()
        {
            DateTime one = new DateTime(DateTime.Now.Year - 1, 1, 1);

            var result1 = DateFormatter.GetFormat(one, one);
            var result2 = DateFormatter.GetFormat(one, null);

            Assert.AreEqual(seventh, result1);
            Assert.AreEqual(seventh, result2);
        }
    }
}
