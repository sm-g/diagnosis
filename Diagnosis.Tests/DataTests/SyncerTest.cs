using Diagnosis.Common;
using Diagnosis.Data.Sync;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class SyncerTest
    {
        [TestMethod]
        public void GetScope()
        {
            Assert.AreEqual(Scope.Holder, typeof(Appointment).GetScope());
            Assert.AreEqual(Scope.Hr, typeof(HrItem).GetScope());
            Assert.AreEqual(Scope.Icd, typeof(IcdBlock).GetScope());
            Assert.AreEqual(Scope.Reference, typeof(Uom).GetScope());
            Assert.AreEqual(Scope.User, typeof(Doctor).GetScope());
        }
    }
}