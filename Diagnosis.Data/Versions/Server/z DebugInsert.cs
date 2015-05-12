using Diagnosis.Common;
using Diagnosis.Data.NHibernate;
using FluentMigrator;

namespace Diagnosis.Data.Versions.Server
{
#if DEBUG
    [Migration(209904140000)]
    public class DebugInsert : SyncronizedMigration
    {
        public DebugInsert() :
            base(Constants.SqlCeProvider)
        { }

        public override string[] UpTables
        {
            get
            {
                return new[] {
                    Names.IcdBlock, Names.IcdChapter, Names.IcdDisease,
                    Names.Uom, Names.UomType, Names.UomFormat,
                    Names.Speciality, Names.SpecialityIcdBlocks,
                    Names.HrCategory,
                    Names.Vocabulary, Names.WordTemplate, Names.SpecialityVocabularies
                };
            }
        }

        public override void Up()
        {
            foreach (var sqlRow in InMemoryHelper.GetScript(true, true))
            {
                Execute.Sql(sqlRow);
            }

        }

        public override void Down()
        {
        }
    }
#endif
}