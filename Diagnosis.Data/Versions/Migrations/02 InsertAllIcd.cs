using FluentMigrator;

namespace Diagnosis.Data.Versions.Off
{
    [Migration(201412080001)]
    public class InsertAllIcd : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript("insert_Icd.sql");
        }

        public override void Down()
        {
            Delete.FromTable(Names.IcdDisease).AllRows();
            Delete.FromTable(Names.IcdBlock).AllRows();
            Delete.FromTable(Names.IcdChapter).AllRows();
        }
    }
}