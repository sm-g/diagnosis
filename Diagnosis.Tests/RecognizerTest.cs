using Diagnosis.ViewModels.Search.Autocomplete;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class RecognizerTest
    {
        [TestMethod]
        public void IsMeasure()
        {
            var q = "2";
            Assert.IsTrue(Recognizer.IsMeasure(q));
            q = "2 ml";
            Assert.IsTrue(Recognizer.IsMeasure(q));
            q = "-2";
            Assert.IsTrue(Recognizer.IsMeasure(q));
            q = "-2 ml";
            Assert.IsTrue(Recognizer.IsMeasure(q));
            q = ".2 ml";
            Assert.IsTrue(Recognizer.IsMeasure(q));
            q = ",2 ml";
            Assert.IsTrue(Recognizer.IsMeasure(q));
            q = "0,123";
            Assert.IsTrue(Recognizer.IsMeasure(q));
            q = "120/70";
            Assert.IsTrue(Recognizer.IsMeasure(q));
        }

        [TestMethod]
        public void NotMeasure()
        {
            var q = "ml";
            Assert.IsFalse(Recognizer.IsMeasure(q));
            q = " ml";
            Assert.IsFalse(Recognizer.IsMeasure(q));
            q = "-ml";
            Assert.IsFalse(Recognizer.IsMeasure(q));
            q = ",";
            Assert.IsFalse(Recognizer.IsMeasure(q));
            q = "/12";
            Assert.IsFalse(Recognizer.IsMeasure(q));
        }

        [TestMethod]
        public void SplitMeasureQuery()
        {
            var q = "2 ml";
            var splitted = Recognizer.SplitMeasureQuery(q);
            Assert.AreEqual("2", splitted.Item1);
            Assert.AreEqual("ml", splitted.Item2);

            q = "2 ";
            splitted = Recognizer.SplitMeasureQuery(q);
            Assert.AreEqual("2", splitted.Item1);
            Assert.AreEqual("", splitted.Item2);

            q = "-0.2";
            splitted = Recognizer.SplitMeasureQuery(q);
            Assert.AreEqual("-0.2", splitted.Item1);
            Assert.AreEqual("", splitted.Item2);

            q = "ml";
            splitted = Recognizer.SplitMeasureQuery(q);
            Assert.AreEqual("", splitted.Item1);
            Assert.AreEqual("", splitted.Item2);
        }
    }
}