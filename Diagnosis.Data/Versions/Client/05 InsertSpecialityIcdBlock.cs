using FluentMigrator;

namespace Diagnosis.Data.Versions.Client.Off
{
    [Migration(201412080004)]
    public class InsertSpecialityIcdBlock : Migration
    {
        public override void Up()
        {
            // I00-I99 blocks для специальноcти Кардиолог
            for (int block = 91; block <= 100; block++)
                Insert.IntoTable(Names.SpecialityIcdBlocks).Row(new { SpecialityID = 1, IcdBlockID = block });
        }

        public override void Down()
        {
            Delete.FromTable(Names.SpecialityIcdBlocks).AllRows();
        }
    }
}