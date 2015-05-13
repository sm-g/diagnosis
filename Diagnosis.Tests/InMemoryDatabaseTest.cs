using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using NHibernate.Linq;
using System.Collections.Generic;

namespace Diagnosis.Tests
{
    [TestClass]
    public abstract class InMemoryDatabaseTest : DbTest
    {
        [TestInitialize]
        public void InMemoryDatabaseTestInit()
        {
            Diagnosis.Data.Mappings.MappingHelper.Reset();
            NHibernateHelper.Default.InMemory = true;
            NHibernateHelper.Default.ShowSql = true;
            NHibernateHelper.Default.FromTest = true;
            Constants.IsClient = true;

            session = NHibernateHelper.Default.OpenSession();
        }

        [TestCleanup]
        public void InMemoryDatabaseTestCleanup()
        {
            session.Dispose();
        }

        protected Word CreateWordAsInEditor(string title)
        {
            var dbWords = session.Query<Word>().ToList();
            var newW = new Word(title);
            var toSave = dbWords
                .Where(w => w.Title == newW.Title && w != newW)
                .FirstOrDefault() ?? newW;

            AuthorityController.CurrentDoctor.AddWords(toSave.ToEnumerable());
            new Saver(session).Save(toSave);
            return toSave;
        }

    }
}