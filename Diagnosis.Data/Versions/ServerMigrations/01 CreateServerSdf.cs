using FluentMigrator;

namespace Diagnosis.Data.Versions.Server
{
    [Migration(201504040000)]
    public class CreateServerSdf : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript("create_sdf_server.sql");
        }

        public override void Down()
        {
        }
    }
}