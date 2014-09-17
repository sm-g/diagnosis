using Diagnosis.App;
using Diagnosis.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using Diagnosis.Core;
using Diagnosis.Data.Queries;
using NHibernate;
using Diagnosis.Data;

namespace Tests
{
    [TestClass]
    public class SymptomsQueryTest : DbTestBase
    {
        [TestMethod]
        public void GetSymptomByWords()
        {
            using (var tx = session.BeginTransaction())
            {
                var symptom = SymptomQuery.ByWords(session)(new Word[] { w1, w2 });

                Assert.AreEqual(s1, symptom);
            }
        }

        [TestMethod]
        public void GetSymptomsWithAnyWord()
        {
            using (var tx = session.BeginTransaction())
            {
                var symptoms = SymptomQuery.WithAnyWord(session)(new Word[] { w1, w2 });

                Assert.IsTrue(symptoms.Count() == 2);
                Assert.IsTrue(symptoms.Contains(s1));
                Assert.IsTrue(symptoms.Contains(s2));

                symptoms = SymptomQuery.WithAnyWord(session)(new Word[] { w1 });

                Assert.IsTrue(symptoms.Count() == 2);
                Assert.IsTrue(symptoms.Contains(s1));
                Assert.IsTrue(symptoms.Contains(s2));
            }
        }
    }
}
