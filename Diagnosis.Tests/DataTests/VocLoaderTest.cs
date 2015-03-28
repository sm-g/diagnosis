using Diagnosis.Common;
using Diagnosis.Data.Queries;
using Diagnosis.Data.Sync;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class VocLoaderTest : InMemoryDatabaseTest
    {
        private VocLoader l;
        private Doctor d1;

        [TestInitialize]
        public void Init()
        {
            d1 = session.Get<Doctor>(IntToGuid<Doctor>(1));
            AuthorityController.TryLogIn(d1);
            l = new VocLoader(session);
            aIds.ForAll((id) => a[id] = session.Get<Appointment>(IntToGuid<Appointment>(id)));

            wIds.ForAll((id) => w[id] = session.Get<Word>(IntToGuid<Word>(id)));
            vocIds.ForAll((id) => voc[id] = session.Get<Vocabulary>(IntToGuid<Vocabulary>(id)));
            wTempIds.ForAll((id) => wTemp[id] = session.Get<WordTemplate>(IntToGuid<WordTemplate>(id)));
        }

        [TestMethod]
        public void DataConditions()
        {
            Assert.AreEqual(wTemp[4].Title, w[6].Title);
            Assert.AreEqual(0, w[6].HealthRecords.Count());

            Assert.AreEqual(wTemp[2].Title, wTemp[6].Title);
            Assert.AreEqual(wTemp[3].Title, wTemp[7].Title);

            // шаблона 1 нет среди слов
            Assert.IsFalse(w.Select(x => x.Value.Title).Contains(wTemp[1].Title));
        }

        [TestMethod]
        public void DeleteWordUpdateVoc()
        {
            l.LoadOrUpdateVocs(voc[1]);
            var word = voc[1].Words.First();
            Assert.IsTrue(word.IsEmpty());

            var origTitle = word.Title;
            using (var tr = session.BeginTransaction())
            {
                voc[1].RemoveWord(word);
                session.Delete(word);
                tr.Commit();
            }
            Assert.IsFalse(GetWordTitles().Contains(origTitle));
            Assert.IsFalse(voc[1].Words.Select(x => x.Title).Contains(origTitle));

            // слово заново создается
            l.LoadOrUpdateVocs(voc[1]);
            Assert.IsTrue(GetWordTitles().Contains(origTitle));
            Assert.IsTrue(voc[1].Words.Select(x => x.Title).Contains(origTitle));
        }

        [TestMethod]
        public void CreateWordLoadVoc()
        {
            // созданное слово в пользовательском словаре
            var newW = new Word(wTemp[1].Title);
            var wEditor = new WordEditorViewModel(newW);
            wEditor.OkCommand.Execute(null);
            Assert.IsTrue(newW.Vocabularies.Count() == 1);

            // загружен словарь - слово также в нем
            l.LoadOrUpdateVocs(voc[1]);
            Assert.IsTrue(newW.Vocabularies.Count() == 2);
            Assert.IsTrue(newW.Vocabularies.Contains(voc[1]));
        }

        [TestMethod]
        public void RenameWordUpdateVoc()
        {
            // измененное слово остается в словаре до обновления словаря,
            // надо вручную перевести в пользовательский словарь, чтобы
            // при удалении словаря измененные слова не удалены
            l.LoadOrUpdateVocs(voc[1]);
            var vocTemplates = voc[1].WordTemplates.Count();
            var word = voc[1].Words.Where(x => x.Title == wTemp[3].Title).Single();
            var origTitle = word.Title;

            Assert.IsTrue(word.Vocabularies.Single() == voc[1]);

            using (var tr = session.BeginTransaction())
            {
                word.Title = "qwe";
                session.SaveOrUpdate(word);
                tr.Commit();
            }
            Assert.IsTrue(voc[1].Words.Contains(word));

            // заново создается слово по шаблону после обновления
            l.LoadOrUpdateVocs(voc[1]);

            Assert.IsFalse(voc[1].Words.Contains(word));

            Assert.AreEqual(vocTemplates, voc[1].Words.Count());
            Assert.IsTrue(voc[1].Words.Count(x => x.Title == origTitle) > 0);
        }

        [TestMethod]
        public void DeleteTemplateUpdateVoc()
        {
            l.LoadOrUpdateVocs(voc[1]);
            var vocWords = voc[1].Words.Count();
            var word = voc[1].Words.Where(x => x.Title == wTemp[3].Title).Single();
            voc[1].RemoveWordTemplate(wTemp[3]);

            l.LoadOrUpdateVocs(voc[1]);
            // слово больше не в словаре
            Assert.IsFalse(voc[1].Words.Contains(word));
            Assert.AreEqual(vocWords - 1, voc[1].Words.Count());
        }

        [TestMethod]
        public void AddTemplateUpdateVoc()
        {
            var newT = new WordTemplate("qwe", voc[1]);
            Assert.IsFalse(GetWordTitles().Contains("qwe"));
            l.LoadOrUpdateVocs(voc[1]);
            Assert.IsTrue(GetWordTitles().Contains("qwe"));
        }

        [TestMethod]
        public void ChangeTemplateUpdateVoc()
        {
            l.LoadOrUpdateVocs(voc[2]);
            Assert.IsTrue(w[6].Vocabularies.Contains(voc[2]));
            Assert.IsTrue(voc[2].Words.Count() == voc[2].WordTemplates.Count());

            var hr = new HealthRecord(a[1], d1);
            hr.SetItems(new[] { w[6] });
            session.SaveOrUpdate(hr);

            Assert.IsTrue(w[6].HealthRecords.Count() == 1);

            // меняем шаблон в словаре, обновляем словарь, слово больше не в словаре
            wTemp[4].Title = "asdf";
            l.LoadOrUpdateVocs(voc[2]);

            Assert.IsFalse(w[6].Vocabularies.Contains(voc[2]));
            Assert.IsTrue(voc[2].Words.Count() == voc[2].WordTemplates.Count());
        }

        [TestMethod]
        public void Sequence()
        {
            // новое слово
            var newW = new Word(wTemp[1].Title);
            var wEditor = new WordEditorViewModel(newW);
            wEditor.OkCommand.Execute(null);

            // загружаем словарь, нет использованных слов
            l.LoadOrUpdateVocs(voc[1]);
            Assert.IsTrue(voc[1].Words.All(x => x.HealthRecords.Count() == 0));

            // меняем неиспользованное слово
            var word3 = voc[1].Words.Where(x => x.Title == wTemp[3].Title).Single();
            Assert.IsTrue(word3.HealthRecords.Count() == 0);

            word3.Title = "qwe";
            // надо вручную перевести в пользовательский словарь, чтобы
            // при удалении словаря измененные слова не удалены
            voc[1].RemoveWord(word3);
            VocabularyQuery.Custom(session)().AddWord(word3);
            using (var tr = session.BeginTransaction())
            {
                session.SaveOrUpdate(word3);
                tr.Commit();
            }

            // убираем словарь
            l.DeleteVocs(voc[1]);

            // осталось 1 как пользовательское, и 3 измененное
            var wordTitles = GetWordTitles();

            Assert.IsTrue(wordTitles.Contains(wTemp[1].Title));
            Assert.IsFalse(wordTitles.Contains(wTemp[2].Title));
            Assert.IsFalse(wordTitles.Contains(wTemp[3].Title));
            Assert.IsTrue(wordTitles.Contains("qwe"));

            // загружаем 2 словарь
            l.LoadOrUpdateVocs(voc[2]);
            // снова есть слово 3
            Assert.IsTrue(voc[2].Words.Where(x => x.Title == wTemp[3].Title).Count() > 0);

            // используем слово 4
            var word4 = voc[2].Words.Where(x => x.Title == wTemp[4].Title).Single();

            var hr = new HealthRecord(a[1], d1);
            hr.SetItems(new[] { word4 });
            session.SaveOrUpdate(hr);

            Assert.IsTrue(word4.HealthRecords.Count() > 0);

            // менем шаблон 4, добавляем шаблон 5
            wTemp[4].Title = "poiuy";
            new WordTemplate("555", voc[2]);
            session.SaveOrUpdate(voc[2]);

            Assert.AreEqual(5, voc[2].WordTemplates.Count());

            // обновляем 2 словарь, снова есть 2 и3
            l.LoadOrUpdateVocs(voc[2]);
            wordTitles = GetWordTitles();
            Assert.IsTrue(wordTitles.Contains(wTemp[2].Title));
            Assert.IsTrue(wordTitles.Contains(wTemp[3].Title));
        }

        IList<string> GetWordTitles()
        {
            return session.Query<Word>()
               .Select(x => x.Title).ToList();
        }
    }
}