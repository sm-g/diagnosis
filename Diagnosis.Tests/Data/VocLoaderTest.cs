﻿using Diagnosis.Common;
using Diagnosis.Data.Sync;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Tests.Data
{
    [TestClass]
    public class VocLoaderTest : InMemoryDatabaseTest
    {
        private VocLoader l;

        [TestInitialize]
        public void Init()
        {
            l = new VocLoader(session);

            Load<Doctor>();
            Load<Appointment>();
            Load<Word>();
            Load<Vocabulary>();
            Load<WordTemplate>();

            AuthorityController.TryLogIn(d1);
        }

        [TestMethod]
        public void DataConditions()
        {
            // есть слова для шаблонов 4 и 5
            Assert.AreEqual(wTemp[4].Title, w[6].Title);
            Assert.AreEqual(wTemp[5].Title, w[5].Title);
            // шаблонов 1,2,3 нет среди слов
            Assert.IsFalse(w.Select(x => x.Value.Title).Any(t => t == wTemp[1].Title || t == wTemp[2].Title || t == wTemp[3].Title));

            // слово 6 без записей
            Assert.AreEqual(0, w[6].HealthRecords.Count());

            // шаблоны 2==6, 3==7
            Assert.AreEqual(wTemp[2].Title, wTemp[6].Title);
            Assert.AreEqual(wTemp[3].Title, wTemp[7].Title);
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
                word.OnDelete();
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
            var newW = CreateWordAsInEditor(wTemp[1].Title);
            Assert.IsTrue(newW.Vocabularies.Single().IsCustom);

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
            // при удалении словаря измененные слова не удались
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
            // remove wTemp[3]
            voc[1].SetTemplates(voc[1].WordTemplates.Select(x => x.Title)
                .Except(word.Title.ToEnumerable()));

            l.LoadOrUpdateVocs(voc[1]);
            // слово больше не в словаре
            Assert.IsFalse(voc[1].Words.Contains(word));
            Assert.AreEqual(vocWords - 1, voc[1].Words.Count());
        }

        [TestMethod]
        public void AddTemplateUpdateVoc()
        {
            voc[1].AddTemplates(new[] { "qwe" });
            Assert.IsFalse(GetWordTitles().Contains("qwe"));
            l.LoadOrUpdateVocs(voc[1]);
            Assert.IsTrue(GetWordTitles().Contains("qwe"));
        }
        [TestMethod]
        public void ChangeTemplateForExistingWordUpdateVoc()
        {
            // есть шаблон для слова w[6] 
            l.LoadOrUpdateVocs(voc[2]);
            Assert.IsTrue(w[6].Vocabularies.Contains(voc[2]));
            Assert.IsTrue(voc[2].Words.Count() == voc[2].WordTemplates.Count());

            // меняем шаблон в словаре, обновляем словарь, слово больше не в словаре
            wTemp[4].Title = "asdf";
            l.LoadOrUpdateVocs(voc[2]);

            Assert.IsFalse(w[6].Vocabularies.Contains(voc[2]));
            Assert.IsTrue(voc[2].Words.Count() == voc[2].WordTemplates.Count());
        }


        [TestMethod]
        public void ChangeUsedTemplateUpdateVoc()
        {
            l.LoadOrUpdateVocs(voc[2]);
            var word4 = voc[2].Words.Where(x => x.Title == wTemp[4].Title).Single();
            Assert.IsTrue(word4.Vocabularies.Contains(voc[2]));

            // используем слово
            var hr = new HealthRecord(a[1], d1);
            hr.SetItems(new[] { word4 });
            session.SaveOrUpdate(hr);

            // меняем шаблон в словаре, обновляем словарь, слово больше не в словаре
            wTemp[4].Title = "asdf";
            l.LoadOrUpdateVocs(voc[2]);
            Assert.IsFalse(word4.Vocabularies.Contains(voc[2]));
            // создано новое слово
            var word4new = voc[2].Words.Where(x => x.Title == wTemp[4].Title).Single();
            Assert.IsTrue(word4new.Vocabularies.Contains(voc[2]));
        }

        [TestMethod]
        public void ChangeUnusedTemplateUpdateVoc()
        {
            // если слово по шаблону не использовано и у него один словарь - поменять текст слова (это новое слово)
            l.LoadOrUpdateVocs(voc[1]);
            var unused = voc[1].Words.FirstOrDefault(x => x.Title == wTemp[1].Title);
            Assert.IsNotNull(unused);
            Assert.AreEqual(0, unused.HealthRecords.Count());
            Assert.AreEqual(1, unused.Vocabularies.Count());

            wTemp[1].Title = "qwe";
            l.LoadOrUpdateVocs(voc[1]);
            var unused2 = voc[1].Words.FirstOrDefault(x => x.Title == wTemp[1].Title);
            Assert.IsNotNull(unused2);
            Assert.AreNotEqual(unused2, unused);
        }
        [TestMethod]
        public void ChangeTemplateCaseUpdateVoc()
        {
            l.LoadOrUpdateVocs(voc[2]);
            Assert.IsTrue(w[6].Vocabularies.Contains(voc[2]));
            Assert.IsTrue(voc[2].Words.Count() == voc[2].WordTemplates.Count());

            // меняем регистр шаблона в словаре, обновляем словарь, слово больше не в словаре
            wTemp[4].Title = wTemp[4].Title.ToUpper();
            l.LoadOrUpdateVocs(voc[2]);

            Assert.IsFalse(w[6].Vocabularies.Contains(voc[2]));
            Assert.IsTrue(voc[2].Words.Count() == voc[2].WordTemplates.Count());
        }

        [TestMethod]
        public void RemoveVoc()
        {
            // при удалении остаются использованные слова и слова из других словарей
            CreateWordAsInEditor(wTemp[1].Title);
            l.LoadOrUpdateVocs(voc[1]);
            l.DeleteVocs(voc[1]);

            var wordTitles = GetWordTitles();
            Assert.IsTrue(wordTitles.Contains(wTemp[1].Title));
        }

        [TestMethod]
        public void Sequence()
        {
            // новое слово
            CreateWordAsInEditor(wTemp[1].Title);

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
            d1.CustomVocabulary.AddWord(word3);
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
            // снова есть слово 3, слово по 5 шаблону не создается повторно
            wordTitles = GetWordTitles();
            Assert.IsTrue(voc[2].Words.Where(x => x.Title == wTemp[3].Title).Count() > 0);
            Assert.AreEqual(1, wordTitles.Count(x => x == wTemp[5].Title));

            // используем слово 4
            var word4 = voc[2].Words.Where(x => x.Title == wTemp[4].Title).Single();
            var w4Title = word4.Title;
            var hr = new HealthRecord(a[1], d1);
            hr.SetItems(new[] { word4 });
            session.SaveOrUpdate(hr);

            Assert.IsTrue(word4.HealthRecords.Count() > 0);

            // менем шаблон 4, добавляем шаблон 5
            wTemp[4].Title = "poiuy";
            voc[2].AddTemplates(new[] { "555" });
            session.SaveOrUpdate(voc[2]);

            wordTitles = GetWordTitles();
            Assert.AreEqual(5, voc[2].WordTemplates.Count());
            Assert.AreEqual(0, wordTitles.Count(x => x == "555"));//

            // обновляем 2 словарь, снова есть 2 и 3, слово по 4 шаблону не меняется, есть слово по новому шаблону
            l.LoadOrUpdateVocs(voc[2]);
            wordTitles = GetWordTitles();
            Assert.IsTrue(wordTitles.Contains(wTemp[2].Title));
            Assert.IsTrue(wordTitles.Contains(wTemp[3].Title));
            Assert.AreEqual(w4Title, word4.Title);
        }

        [TestMethod]
        public void RemoveUpdateCustom()
        {
            // не имеет смысла удалять/обновлять пользовательские словари
            var newW = CreateWordAsInEditor("123");
            var words = d1.CustomVocabulary.Words.ToList();
            Assert.IsTrue(words.Contains(newW));

            l.DeleteVocs(d1.CustomVocabulary);
            Assert.IsTrue(words.ScrambledEquals(d1.CustomVocabulary.Words));

            l.LoadOrUpdateVocs(d1.CustomVocabulary);
            Assert.IsTrue(words.ScrambledEquals(d1.CustomVocabulary.Words));
        }        
    }
}