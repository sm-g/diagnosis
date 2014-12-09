using FluentMigrator;

namespace Diagnosis.Data.Versions
{
    [Migration(201412080002)]
    public class InsertFirstWordSet : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript("insert_Word.sql");
        }

        public override void Down()
        {
            Delete.ForeignKey(Names.FKWordWord).OnTable(Names.WordTbl);
            Delete.FromTable(Names.WordTbl).AllRows();
            Create.ForeignKey(Names.FKWordWord).FromTable(Names.WordTbl)
                .ForeignColumn("WordID")
                .ToTable(Names.WordTbl)
                .PrimaryColumn("Id")
                .OnDeleteOrUpdate(System.Data.Rule.None);
        }
    }
}