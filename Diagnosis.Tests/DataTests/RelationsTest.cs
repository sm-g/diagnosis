using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class RelationsTest : InMemoryDatabaseTest
    {


        [TestInitialize]
        public void Init()
        {
            Load<Doctor>();
            Load<Appointment>();
            Load<Word>();
            Load<Vocabulary>();
            Load<WordTemplate>();
            Load<UomType>();
            Load<Speciality>();
            Load<IcdBlock>();

            AuthorityController.TryLogIn(d1);
        }

        [TestMethod]
        public void Hrs2Words()
        {
            // новая запись со словом
            var hrsBefore = w[6].HealthRecords.Count();
            var hr = new HealthRecord(a[1], d1);
            hr.SetItems(new[] { w[6] });

            Assert.AreEqual(hrsBefore + 1, w[6].HealthRecords.Count());

            // запись удалена
            hr.OnDelete();
            session.Delete(hr);

            Assert.AreEqual(hrsBefore, w[6].HealthRecords.Count());
        }

        [TestMethod]
        public void Uom()
        {
            var uom = new Uom("x", 0, uomType[1]);
            Assert.IsTrue(uomType[1].Uoms.Contains(uom));
        }

        [TestMethod]
        public void AddIcdToSpec()
        {
            var blocksBefore = spec[1].IcdBlocks.Count();

            spec[1].AddBlock(block[91]);

            var blocksAfter = spec[1].IcdBlocks.Count();

            Assert.AreEqual(blocksBefore + 1, blocksAfter);
        }
    }
}