﻿using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class VocabularySyncTest : ViewModelTest
    {
        private VocabularySyncViewModel vl;

        [TestInitialize]
        public void Init()
        {
            

            vl = new VocabularySyncViewModel();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (vl != null)
                vl.Dispose();
            vl = null;
        }

        [TestMethod]
        public void DontShowCustomVoc()
        {
            // в списке установленных нет пользовательских
            Assert.IsTrue(vl.Vocs.All(x => !x.voc.IsCustom));
        }
    }
}