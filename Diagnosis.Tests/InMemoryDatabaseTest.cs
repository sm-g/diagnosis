using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Diagnosis.Tests
{
    [TestClass]
    public abstract class InMemoryDatabaseTest : DbTest
    {
        public InMemoryDatabaseTest()
        {
            NHibernateHelper.Default.InMemory = true;
            NHibernateHelper.Default.ShowSql = true;
            NHibernateHelper.Default.FromTest = true;
            Constants.IsClient = true;
        }

        [TestInitialize]
        public void InMemoryDatabaseTestInit()
        {
            session = NHibernateHelper.Default.OpenSession();
        }

        [TestCleanup]
        public void InMemoryDatabaseTestCleanup()
        {
            session.Dispose();
        }

        protected static Word CreateWordInEditor(string title)
        {
            var newW = new Word(title);
            using (var wEditor = new WordEditorViewModel(newW))
            {
                Assert.IsTrue(wEditor.OkCommand.CanExecute(null));
                wEditor.OkCommand.Execute(null);
                return wEditor.saved;
            }
        }
    }
}