﻿using Diagnosis.Common;
using Diagnosis.Data.Sync;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Diagnosis.Data.Tests
{
    [TestClass]
    public class ScopesTest
    {
        [TestMethod]
        public void GetScope()
        {
            Assert.AreEqual(Scope.Holder, typeof(Appointment).GetScopes().First());
            Assert.AreEqual(Scope.Hr, typeof(HrItem).GetScopes().First());
            Assert.AreEqual(Scope.Icd, typeof(IcdBlock).GetScopes().First());
            Assert.AreEqual(Scope.Reference, typeof(Uom).GetScopes().First());
            Assert.AreEqual(Scope.User, typeof(Doctor).GetScopes().First());
            Assert.AreEqual(Scope.Voc, typeof(Vocabulary).GetScopes().First());

            Assert.IsTrue(typeof(Speciality).GetScopes().Count() > 1);

        }

        [TestMethod]
        public void GetRelated()
        {
            var r = Scope.User.GetRelatedScopes();
            Assert.IsTrue(r.Contains(Scope.Voc));
            Assert.IsTrue(r.Contains(Scope.User));
            Assert.IsTrue(r.Contains(Scope.Reference));

            r = Scope.Voc.GetRelatedScopes();
            Assert.IsTrue(r.Contains(Scope.Voc));
            Assert.IsTrue(r.Contains(Scope.User));
            Assert.IsTrue(r.Contains(Scope.Reference));

            r = Scope.Reference.GetRelatedScopes();
            Assert.IsTrue(r.Contains(Scope.Voc));
            Assert.IsTrue(r.Contains(Scope.User));
            Assert.IsTrue(r.Contains(Scope.Reference));
        }
    }
}