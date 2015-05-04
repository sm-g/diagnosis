using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class MeasureEditorTest : ViewModelTest
    {
        private MeasureEditorViewModel e;
        private new Word w;

        [TestInitialize]
        public void MeasureEditorTestInit()
        {
            Load<Uom>();
            Load<Doctor>();

            w = session.Get<Word>(IntToGuid<Word>(1));
            e = new MeasureEditorViewModel(false);
        }

        [TestCleanup]
        public void MeasureEditorTestCleanup()
        {
            if (e != null)
                e.Dispose();
        }

        [TestMethod]
        public void DataConditions()
        {
            Assert.AreEqual(2, uom[2].Formats.Count());
        }

        [TestMethod]
        public void OpenEmpty()
        {
            Assert.AreEqual(false, e.CanOk);
        }

        [TestMethod]
        public void FillMinimum()
        {
            e.Word = w;
            e.Value = "1";
            Assert.AreEqual(true, e.CanOk);
        }

        [TestMethod]
        public void FormattedValue()
        {
            e.Word = w;
            e.Value = uom[2].Formats.ElementAt(0).String;
            e.Uom = uom[2];
            Assert.AreEqual(true, e.CanOk);
        }
    }
}