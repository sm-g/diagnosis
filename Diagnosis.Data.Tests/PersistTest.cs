using Diagnosis.Common;
using Diagnosis.Data.Sync;
using Diagnosis.Models;
using Diagnosis.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.Data.Tests
{
    [TestClass]
    public class PersistTest : InMemoryDatabaseTest
    {
        [TestMethod]
        public void AddWordSaveVoc()
        {
            Load<Vocabulary>();
            var w = new Word("asdsad");
            voc[1].AddWord(w);

            Assert.IsTrue(voc[1].Words.Contains(w));

            new Saver(session).Save(voc[1]);

            Assert.IsTrue(!w.IsTransient);
        }

        [TestMethod]
        public void CustomVocCreatedAfterSaveNewWord()
        {
            var d = new Doctor("doc");
            Assert.IsTrue(d.CustomVocabulary.IsTransient);

            var newW = CreateWordAsInEditor("123", d);

            Assert.IsFalse(newW.IsTransient);
            Assert.IsFalse(d.CustomVocabulary.IsTransient);
        }

        [TestMethod]
        public void RemoveWordSaveVoc()
        {
            Load<Vocabulary>();

            var l = new VocLoader(session);
            l.LoadOrUpdateVocs(voc[1]);
            var w = voc[1].Words.First();

            voc[1].RemoveWord(w);
            w.OnDelete();
            new Saver(session).Delete(w); // или после
            new Saver(session).Save(voc[1]);

            Assert.IsFalse(GetWordTitles().Any(x => x == w.Title));
        }

        [TestMethod]
        public void RemoveTemplateSaveVoc()
        {
            Load<Vocabulary>();
            Load<WordTemplate>();

            var title = wTemp[1].Title;
            // remove unique wTemp[1]
            voc[1].SetTemplates(voc[1].WordTemplates.Select(x => x.Title)
                .Except(title.ToEnumerable()));

            new Saver(session).Save(voc[1]);

            Assert.IsTrue(!session.QueryOver<WordTemplate>().List().Any(x => x.Title == title));
        }

        [TestMethod]
        public void AddWordSaveCrit()
        {
            var crit = new Estimator() { Description = "1" };
            var w = new Word("1");
            var w2 = new Word("2");
            crit.SetWords(new[] { w, w2 });

            new Saver(session).Save(crit);

            var dbCrit = session.Get<Estimator>(crit.Id);
            var dbCritWords = session.QueryOver<CritWords>().List();

            Assert.IsTrue(dbCrit.Words.Contains(w));
            Assert.IsTrue(dbCrit.Words.Contains(w2));
            Assert.IsTrue(dbCritWords.Count >= 2);
        }

        [TestMethod]
        public void EntityDeletedRaisedByCascadeSave()
        {
            Load<Appointment>();

            bool found = false;
            var h = this.Subscribe(Event.EntityDeleted, (e) =>
            {
                var x = e.GetValue<IEntity>(MessageKeys.Entity);
                if (!found && x.Equals(a[1]))
                    found = true;
            });

            using (var tr = session.BeginTransaction())
            {
                a[1].GetAllHrs().ToList().ForEach(x =>
                    a[1].RemoveHealthRecord(x));

                a[1].Course.RemoveAppointment(a[1]);
                session.Save(a[1].Course);
                tr.Commit();
            }

            h.Dispose();
            Assert.IsTrue(found);
        }

        [TestMethod]
        public void SaveReadonly()
        {
            Load<HealthRecord>();
            Load<HrCategory>();
            Load<Word>();
            var yearWas = hr[1].FromDate.Year;
            var catWas = hr[1].Category;
            var unit = hr[1].Unit;
            var hriCountWas = hr[1].HrItems.Count();

            session.SetReadOnly(hr[1], true);
            hr[1].FromDate.Year = (yearWas ?? 2000) + 1;
            hr[1].Category = cat[4];
            hr[1].Unit = HealthRecordUnit.Week;
            hr[1].AddItems(w[22].ToEnumerable());
            using (var tr = session.BeginTransaction())
            {
                session.SaveOrUpdate(hr[1]);
                tr.Commit();
            }

            session.Evict(hr[1]);
            var loaded = session.Get<HealthRecord>(IntToGuid<HealthRecord>(1));
            Assert.AreEqual(yearWas, loaded.FromDate.Year);
            Assert.AreEqual(catWas, loaded.Category);
            Assert.AreEqual(unit, loaded.Unit);
            Assert.AreEqual(hriCountWas + 1, loaded.HrItems.Count());

        }
    }
}