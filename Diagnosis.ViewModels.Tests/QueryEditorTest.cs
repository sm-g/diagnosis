using Diagnosis.Models;
using Diagnosis.ViewModels.Search;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class QueryEditorTest : ViewModelTest
    {
        private QueryEditorViewModel e;

        [TestInitialize]
        public void Init()
        {
            e = new QueryEditorViewModel(session);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (e != null)
                e.Dispose();
        }

        [TestMethod]
        public void LoadOptions_SelectChild_GetOptions()
        {
            var opt = new SearchOptions() { MinAny = 2 };
            opt.Children.Add(new SearchOptions() { MinAny = 3 });

            e.SetOptions(opt);
            var lastChild = e.QueryBlocks[0].Children.Last();

            Assert.IsTrue(lastChild.Parent.IsExpanded);

            lastChild.IsSelected = true;

            var forSearch = e.GetOptions();
            Assert.AreEqual(opt, forSearch);
        }
    }
}