using Diagnosis.Common;
using Diagnosis.Data.DTOs;
using Diagnosis.Data.NHibernate;
using Diagnosis.Models;
using Diagnosis.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

namespace Diagnosis.Data.Tests
{
    [TestClass]
    public class OptionsLoaderTest : SdfDatabaseTest
    {
        private JsonOptionsLoader l;

        [TestInitialize]
        public void InitOptionsLoader()
        {
            InMemoryHelper.FillData(clCfg, clSession, false);

            Load<Word>();
            Load<Uom>();

            l = new JsonOptionsLoader();
        }

        [TestMethod]
        public void WriteReadEquals()
        {
            Load<HrCategory>();

            var opt = new SearchOptions();
            opt.CWordsAll.AddRange(new[] { w[1].AsConfidencable(), w[2].AsConfidencable() });
            opt.Categories.Add(cat[1]);
            opt.SearchScope = SearchScope.Patient;

            var str = l.WriteOptions(opt);
            var read = l.ReadOptions(str, session);

            Assert.IsTrue(opt.Equals(read));
        }
        [TestMethod]
        public void WriteReadConfidenceEquals()
        {
            var opt = new SearchOptions();
            opt.CWordsAll.AddRange(new[] { w[1].AsConfidencable(Confidence.Absent) });

            var str = l.WriteOptions(opt);
            var read = l.ReadOptions(str, session);

            Assert.IsTrue(opt.Equals(read));
        }
        [TestMethod]
        public void WriteReadWithMeasureEquals()
        {
            var opt = new SearchOptions();
            opt.MeasuresAll.Add(new MeasureOp(MeasureOperator.Equal, 1, uom[1], w[1]));
            opt.MeasuresAll.Add(new MeasureOp(MeasureOperator.Between, 3, uom[2], w[1]) { RightValue = 7 });

            var str = l.WriteOptions(opt);
            var read = l.ReadOptions(str, session);

            Assert.IsTrue(opt.Equals(read));
        }
        [TestMethod]
        public void WriteReadSameWordEquals()
        {
            var opt = new SearchOptions();
            opt.CWordsAll.Add(w[1].AsConfidencable());
            opt.CWordsAny.Add(w[1].AsConfidencable());
            opt.MeasuresAll.Add(new MeasureOp(MeasureOperator.Equal, 1, uom[1], w[1]));
            opt.MeasuresAny.Add(new MeasureOp(MeasureOperator.Between, 3, uom[2], w[1]));

            var str = l.WriteOptions(opt);
            var read = l.ReadOptions(str, session);

            Assert.IsTrue(opt.Equals(read));
        }

        [TestMethod]
        public void WriteReadWithChildEquals()
        {
            var child = new SearchOptions();
            child.MinAny = 2;
            child.MeasuresAny.Add(new MeasureOp(MeasureOperator.Between, 3, uom[2], w[1]) { RightValue = 7 });

            var opt = new SearchOptions();
            opt.Children.Add(child);

            var str = l.WriteOptions(opt);
            var read = l.ReadOptions(str, session);

            Assert.IsTrue(opt.Equals(read));
        }

        [TestMethod]
        public void PartialLoaded()
        {
            var opt = new SearchOptions();
            opt.CWordsAll.AddRange(new[] { w[1].AsConfidencable(), w[2].AsConfidencable() });

            var str = l.WriteOptions(opt).Replace(w[1].Title, "qwe");
            var read = l.ReadOptions(str, session);

            Assert.IsTrue(read.PartialLoaded);
        }

        [TestMethod]
        public void PartialLoadedChild()
        {
            var opt = new SearchOptions();
            var child = new SearchOptions();
            child.CWordsAll.AddRange(new[] { w[1].AsConfidencable(), w[2].AsConfidencable() });
            opt.Children.Add(child);

            var str = l.WriteOptions(opt).Replace(w[1].Title, "qwe");
            var read = l.ReadOptions(str, session);

            Assert.IsTrue(read.PartialLoaded);
            Assert.IsTrue(read.Children[0].PartialLoaded);
        }
        [TestMethod]
        public void PartialLoadedUom()
        {
            var opt = new SearchOptions();
            opt.MeasuresAny.Add(new MeasureOp(MeasureOperator.Between, 3, uom[2]));

            var str = l.WriteOptions(opt).Replace(uom[2].Abbr, "qwe");
            var read = l.ReadOptions(str, session);

            Assert.IsTrue(read.PartialLoaded);
        }
        [TestMethod]
        public void PartialLoadedCat()
        {
            Load<HrCategory>();
            var opt = new SearchOptions();
            opt.Categories.Add(cat[1]);

            var str = l.WriteOptions(opt).Replace(cat[1].Title, "qwe");
            var read = l.ReadOptions(str, session);

            Assert.IsTrue(read.PartialLoaded);
        }
        [TestMethod]
        public void PartialLoadedUomType()
        {
            var opt = new SearchOptions();
            opt.MeasuresAny.Add(new MeasureOp(MeasureOperator.Between, 3, uom[2]));

            var str = l.WriteOptions(opt).Replace(uom[2].Type.Title, "qwe");
            var read = l.ReadOptions(str, session);

            Assert.IsTrue(read.PartialLoaded);
        }
        [TestMethod]
        public void PartialLoadedMeasureWord()
        {
            var opt = new SearchOptions();
            opt.MeasuresAny.Add(new MeasureOp(MeasureOperator.Between, 3, word: w[2]));

            var str = l.WriteOptions(opt).Replace(w[2].Title, "qwe");
            var read = l.ReadOptions(str, session);

            Assert.IsTrue(read.PartialLoaded);
        }
    }
}