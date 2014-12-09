using FluentMigrator;

namespace Diagnosis.Data.Versions
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
            Delete.FromTable(Names.UomTbl).AllRows();
            Delete.FromTable(Names.HrCategoryTbl).AllRows();
            Delete.FromTable(Names.SpecialityTbl).AllRows();
        }
    }
}