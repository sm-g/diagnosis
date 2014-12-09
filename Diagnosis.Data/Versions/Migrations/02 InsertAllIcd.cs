using FluentMigrator;

namespace Diagnosis.Data.Versions
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
            Delete.FromTable(Names.IcdDiseaseTbl).AllRows();
            Delete.FromTable(Names.IcdBlockTbl).AllRows();
            Delete.FromTable(Names.IcdChapterTbl).AllRows();
        }
    }
}