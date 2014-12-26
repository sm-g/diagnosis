using Diagnosis.Common;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.ModelTests
{
    [TestClass]
    public class CourseTest
    {
        private List<DateTime> dt = new List<DateTime>();

        [TestInitialize]
        public void Init()
        {
            Enumerable.Range(1, 12)
                .ForAll(m => dt.Add(new DateTime(2013, m, 1)));
        }

        /// <summary>
        /// c1----
        /// ---c2-
        /// </summary>
        [TestMethod]
        public void CompareEnd()
        {
            var c1 = new Course() { Start = dt[0], End = dt[1] };
            var c2 = new Course() { Start = dt[2], End = dt[3] };

            var r = new CourseEarlierFirst().Compare(c1, c2);
            Assert.IsTrue(r == -1);
        }

        /// <summary>
        /// c1+++-
        /// --c2+-
        /// </summary>
        [TestMethod]
        public void CompareEndSame()
        {
            var c1 = new Course() { Start = dt[0], End = dt[3] };
            var c2 = new Course() { Start = dt[2], End = dt[3] };

            var r = new CourseEarlierFirst().Compare(c1, c2);
            Assert.IsTrue(r == -1);
        }

        /// <summary>
        /// c1++-
        /// -c2--
        /// </summary>
        [TestMethod]
        public void CompareEndInside()
        {
            var c1 = new Course() { Start = dt[0], End = dt[3] };
            var c2 = new Course() { Start = dt[1], End = dt[2] };

            var r = new CourseEarlierFirst().Compare(c1, c2);
            Assert.IsTrue(r == 1);
        }

        /// <summary>
        /// -c1-
        /// c2--
        /// </summary>
        [TestMethod]
        public void CompareEndShifted()
        {
            var c1 = new Course() { Start = dt[1], End = dt[3] };
            var c2 = new Course() { Start = dt[0], End = dt[2] };

            var r = new CourseEarlierFirst().Compare(c1, c2);
            Assert.IsTrue(r == 1);
        }

        /// <summary>
        /// -c1+
        /// c2--
        /// </summary>
        [TestMethod]
        public void CompareNoEnd()
        {
            var c1 = new Course() { Start = dt[3] };
            var c2 = new Course() { Start = dt[1], End = dt[2] };

            var r = new CourseEarlierFirst().Compare(c1, c2);
            Assert.IsTrue(r == 1);
        }

        /// <summary>
        /// -c1++
        /// --c2-
        /// </summary>
        [TestMethod]
        public void CompareNoEndInside()
        {
            var c1 = new Course() { Start = dt[0] };
            var c2 = new Course() { Start = dt[1], End = dt[2] };

            var r = new CourseEarlierFirst().Compare(c1, c2);
            Assert.IsTrue(r == 1);
        }

        /// <summary>
        /// c1++
        /// -c2+
        /// </summary>
        [TestMethod]
        public void CompareNoEndBoth()
        {
            var c1 = new Course() { Start = dt[0] };
            var c2 = new Course() { Start = dt[1] };

            var r = new CourseEarlierFirst().Compare(c1, c2);
            Assert.IsTrue(r == -1);
        }
    }
}