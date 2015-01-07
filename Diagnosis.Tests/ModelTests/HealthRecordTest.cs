using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Diagnosis.Common;

namespace Tests
{
    [TestClass]
    public class HealthRecordTest : InMemoryDatabaseTest
    {
        private Dictionary<int, HealthRecord> hr = new Dictionary<int, HealthRecord>();
        private static int[] hrIds = new[] { 1, 2, 20, 21, 22, 30, 31, 31, 40, 70, 71, 72, 73, 74 };

        [TestInitialize]
        public void AutocompleteTestInit()
        {
            hrIds.ForAll((id) => hr[id] = session.Get<HealthRecord>(IntToGuid<HealthRecord>(id)));
        }
        [TestMethod]
        public void Unit()
        {
            Assert.AreEqual(HealthRecordUnit.Year, hr[40].Unit);
            Assert.AreEqual(2005, hr[40].FromYear);


            Assert.AreEqual(HealthRecordUnit.Month, hr[20].Unit);
            Assert.AreEqual(2014, hr[20].FromYear);
            Assert.AreEqual(1, hr[20].FromMonth);

            Assert.AreEqual(HealthRecordUnit.NotSet, hr[1].Unit);
            Assert.AreEqual(2013, hr[1].FromYear);
            Assert.AreEqual(11, hr[1].FromMonth);

            Assert.AreEqual(HealthRecordUnit.ByAge, hr[2].Unit);
            Assert.AreEqual(2013, hr[2].FromYear);
            Assert.AreEqual(12, hr[2].FromMonth);

        }

    

    }
}
