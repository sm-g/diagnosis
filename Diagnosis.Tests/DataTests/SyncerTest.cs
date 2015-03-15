using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Diagnosis.Common;
using System.Linq;
using Diagnosis.Data.Sync;

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