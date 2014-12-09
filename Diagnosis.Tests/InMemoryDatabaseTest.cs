using Diagnosis.Data;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate;
using System;

namespace Tests
{
    [TestClass]
    public abstract class InMemoryDatabaseTest
    {
        protected ISession session;

        public InMemoryDatabaseTest()
        {
            NHibernateHelper.InMemory = true;
            NHibernateHelper.ShowSql = true;
            NHibernateHelper.FromTest = true;
        }

        [TestInitialize]
        public void InMemoryDatabaseTestInit()
        {
            session = NHibernateHelper.OpenSession();
        }

        [TestCleanup]
        public void InMemoryDatabaseTestCleanup()
        {
            session.Dispose();
        }

        protected Guid IntToGuid<T>(int id) where T : EntityBase<Guid>
        {
            if (typeof(T) == typeof(Word))
            {
                return Guid.Parse(string.Format("00000{0:000}-0000-0000-0001-000000000{0:000}", id));
            }
            else if (typeof(T) == typeof(Doctor))
            {
                return Guid.Parse(string.Format("00000{0:000}-1000-0000-0000-000000000{0:000}", id));
            }
            else if (typeof(T) == typeof(Patient))
            {
                return Guid.Parse(string.Format("00000{0:000}-2000-0000-0000-000000000{0:000}", id));
            }
            else if (typeof(T) == typeof(Course))
            {
                return Guid.Parse(string.Format("00000{0:000}-3000-0000-0000-000000000{0:000}", id));
            }
            else if (typeof(T) == typeof(Appointment))
            {
                return Guid.Parse(string.Format("00000{0:000}-4000-0000-0000-000000000{0:000}", id));
            }
            else if (typeof(T) == typeof(HealthRecord))
            {
                return Guid.Parse(string.Format("00000{0:000}-5000-0000-0000-000000000{0:000}", id));
            }
            else if (typeof(T) == typeof(HrItem))
            {
                return Guid.Parse(string.Format("00000{0:000}-6000-0000-0000-000000000{0:000}", id));
            }
            throw new ArgumentOutOfRangeException();
        }
    }
}