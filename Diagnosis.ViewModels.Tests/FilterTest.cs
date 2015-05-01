using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.Tests;
using Diagnosis.ViewModels.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class FilterTest : ViewModelTest
    {
        static string[] strs = new[] { "qwe", "asd", "zxc" };
        [TestMethod]
        public void AutoUpdateQuery()
        {
            var filter = new FilterViewModel<string>((x) => strs.Where(s => s.StartsWith(x)));
            filter.Query = "q";

            var res1 = filter.Results;

            Assert.IsTrue(filter.AutoFiltered);
            Assert.IsTrue(res1.Any());
        }
    }
}