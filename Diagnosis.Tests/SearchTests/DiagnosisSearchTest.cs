using Diagnosis.Common;
using Diagnosis.Data.Queries;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Diagnosis.ViewModels;
using Diag = Diagnosis.Models.Diagnosis;

namespace Tests.SearchTests
{
    [TestClass]
    public class DiagnosisSearchTest : InMemoryDatabaseTest
    {
        [TestMethod]
        public void Test1()
        {
            var d1 = session.Get<Doctor>(1);
            AuthorityController.LogIn(d1);

            var filter = new FilterViewModel<IcdDisease>(DiagnosisQuery.StartingWith(session));
            filter.Query = "перикард";
            var res1 = filter.Results;

            Assert.IsTrue(res1.Count() > 0);

        }
    }
}
