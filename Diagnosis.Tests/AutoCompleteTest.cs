using Diagnosis.App;
using Diagnosis.Core;
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
        private static WordViewModel word1 = EntityProducers.WordsProducer.Find(w1);
        private static WordViewModel word2 = EntityProducers.WordsProducer.Find(w2);
        private static WordViewModel word3 = EntityProducers.WordsProducer.Find(w3);
        private static SymptomViewModel sym1 = EntityProducers.SymptomsProducer.Create(new[] { word1 });
        private static SymptomViewModel sym12 = EntityProducers.SymptomsProducer.Create(new[] { word1, word2 });
        private static SymptomViewModel sym23 = EntityProducers.SymptomsProducer.Create(new[] { word3, word2 });

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