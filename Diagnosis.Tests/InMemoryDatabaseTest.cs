using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.ViewModels.Screens;
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
        protected Doctor d1;
        protected Doctor d2;

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

        private static Dictionary<Type, int[]> ids = new Dictionary<Type, int[]>()
        {
            { typeof(HealthRecord), new[] { 1, 2, 20, 21, 22, 30, 31, 32, 40, 70, 71, 72, 73, 74 } },
            { typeof(Word),         new[] { 1, 2, 3, 4, 5, 6, 22, 51, 94 } },
            { typeof(Patient),      new[] { 1, 2, 3, 4, 5 } },
            { typeof(Course),       new[] { 1, 2, 3, 4 } },
            { typeof(Appointment),  new[] { 1, 2, 3, 4, 5 } },
            { typeof(IcdDisease),   new[] { 1, 2, 3, 4 } },
            { typeof(IcdBlock),     new[] { 91, 92 } },
            { typeof(HrCategory),   new[] { 1, 2, 3, 4, 5, 6 } },
            { typeof(Uom),          new[] { 1, 2, 3, 4, 5, 6, 7 } },
            { typeof(UomType),      new[] { 1, 2 } },
            { typeof(Vocabulary),   new[] { 1, 2 } },
            { typeof(WordTemplate), new[] { 1, 2, 3, 4, 5, 6, 7 } },
            { typeof(Speciality),   new[] { 1 } },
        };

        private static Dictionary<Type, string> guidFormats = new Dictionary<Type, string>()
        {
            { typeof(Word),                 "00000{0:000}-0000-0000-0001-000000000{0:000}" },
            { typeof(Doctor),               "00000{0:000}-1000-0000-0000-000000000{0:000}" },
            { typeof(Patient),              "00000{0:000}-2000-0000-0000-000000000{0:000}" },
            { typeof(Course),               "00000{0:000}-3000-0000-0000-000000000{0:000}" },
            { typeof(Appointment),          "00000{0:000}-4000-0000-0000-000000000{0:000}" },
            { typeof(HealthRecord),         "00000{0:000}-5000-0000-0000-000000000{0:000}" },
            { typeof(HrItem),               "00000{0:000}-6000-0000-0000-000000000{0:000}" },
            { typeof(HrCategory),           "00000{0:000}-7000-0000-0000-000000000{0:000}" },
            { typeof(UomType),              "00000{0:000}-8000-0000-0000-000000000{0:000}" },
            { typeof(Uom),                  "00000{0:000}-9000-0000-0000-000000000{0:000}" },
            { typeof(Speciality),           "00000{0:000}-1000-0000-0000-000000000{0:000}" },
            { typeof(SpecialityIcdBlocks),  "00000{0:000}-1100-0000-0000-000000000{0:000}" },
            { typeof(Vocabulary),           "00000{0:000}-1200-0000-0000-000000000{0:000}" },
            { typeof(WordTemplate),         "00000{0:000}-1300-0000-0000-000000000{0:000}" },
        };

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

        protected void Load<T>()
        {
            var @switch = new Dictionary<Type, Action> {
                { typeof(HealthRecord), () => ids[typeof(T)].ForAll((id) => hr[id] = session.Get<HealthRecord>      (IntToGuid<HealthRecord>(id))) },
                { typeof(Word),         () => ids[typeof(T)].ForAll((id) => w[id] = session.Get<Word>               (IntToGuid<Word>(id))) },
                { typeof(Patient),      () => ids[typeof(T)].ForAll((id) => p[id] = session.Get<Patient>            (IntToGuid<Patient>(id))) },
                { typeof(Course),       () => ids[typeof(T)].ForAll((id) => c[id] = session.Get<Course>             (IntToGuid<Course>(id))) },
                { typeof(Appointment),  () => ids[typeof(T)].ForAll((id) => a[id] = session.Get<Appointment>        (IntToGuid<Appointment>(id))) },
                { typeof(IcdDisease),   () => ids[typeof(T)].ForAll((id) => icd[id] = session.Get<IcdDisease>       (id)) },
                { typeof(IcdBlock),     () => ids[typeof(T)].ForAll((id) => block[id] = session.Get<IcdBlock>       (id)) },
                { typeof(HrCategory),   () => ids[typeof(T)].ForAll((id) => cat[id] = session.Get<HrCategory>       (IntToGuid<HrCategory>(id))) },
                { typeof(Uom),          () => ids[typeof(T)].ForAll((id) => uom[id] = session.Get<Uom>              (IntToGuid<Uom>(id))) },
                { typeof(UomType),      () => ids[typeof(T)].ForAll((id) => uomType[id] = session.Get<UomType>      (IntToGuid<UomType>(id))) },
                { typeof(Vocabulary),   () => ids[typeof(T)].ForAll((id) => voc[id] = session.Get<Vocabulary>       (IntToGuid<Vocabulary>(id))) },
                { typeof(WordTemplate), () => ids[typeof(T)].ForAll((id) => wTemp[id] = session.Get<WordTemplate>   (IntToGuid<WordTemplate>(id))) },
                { typeof(Speciality),   () => ids[typeof(T)].ForAll((id) => spec[id] = session.Get<Speciality>      (IntToGuid<Speciality>(id))) },
            };

            Action act;
            if (@switch.TryGetValue(typeof(T), out act))
                act();
            else if (typeof(T) == typeof(Doctor))
            {
                d1 = session.Get<Doctor>(IntToGuid<Doctor>(1));
                d2 = session.Get<Doctor>(IntToGuid<Doctor>(2));
            }
            else
                throw new NotImplementedException();
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

        protected Guid IntToGuid<T>(int id) where T : EntityBase<Guid>
        {
            string format;
            if (guidFormats.TryGetValue(typeof(T), out format))
                return Guid.Parse(string.Format(format, id));
            throw new ArgumentOutOfRangeException();
        }
    }
}