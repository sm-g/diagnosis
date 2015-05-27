using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using NHibernate.Linq;
using System.Collections.Generic;
using NHibernate;

namespace Diagnosis.Tests
{
    [TestClass]
    public abstract class InMemoryDatabaseTest : DbTest
    {
        private EventAggregator.EventMessageHandler handler;
        [TestInitialize]
        public void InMemoryDatabaseTestInit()
        {
            Diagnosis.Data.Mappings.MappingHelper.Reset();
            NHibernateHelper.Default.InMemory = true;
            NHibernateHelper.Default.ShowSql = true;
            NHibernateHelper.Default.FromTest = true;
            Constants.IsClient = true;

            session = NHibernateHelper.Default.OpenSession();

            handler = this.Subscribe(Event.NewSession, (e) =>
            {
                var s = e.GetValue<ISession>(MessageKeys.Session);
                if (session.SessionFactory == s.SessionFactory)
                {
                    session = s;
                }
            });
        }

        [TestCleanup]
        public void InMemoryDatabaseTestCleanup()
        {
            session.Dispose();
            handler.Dispose();
        }

        protected Word CreateWordAsInEditor(string title)
        {
            var dbWords = session.Query<Word>().ToList();
            var newW = new Word(title);
            var toSave = dbWords
                .Where(w => w.Title == newW.Title && w != newW)
                .FirstOrDefault() ?? newW;

            AuthorityController.CurrentDoctor.AddWords(toSave.ToEnumerable());
            session.DoSave(toSave);
            return toSave;
        }

    }
}