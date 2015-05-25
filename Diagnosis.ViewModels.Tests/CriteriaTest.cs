using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
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
            var crg = est[1].CriteriaGroups.Last();
            Assert.AreEqual(crg, crit.Navigator.Current.Crit);
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
        [TestMethod]
        public void Close_CurrentNull()
        {
            crit.Open(cgr[1]);
            crit.CurrentEditor.CancelCommand.Execute(null);

            Assert.AreEqual(null, crit.Navigator.Current);
            Assert.AreEqual(null, crit.CurrentEditor);
        }
        [TestMethod]
        public void Close_TitleEmpty()
        {
            crit.Open(cgr[1]);
            crit.CurrentEditor.CancelCommand.Execute(null);
            Assert.IsTrue(crit.Title.IsNullOrEmpty());

        }

        [TestMethod]
        public void OpenOther_TitleNotEmpty()
        {
            crit.Open(cgr[1]);
            crit.Open(est[1]);

            Assert.IsTrue(!crit.Title.IsNullOrEmpty());

        }
        #endregion Opening

        #region Saving

        [TestMethod]
        public void Cancel_DeleteInvalid_Criterion()
        {
            crit.Open(cgr[1]);
            crit.Navigator.Current.AddCriterionCommand.Execute(null);
            var newCrit = cgr[1].Criteria.Last();

            Assert.IsFalse(crit.CurrentEditor.CanOk);

            crit.CurrentEditor.CancelCommand.Execute(null);

            Assert.IsFalse(cgr[1].Criteria.Contains(newCrit));
        }

        [TestMethod]
        public void OpenOther_DeleteInvalid_Criterion()
        {
            crit.Open(cgr[1]);
            crit.Navigator.Current.AddCriterionCommand.Execute(null);
            var newCrit = cgr[1].Criteria.Last();

            Assert.IsFalse(crit.CurrentEditor.CanOk);

            crit.Open(est[1]);
            Assert.IsFalse(cgr[1].Criteria.Contains(newCrit));
        }

        [TestMethod]
        public void OpenOther_DeleteInvalid_Estimator()
        {
            crit.AddCommand.Execute(null);
            var newCrit = (crit.CurrentEditor as ICritKeeper).Crit;

            crit.Open(cgr[1]);

            Assert.IsTrue(crit.Navigator.FindItemVmOf(newCrit) == null);
        }

        [TestMethod]
        public void SaveNewWordsFromCriterionQueryEditor()
        {
            Load<Doctor>();
            AuthorityController.TryLogIn(d1);

            crit.Open(cr[1]);
            var w = new Word("1");
            (crit.CurrentEditor as CriterionEditorViewModel).QueryEditor.AddTag(w);
            crit.CurrentEditor.OkCommand.Execute(null);

            Assert.IsTrue(!w.IsTransient);
        }

        [TestMethod]
        public void SaveNewWordsFromEstimatorQueryEditor()
        {
            Load<Doctor>();
            AuthorityController.TryLogIn(d1);

            crit.Open(est[1]);
            var w = new Word("1");
            (crit.CurrentEditor as EstimatorEditorViewModel).QueryEditor.AddTag(w);
            crit.CurrentEditor.OkCommand.Execute(null);

            Assert.IsTrue(!w.IsTransient);
        }


        [TestMethod]
        public void DoctorCanUseNewWordsFromCritQueryEditor()
        {
            Load<Doctor>();
            AuthorityController.TryLogIn(d1);

            crit.Open(cr[1]);
            var w = new Word("1");
            (crit.CurrentEditor as CriterionEditorViewModel).QueryEditor.AddTag(w);
            crit.CurrentEditor.OkCommand.Execute(null);

            Assert.IsTrue(d1.Words.Contains(w));

            crit.Open(est[1]);
            var w2 = new Word("2");
            (crit.CurrentEditor as EstimatorEditorViewModel).QueryEditor.AddTag(w2);
            crit.CurrentEditor.OkCommand.Execute(null);

            Assert.IsTrue(d1.Words.Contains(w2));
        }


        [TestMethod]
        public void CopyNewWord_Save_Remove_PasteTransient_GetFromDb()
        {
            Load<Doctor>();
            AuthorityController.TryLogIn(d1);

            var w = new Word("11");
            crit.Open(cr[1]);
            var auto = (crit.CurrentEditor as CriterionEditorViewModel).QueryEditor.QueryBlocks[0].AutocompleteAll as AutocompleteViewModel;

            // copy
            auto.SelectedTag = auto.AddTag(w);
            auto.Copy();

            // save
            crit.CurrentEditor.OkCommand.Execute(null);

            // remove (необязательно)
            crit.Open(cr[1]);
            auto = (crit.CurrentEditor as CriterionEditorViewModel).QueryEditor.QueryBlocks[0].AutocompleteAll as AutocompleteViewModel;
            auto.LastTag.DeleteCommand.Execute(null);
            crit.CurrentEditor.OkCommand.Execute(null);

            // paste
            crit.Open(cr[1]);
            auto = (crit.CurrentEditor as CriterionEditorViewModel).QueryEditor.QueryBlocks[0].AutocompleteAll as AutocompleteViewModel;
            auto.Paste(); // достаем из БД по тексту

            // can save
            crit.CurrentEditor.OkCommand.Execute(null);

            Assert.AreEqual(w, auto.Tags[0].Blank);
        }
        #endregion Saving
    }
}