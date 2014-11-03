using Diagnosis.Core;
using Diagnosis.Data.Queries;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Search;
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

            var parent = EntityProducers.DiagnosisProducer.Diagnoses[0];
            var s = new DiagnosisSearcher(parent, new Diagnosis.ViewModels.Search.HierarchicalSearchSettings());
            var res1 = s.Search("перикард").Select(x => x.diagnosis);

            var repo = new NHibernateRepository<Diag>(session);
            var s2 = new NewDiagnosisSearcher(parent.diagnosis, repo);
            var res2 = s2.Search("перикард");

            Assert.AreEqual(res1.Count(), res2.Count());
        }
    }
}
