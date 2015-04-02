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
            Load<Vocabulary>();

            AuthorityController.TryLogIn(d1);
        }

        [TestMethod]
        public void Hrs2Words()
        {
            var hr = new HealthRecord(a[1], d1);
            hr.SetItems(new[] { w[6], w[3] });
            Assert.IsTrue(w[6].HealthRecords.Contains(hr));
            Assert.IsTrue(hr.GetOrderedEntities().Contains(w[3]));

            hr.SetItems(new[] { w[6] });

            Assert.IsFalse(hr.GetOrderedEntities().Contains(w[3]));
            Assert.IsFalse(w[3].HealthRecords.Contains(hr));

            hr.OnDelete();
            session.Delete(hr);
            Assert.IsFalse(w[6].HealthRecords.Contains(hr));
        }

        [TestMethod]
        public void AddUomToUomType()
        {
            var uom = new Uom("x", 0, uomType[1]);
            Assert.IsTrue(uomType[1].Uoms.Contains(uom));
        }

        [TestMethod]
        public void Spec2Icd()
        {
            Assert.IsFalse(spec[1].IcdBlocks.Contains(block[91]));

            spec[1].AddBlock(block[91]);
            Assert.IsTrue(spec[1].IcdBlocks.Contains(block[91]));

            spec[1].RemoveBlock(block[91]);
            Assert.IsFalse(spec[1].IcdBlocks.Contains(block[91]));
        }

        [TestMethod]
        public void Spec2Voc()
        {
            Assert.IsFalse(spec[1].Vocabularies.Contains(voc[1]));

            spec[1].AddVoc(voc[1]);
            Assert.IsTrue(spec[1].Vocabularies.Contains(voc[1]));
            Assert.IsTrue(voc[1].Specialities.Contains(spec[1]));

            spec[1].RemoveVoc(voc[1]);
            Assert.IsFalse(spec[1].Vocabularies.Contains(voc[1]));
            Assert.IsFalse(voc[1].Specialities.Contains(spec[1]));
        }

        [TestMethod]
        public void Hr2HrItem()
        {
            Assert.IsFalse(spec[1].Vocabularies.Contains(voc[1]));

            var hr = new HealthRecord(a[1], d1);
            hr.AddItems(new[] { w[1], w[2] });
            Assert.IsTrue(hr.GetOrderedEntities().Contains(w[2]));

            var hrItem = hr.HrItems.FirstOrDefault(x => x.Entity.Equals(w[2]));
            Assert.IsNotNull(hrItem);
            Assert.AreEqual(hr, hrItem.HealthRecord);

            hr.SetItems(new[] { w[1] });
            Assert.IsFalse(hr.GetOrderedEntities().Contains(w[2]));

            hrItem = hr.HrItems.FirstOrDefault(x => x.Entity.Equals(w[2]));
            Assert.IsNull(hrItem);
        }

        [TestMethod]
        public void Voc2WordTemplate()
        {
            var titleswas = voc[1].GetOrderedTemplateTitles().ToList();
            Assert.IsFalse(titleswas.Contains("123"));
            voc[1].SetTemplates(titleswas.Union("123".ToEnumerable()));

            var wt = voc[1].WordTemplates.FirstOrDefault(x => x.Title == "123");
            Assert.IsNotNull(wt);
            Assert.AreEqual(voc[1], wt.Vocabulary);

            voc[1].SetTemplates(titleswas);
            Assert.IsFalse(voc[1].GetOrderedTemplateTitles().Contains("123"));

            wt = voc[1].WordTemplates.FirstOrDefault(x => x.Title == "123");
            Assert.IsNull(wt);
        }

        [TestMethod]
        public void Course2App()
        {
            var c = new Course();
            var app = c.AddAppointment(d1);

            Assert.AreEqual(app, c.Appointments.Single());
            Assert.AreEqual(c, app.Course);

            c.RemoveAppointment(app);

            Assert.IsFalse(c.Appointments.Contains(app));
            // у осмотра все еще есть курс
        }

        [TestMethod]
        public void Pat2Course()
        {
            var p = new Patient();
            var c = p.AddCourse(d1);

            Assert.AreEqual(c, p.Courses.Single());
            Assert.AreEqual(p, c.Patient);

            p.RemoveCourse(c);
            Assert.IsFalse(p.Courses.Contains(c));
        }

        [TestMethod]
        public void Doctor2Course()
        {
            var p = new Patient();
            var c = p.AddCourse(d1);

            Assert.AreEqual(d1, c.LeadDoctor);
            Assert.IsTrue(d1.Courses.Contains(c));

            p.RemoveCourse(c);
            Assert.IsFalse(d1.Courses.Contains(c));
        }

        [TestMethod]
        public void Doctor2App()
        {
            var c = new Course();
            var app = c.AddAppointment(d1);

            Assert.AreEqual(d1, app.Doctor);
            Assert.IsTrue(d1.Appointments.Contains(app));

            c.RemoveAppointment(app);
            Assert.IsFalse(d1.Appointments.Contains(app));
        }

        [TestMethod]
        public void Pat2Hr()
        {
            var p = new Patient();
            var hr = p.AddHealthRecord(d1);

            Assert.AreEqual(p, hr.Patient);
            Assert.IsTrue(p.HealthRecords.Contains(hr));

            p.RemoveHealthRecord(hr);

            Assert.IsFalse(p.HealthRecords.Contains(hr));
        }

        [TestMethod]
        public void Course2Hr()
        {
            var c = new Course();
            var hr = c.AddHealthRecord(d1);

            Assert.AreEqual(c, hr.Course);
            Assert.IsTrue(c.HealthRecords.Contains(hr));

            c.RemoveHealthRecord(hr);

            Assert.IsFalse(c.HealthRecords.Contains(hr));
        }

        [TestMethod]
        public void App2Hr()
        {
            var app = new Appointment();
            var hr = app.AddHealthRecord(d1);

            Assert.AreEqual(app, hr.Appointment);
            Assert.IsTrue(app.HealthRecords.Contains(hr));

            app.RemoveHealthRecord(hr);

            Assert.IsFalse(app.HealthRecords.Contains(hr));
        }

        [TestMethod]
        public void Doctor2Hr()
        {
            var app = new Appointment();
            var hr = app.AddHealthRecord(d1);

            Assert.IsTrue(d1.HealthRecords.Contains(hr));

            var c = new Course();
            var hr2 = c.AddHealthRecord(d1);

            Assert.IsTrue(d1.HealthRecords.Contains(hr2));

            app.RemoveHealthRecord(hr);
            c.RemoveHealthRecord(hr2);
            Assert.IsFalse(d1.HealthRecords.Contains(hr));
            Assert.IsFalse(d1.HealthRecords.Contains(hr2));
        }

        [TestMethod]
        public void HrCascade()
        {
        }
    }
}