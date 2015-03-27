using Diagnosis.Data;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate;
using System;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public abstract class InMemoryDatabaseTest
    {
        protected ISession session;

        protected Dictionary<int, HealthRecord> hr = new Dictionary<int, HealthRecord>();
        protected Dictionary<int, Word> w = new Dictionary<int, Word>();
        protected Dictionary<int, Patient> p = new Dictionary<int, Patient>();
        protected Dictionary<int, Course> c = new Dictionary<int, Course>();
        protected Dictionary<int, Appointment> a = new Dictionary<int, Appointment>();
        protected Dictionary<int, IcdDisease> icd = new Dictionary<int, IcdDisease>();
        protected Dictionary<int, IcdBlock> block = new Dictionary<int, IcdBlock>();
        protected Dictionary<int, HrCategory> cat = new Dictionary<int, HrCategory>();
        protected Dictionary<int, Uom> uom = new Dictionary<int, Uom>();
        protected Dictionary<int, UomType> uomType = new Dictionary<int, UomType>();
        protected Dictionary<int, Vocabulary> voc = new Dictionary<int, Vocabulary>();
        protected Dictionary<int, WordTemplate> wTemp = new Dictionary<int, WordTemplate>();
        protected Dictionary<int, Speciality> spec = new Dictionary<int, Speciality>();

        protected static int[] hrIds = new[] { 1, 2, 20, 21, 22, 30, 31, 32, 40, 70, 71, 72, 73, 74 };
        protected static int[] wIds = new[] { 1, 2, 3, 4, 5, 6, 22, 51, 94 };
        protected static int[] pIds = new[] { 1, 2, 3, 4, 5 };
        protected static int[] cIds = new[] { 1, 2, 3, 4 };
        protected static int[] aIds = new[] { 1, 2, 3, 4, 5 };
        protected static int[] icdIds = new[] { 1, 2, 3, 4 };
        protected static int[] blockIds = new[] { 91, 92 };
        protected static int[] catIds = new[] { 1, 2, 3, 4, 5, 6 };
        protected static int[] uomIds = new[] { 1, 2, 3, 4, 5, 6, 7 };
        protected static int[] uomTypeIds = new[] { 1, 2 };
        protected static int[] vocIds = new[] { 1, 2 };
        protected static int[] wTempIds = new[] { 1, 2, 3, 4, 5, 6, 7 };
        protected static int[] specIds = new[] { 1 };

        public InMemoryDatabaseTest()
        {
            NHibernateHelper.Default.InMemory = true;
            NHibernateHelper.Default.ShowSql = true;
            NHibernateHelper.Default.FromTest = true;
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
            else if (typeof(T) == typeof(HrCategory))
            {
                return Guid.Parse(string.Format("00000{0:000}-7000-0000-0000-000000000{0:000}", id));
            }
            else if (typeof(T) == typeof(UomType))
            {
                return Guid.Parse(string.Format("00000{0:000}-8000-0000-0000-000000000{0:000}", id));
            }
            else if (typeof(T) == typeof(Uom))
            {
                return Guid.Parse(string.Format("00000{0:000}-9000-0000-0000-000000000{0:000}", id));
            }
            else if (typeof(T) == typeof(Speciality))
            {
                return Guid.Parse(string.Format("00000{0:000}-1000-0000-0000-000000000{0:000}", id));
            }
            else if (typeof(T) == typeof(SpecialityIcdBlocks))
            {
                return Guid.Parse(string.Format("00000{0:000}-1100-0000-0000-000000000{0:000}", id));
            }
            else if (typeof(T) == typeof(Vocabulary))
            {
                return Guid.Parse(string.Format("00000{0:000}-1200-0000-0000-000000000{0:000}", id));
            }
            else if (typeof(T) == typeof(WordTemplate))
            {
                return Guid.Parse(string.Format("00000{0:000}-1300-0000-0000-000000000{0:000}", id));
            }
            throw new ArgumentOutOfRangeException();
        }
    }
}