using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.Models.Tests
{
    [TestClass]
    public class VocabularyTest : InMemoryDatabaseTest
    {
        string wt1 = "1";
        string wt2 = "2";
        [TestInitialize]
        public void VocabularyTestInit()
        {
            Load<Vocabulary>();
            Load<WordTemplate>();
        }
        [TestMethod]
        public void SetTemplates()
        {
            var voc = new Vocabulary("123");
            bool changed = false;
            voc.WordTemplatesChanged += (s, e) =>
            {
                changed = true;
            };

            voc.SetTemplates(new[] { wt1 });
            Assert.IsTrue(changed);
            Assert.IsTrue(voc.WordTemplates.Count(x => x.Title == wt1) > 0);

        }

        [TestMethod]
        public void SetSameTemplate()
        {
            var voc = new Vocabulary("123");
            voc.SetTemplates(new[] { wt1, wt1 });
            Assert.AreEqual(1, voc.WordTemplates.Count());
        }

        [TestMethod]
        public void SetTemplatesDistinctIgnoreCase()
        {
            var voc = new Vocabulary("123");
            voc.SetTemplates(new[] { "qwe", "qWe" });
            Assert.AreEqual(1, voc.WordTemplates.Count());
        }
        [TestMethod]
        public void SetTemplateAutoTrim()
        {
            var voc = new Vocabulary("123");
            voc.SetTemplates(new[] { " qwe  " });
            Assert.AreEqual("qwe", voc.WordTemplates.First().Title);
        }

        [TestMethod]
        public void ChangeTemplateCase()
        {
            // заменяем шаблон при смене регистра
            var voc = new Vocabulary("123");
            voc.SetTemplates(new[] { "qwe" });
            voc.SetTemplates(new[] { "QWE" });
            Assert.AreEqual("QWE", voc.WordTemplates.First().Title);
        }

        [TestMethod]
        public void DontChangeTemplatesCaseOnSave()
        {
            // сохраняем регистр шаблонов
            var voc = new Vocabulary("123");
            voc.SetTemplates(new[] { "qwe", "Asd" });
            voc.AddTemplates(new[] { "qweasd" });

            var titles = voc.WordTemplates.Select(x => x.Title);
            Assert.IsTrue(titles.Contains("qwe"));
            Assert.IsTrue(titles.Contains("Asd"));

        }

        [TestMethod]
        public void AddTemplates()
        {
            var tBefore = voc[1].WordTemplates.Count();
            voc[1].AddTemplates(new[] { wt1 });
            Assert.AreEqual(1, voc[1].WordTemplates.Count(x => x.Title == wt1));
            Assert.AreEqual(1 + tBefore, voc[1].WordTemplates.Count());
        }
    }
}
