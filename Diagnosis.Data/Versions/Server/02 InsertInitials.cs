using Diagnosis.Data.NHibernate;
using FluentMigrator;

namespace Diagnosis.Data.Versions.Server
{
    [Migration(201504140000)]
    public class InsertInitials : SyncronizedMigration
    {
        public override string[] UpTables
        {
            get
            {
                return new[] {
                    Names.IcdBlock, Names.IcdChapter, Names.IcdDisease,
#if DEBUG
                    Names.Uom, Names.UomType, 
                    Names.Speciality, Names.SpecialityIcdBlocks, 
                    Names.HrCategory, 
                    Names.Vocabulary, Names.WordTemplate, Names.SpecialityVocabularies
#endif
                };
            }
        }
        public override void Up()
        {
#if DEBUG
            foreach (var sqlRow in InMemoryHelper.GetScript(true, true))
            {
                Execute.Sql(sqlRow);
            }
#else
            Execute.EmbeddedScript("insert_Icd.sql");
#endif
        }

        public override void Down()
        {
        }
    }
}