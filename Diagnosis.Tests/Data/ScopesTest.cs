using Diagnosis.Common;
using Diagnosis.Data.Sync;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Diagnosis.Tests.Data
{
    [TestClass]
    public class ScopesTest
    {
        [TestMethod]
        public void GetScope()
        {
            Assert.AreEqual(Scope.Holder, typeof(Appointment).GetScopes().First());
            Assert.AreEqual(Scope.Hr, typeof(HrItem).GetScopes().First());
            Assert.AreEqual(Scope.Icd, typeof(IcdBlock).GetScopes().First());
            Assert.AreEqual(Scope.Reference, typeof(Uom).GetScopes().First());
            Assert.AreEqual(Scope.User, typeof(Doctor).GetScopes().First());
            Assert.AreEqual(Scope.Voc, typeof(Vocabulary).GetScopes().First());

            Assert.IsTrue(typeof(Speciality).GetScopes().Count() > 1);

        }
    }
}