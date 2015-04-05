using Diagnosis.Common;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Diagnosis.Tests.ViewModels
{
    [TestClass]
    public class FilterTest : InMemoryDatabaseTest
    {
        [TestMethod]
        public void AutoUpdateQuery()
        {
            var filter = new FilterViewModel<IcdDisease>(DiagnosisQuery.StartingWith(session));
            filter.Query = "перикард";

            var res1 = filter.Results;

            Assert.IsTrue(filter.AutoFiltered);
            Assert.IsTrue(res1.Count() > 0);
        }
    }
}