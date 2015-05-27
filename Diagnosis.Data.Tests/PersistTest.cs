using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Sync;
using Diagnosis.Models;
using Diagnosis.Tests;
using Diagnosis.ViewModels;
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

            session.DoSave(voc[1]);

            Assert.IsTrue(!w.IsTransient);
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
            session.DoDelete(w); // или после
            session.DoSave(voc[1]);

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

            session.DoSave(voc[1]);

            Assert.IsTrue(!session.QueryOver<WordTemplate>().List().Any(x => x.Title == title));
        }
    }
}