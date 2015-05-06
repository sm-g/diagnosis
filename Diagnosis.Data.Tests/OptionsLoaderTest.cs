using Diagnosis.Common;
using Diagnosis.Data.DTOs;
using Diagnosis.Data.NHibernate;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Diagnosis.Data.Tests
{
    [TestClass]
    public class OptionsLoaderTest : SdfDatabaseTest
    {
        [TestInitialize]
        public void InitOptionsLoader()
        {
            InMemoryHelper.FillData(clCfg, clSession, false);

            Load<Word>();
            ModelDtosMapper.Map();
        }

        [TestMethod]
        public void WriteReadEquals()
        {
            Load<HrCategory>();

            var opt = new SearchOptions();
            opt.WordsAll.AddRange(new[] { w[1], w[2] });
            opt.Categories.Add(cat[1]);
            opt.SearchScope = SearchScope.Patient;

            var l = new JsonOptionsLoader(session);
            var str = l.WriteOptions(opt);
            var readed = l.ReadOptions(str);

            Assert.IsTrue(opt.Equals(readed));
        }

        [TestMethod]
        public void WriteReadWithMeasureEquals()
        {
            Load<Uom>();

            var opt = new SearchOptions();
            opt.MeasuresAll.Add(new MeasureOp(MeasureOperator.Equal, 1, uom[1], w[1]));
            opt.MeasuresAll.Add(new MeasureOp(MeasureOperator.Between, 3, uom[2], w[1]) { RightValue = 7 });

            var l = new JsonOptionsLoader(session);
            var str = l.WriteOptions(opt);
            var readed = l.ReadOptions(str);

            Assert.IsTrue(opt.Equals(readed));
        }

        [TestMethod]
        public void WriteReadWithChildEquals()
        {
            Load<Uom>();

            var child = new SearchOptions();
            child.MinAny = 2;
            child.MeasuresAny.Add(new MeasureOp(MeasureOperator.Between, 3, uom[2], w[1]) { RightValue = 7 });

            var opt = new SearchOptions();
            opt.Children.Add(child);

            var l = new JsonOptionsLoader(session);
            var str = l.WriteOptions(opt);
            var readed = l.ReadOptions(str);

            Assert.IsTrue(opt.Equals(readed));
        }
    }
}
