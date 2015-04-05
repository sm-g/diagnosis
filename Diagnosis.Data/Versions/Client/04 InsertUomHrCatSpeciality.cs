using FluentMigrator;

namespace Diagnosis.Data.Versions.Client.Off
{
    [Migration(201412080003)]
    public class InsertUomHrCatSpeciality : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript("insert_Uom_HrCat_Speciality.sql");
        }

        public override void Down()
        {
            Delete.FromTable(Names.Uom).AllRows();
            Delete.FromTable(Names.HrCategory).AllRows();
            Delete.FromTable(Names.Speciality).AllRows();
        }
    }
}