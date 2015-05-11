using Diagnosis.Common;
using Diagnosis.Data.NHibernate;
using FluentMigrator;

namespace Diagnosis.Data.Versions.Server // TODO for sql server
{
#if !DEBUG
    [Migration(201504140000)]
    public class InsertInitials : SyncronizedMigration
    {
        public InsertInitials() :
            base(Constants.SqlCeProvider) 
        { }

        public override string[] UpTables
        {
            get
            {
                return new[] {
                    Names.IcdBlock, Names.IcdChapter, Names.IcdDisease,
                };
            }
        }

        public override void Up()
        {
            Execute.EmbeddedScript("insert_Icd.sql");
        }

        public override void Down()
        {
        }
    }
#endif
}