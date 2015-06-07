using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class TagTest : ViewModelTest
    {
        private new AutocompleteViewModel a;

        [TestInitialize]
        public void AutocompleteTestInit()
        {
            
            var r = new SuggestionsMaker(session, AuthorityController.CurrentDoctor, clearCreated: true);
            a = new HrEditorAutocomplete(r);
        }

        [TestCleanup]
        public void Clean()
        {
            if (a != null)
                a.Dispose();
        }

        [TestMethod]
        public void CanNotConvertEmptyToWordOrComment()
        {
            var t = new TagViewModel(a);
            t.Query = "";

            foreach (BlankType item in System.Enum.GetValues(typeof(BlankType)))
            {
                if (item == BlankType.None) continue;
                Assert.IsTrue(a.WithConvertTo(item));
                if (item == BlankType.Word || item == BlankType.Comment)
                    Assert.IsFalse(t.ConvertToCommand.CanExecute(item));
                else
                    Assert.IsTrue(t.ConvertToCommand.CanExecute(item));
            }
        }
    }
}