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
        private Doctor d1;

        [TestInitialize]
        public void Init()
        {
            d1 = session.Get<Doctor>(IntToGuid<Doctor>(1));
            AuthorityController.TryLogIn(d1);

            aIds.ForAll((id) => a[id] = session.Get<Appointment>(IntToGuid<Appointment>(id)));
            wIds.ForAll((id) => w[id] = session.Get<Word>(IntToGuid<Word>(id)));
            vocIds.ForAll((id) => voc[id] = session.Get<Vocabulary>(IntToGuid<Vocabulary>(id)));
            wTempIds.ForAll((id) => wTemp[id] = session.Get<WordTemplate>(IntToGuid<WordTemplate>(id)));
            uomTypeIds.ForAll((id) => uomType[id] = session.Get<UomType>(IntToGuid<UomType>(id)));
            specIds.ForAll((id) => spec[id] = session.Get<Speciality>(IntToGuid<Speciality>(id)));
            blockIds.ForAll((id) => block[id] = session.Get<IcdBlock>(id));
        }

        [TestMethod]
        public void Hrs2Words()
        {
            var hrsBefore = w[6].HealthRecords.Count();
            var hr = new HealthRecord(a[1], d1);
            hr.SetItems(new[] { w[6] });

            Assert.AreEqual(hrsBefore + 1, w[6].HealthRecords.Count());

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