using FluentMigrator;

namespace Diagnosis.Data.Versions
{
    [Migration(201412080000)]
    public class CreateClientDb : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript("create_client.sql");
        }

        public override void Down()
        {
        }
    }
}