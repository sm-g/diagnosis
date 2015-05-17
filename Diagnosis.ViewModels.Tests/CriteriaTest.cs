﻿using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class CriteriaTest : ViewModelTest
    {
        private CriteriaViewModel crit;

        [TestInitialize]
        public void Init()
        {
            Load<Estimator>();
            Load<CriteriaGroup>();
            Load<Criterion>();

            crit = new CriteriaViewModel();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (crit != null)
                crit.Dispose();
        }

        #region Opening

        [TestMethod]
        public void OpenEst()
        {
            crit.Open(est[1]);
            Assert.AreEqual(est[1], crit.Navigator.Current.Crit);
            Assert.AreEqual(est[1], (crit.CurrentEditor as ICritKeeper).Crit);
        }

        [TestMethod]
        public void OpenCrGr()
        {
            crit.Open(cgr[1]);
            Assert.AreEqual(cgr[1], crit.Navigator.Current.Crit);
            Assert.AreEqual(cgr[1], (crit.CurrentEditor as ICritKeeper).Crit);
        }

        [TestMethod]
        public void OpenCrit()
        {
            crit.Open(cr[1]);
            Assert.AreEqual(cr[1], crit.Navigator.Current.Crit);
            Assert.AreEqual(cr[1], (crit.CurrentEditor as ICritKeeper).Crit);
        }

        [TestMethod]
        public void OpenNewCrGr()
        {
            crit.Open(est[1]);

            crit.Navigator.Current.AddCritGroupCommand.Execute(null);
            var newCrit = cgr[1].Criteria.Last();
            Assert.AreEqual(newCrit, crit.Navigator.Current.Crit);
        }

        [TestMethod]
        public void OpenNewCr()
        {
            crit.Open(cgr[1]);
            crit.Navigator.Current.AddCriterionCommand.Execute(null);

            Assert.AreEqual(cgr[1].Criteria.LastOrDefault(), crit.Navigator.Current.Crit);
        }

        [TestMethod]
        public void Reopen()
        {
            crit.Open(cgr[1]);
            crit.CurrentEditor.CancelCommand.Execute(null);

            Assert.AreEqual(null, crit.Navigator.Current);

            crit.Open(cgr[1]);

            Assert.AreEqual(cgr[1], crit.Navigator.Current.Crit);
            Assert.AreEqual(cgr[1], (crit.CurrentEditor as ICritKeeper).Crit);
        }

        #endregion Opening

        #region Saving

        [TestMethod]
        public void Cancel_DeleteInvalid()
        {
            crit.Open(cgr[1]);
            crit.Navigator.Current.AddCriterionCommand.Execute(null);
            var newCrit = cgr[1].Criteria.Last();

            Assert.IsFalse(crit.CurrentEditor.CanOk);

            crit.CurrentEditor.CancelCommand.Execute(null);

            // open near
            Assert.AreEqual(cr[1], crit.Navigator.Current.Crit);
            Assert.IsFalse(cgr[1].Criteria.Contains(newCrit));
        }

        [TestMethod]
        public void SaveNewWordsFromCriterionQueryEditor()
        {
            crit.Open(cr[1]);
            var w = new Word("1");
            (crit.CurrentEditor as CriterionEditorViewModel).QueryEditor.QueryBlocks[0].AutocompleteAll.AddTag(w);
            crit.CurrentEditor.OkCommand.Execute(null);

            Assert.IsTrue(!w.IsTransient);
        }

        [TestMethod]
        public void SaveNewWordsFromEstimatorQueryEditor()
        {
            crit.Open(est[1]);
            var w = new Word("1");
            (crit.CurrentEditor as EstimatorEditorViewModel).QueryEditor.QueryBlocks[0].AutocompleteAll.AddTag(w);
            crit.CurrentEditor.OkCommand.Execute(null);

            Assert.IsTrue(!w.IsTransient);
        }

        #endregion Saving
    }
}