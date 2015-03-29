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
            Delete.ForeignKey(Names.FK.Word_Word).OnTable(Names.Word);
            Delete.FromTable(Names.Word).AllRows();
            Create.ForeignKey(Names.FK.Word_Word).FromTable(Names.Word)
                .ForeignColumn(Names.Id.Word)
                .ToTable(Names.Word)
                .PrimaryColumn("Id")
                .OnDeleteOrUpdate(System.Data.Rule.None);
        }
    }
}