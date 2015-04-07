using FluentMigrator;

namespace Diagnosis.Data.Versions.Client
{
    [Migration(201412080000)]
    public class CreateClientDb : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript("create_client_2015_04_01.sql");
        }

        public override void Down()
        {
        }
    }
}