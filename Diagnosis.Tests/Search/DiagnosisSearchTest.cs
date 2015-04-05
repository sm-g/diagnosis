using Diagnosis.Common;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Controls;
using Diagnosis.ViewModels.Search;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Diagnosis.Tests.Search
{
    [TestClass]
    public class DiagnosisSearchTest : InMemoryDatabaseTest
    {
        [TestMethod]
        public void Test1()
        {
            Load<Doctor>();
            AuthorityController.TryLogIn(d1);

            var filter = new FilterViewModel<IcdDisease>(DiagnosisQuery.StartingWith(session));
            filter.Query = "перикард";
            var res1 = filter.Results;

            Assert.IsTrue(res1.Count() > 0);
        }
    }
}