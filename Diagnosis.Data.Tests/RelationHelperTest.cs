using Diagnosis.Common;
using Diagnosis.Data.Sync;
using Diagnosis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Diagnosis.Data.Tests
{
    [TestClass]
    public class RelationHelperTest
    {
        [TestMethod]
        public void ParentChildOrder()
        {
            var types = Scopes.GetTypes(Scope.Reference)
                .Union(new[] { typeof(HrItem), typeof(Vocabulary), typeof(VocabularyWords) })
                .OrderBy(x => x, new RefModelsComparer())
                .ToList();

            System.Console.WriteLine(string.Join("\n", types));
            // сначала дети
            Assert.IsTrue(
                types.IndexOf(typeof(VocabularyWords)) <
                types.IndexOf(typeof(Vocabulary)));

            Assert.IsTrue(
                types.IndexOf(typeof(Uom)) <
                types.IndexOf(typeof(HrItem)));

            Assert.IsTrue(
                types.IndexOf(typeof(UomFormat)) <
                types.IndexOf(typeof(Uom)));

            Assert.IsTrue(
                types.IndexOf(typeof(Uom)) <
                types.IndexOf(typeof(UomType)));
        }
    }
}