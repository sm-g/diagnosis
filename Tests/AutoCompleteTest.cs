using Diagnosis.App;
using Diagnosis.App.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class AutoCompleteTest
    {
        private const string delim = " ";
        private const string w1 = "анемия";
        private const string w2 = "лежа";
        private const string w3 = "порок сердца";
        private static WordViewModel word1 = EntityManagers.WordsManager.Find(w1);
        private static WordViewModel word2 = EntityManagers.WordsManager.Find(w2);
        private static WordViewModel word3 = EntityManagers.WordsManager.Find(w3);
        private static SymptomViewModel sym1 = EntityManagers.SymptomsManager.Create(new[] { word1 });
        private static SymptomViewModel sym12 = EntityManagers.SymptomsManager.Create(new[] { word1, word2 });
        private static SymptomViewModel sym23 = EntityManagers.SymptomsManager.Create(new[] { word3, word2 });
        private static SearchWrap word1wrap = new SearchWrap(word1);
        private static SearchWrap word3wrap = new SearchWrap(word3);
        private static SearchWrap sym1wrap = new SearchWrap(sym1);
        private static SearchWrap sym12wrap = new SearchWrap(sym12);
        private static SearchWrap sym23wrap = new SearchWrap(sym23);

        #region Filter

        [TestMethod]
        public void TestFilterOneWord()
        {
            Assert.IsTrue(AutoComplete.FilterItem(w1, word1wrap));
            Assert.IsFalse(AutoComplete.FilterItem(w2, word1wrap));
        }

        [TestMethod]
        public void TestFilterOneWordPart()
        {
            Assert.IsTrue(AutoComplete.FilterItem(w1.Substring(0, w1.Length - 2), word1wrap));
        }

        [TestMethod]
        public void TestFilterTwoWords()
        {
            Assert.IsTrue(AutoComplete.FilterItem(w3, word3wrap));
        }

        [TestMethod]
        public void TestFilterWordDuplicates()
        {
            Assert.IsFalse(AutoComplete.FilterItem(w1 + delim + w1, word1wrap));
        }

        [TestMethod]
        public void TestFilterSymptomFirstWord()
        {
            Assert.IsTrue(AutoComplete.FilterItem(w1, sym1wrap));
            Assert.IsTrue(AutoComplete.FilterItem(w1, sym12wrap));
        }

        [TestMethod]
        public void TestFilterSymptomFirstWordAndPart()
        {
            var q = w1 + delim + w2.Substring(0, w2.Length - 2);

            Assert.IsFalse(AutoComplete.FilterItem(q, sym1wrap));
            Assert.IsTrue(AutoComplete.FilterItem(q, sym12wrap));
        }

        [TestMethod]
        public void TestFilterSymptomAllWords()
        {
            var q = w1 + delim + w2;

            Assert.IsFalse(AutoComplete.FilterItem(q, sym1wrap));
            Assert.IsTrue(AutoComplete.FilterItem(q, sym12wrap));
        }

        [TestMethod]
        public void TestFilterSymptomSecondWord()
        {
            Assert.IsTrue(AutoComplete.FilterItem(w2, sym12wrap));
        }

        [TestMethod]
        public void TestFilterSymptomSecondWordPart()
        {
            Assert.IsTrue(AutoComplete.FilterItem(w2.Substring(0, w2.Length - 2), sym12wrap));
        }

        [TestMethod]
        public void TestFilterSymptomComplexWords()
        {
            Assert.IsTrue(AutoComplete.FilterItem(w2 + delim + w3, sym23wrap));
        }

        [TestMethod]
        public void TestFilterSymptomComplexWordsParts()
        {
            Assert.IsTrue(AutoComplete.FilterItem(w2.Substring(0, w2.Length - 2) + delim + w3.Substring(0, w3.Length - 2), sym23wrap));
        }

        #endregion Filter

        #region Partition

        [TestMethod]
        public void TestSequencePart()
        {
            var input = new[] { 1, 2, 3, 4, 5 };
            var result = SequencePartition.Part(input);
            foreach (var item in result)
            {
                Console.WriteLine("");
                foreach (var item1 in item)
                {
                    Console.Write("(");
                    foreach (var item2 in item1)
                    {
                        Console.Write("{0} ", item2);
                    }
                    Console.Write(")");
                }
            }
            Assert.IsTrue(result.Any(l => l.Count() == 3 &&
               l.Any(g => g.All(e => (new[] { 1, 2 }).Contains(e))) &&
               l.Any(g => g.All(e => (new[] { 3 }).Contains(e))) &&
               l.Any(g => g.All(e => (new[] { 4, 5 }).Contains(e)))
           ));
            Assert.IsTrue(result.Count() == 16);
        }

        [TestMethod]
        public void TestStringSequencePart()
        {
            var input = w1 + delim + w2 + delim + w3;
            var result = StringSequencePartition.Part(input);
            foreach (var item in result)
            {
                Console.WriteLine("");
                foreach (var item1 in item)
                {
                    Console.Write("(" + item1 + ')');
                }
            }
            Assert.IsTrue(result.Any(l => l.Count() == 2 &&
               l.Contains(w1) &&
               l.Contains(w2 + delim + w3)
            ));

            Assert.IsTrue(result.Count() == 8);
        }

        [TestMethod]
        public void TestOneWordStringSequencePart()
        {
            var result = StringSequencePartition.Part(w1);
            Assert.IsTrue(result.Count() == 1);
        }
        [TestMethod]
        public void TestTwoWordStringSequencePart()
        {
            var result = StringSequencePartition.Part(w1 + delim + w2);
            Assert.IsTrue(result.Count() == 2);
        }

        #endregion Partition
    }
}