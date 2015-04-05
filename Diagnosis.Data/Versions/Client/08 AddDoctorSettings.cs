using FluentMigrator;
using System;
using System.Linq;

namespace Diagnosis.Data.Versions.Client.Off
{
    [Migration(201501141200)]
    public class AddDoctorSettings : Migration
    {

        private const string Title = "Title";
        private const string Value = "Value";
        private const string DoctorSettings = "Settings";

        public override void Up()
        {
            Create.Table(Names.Setting)
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey("PK__Setting")
                .WithColumn(Title).AsString().NotNullable()
                .WithColumn(Value).AsString().NotNullable()
                .WithColumn(Names.Id.Doctor).AsGuid().Nullable();

            Create.ForeignKey(Names.FK.Setting_Doctor).FromTable(Names.Setting)
               .ForeignColumn(Names.Id.Doctor)
               .ToTable(Names.Doctor)
               .PrimaryColumn("Id");

            Delete.Column(DoctorSettings)
                .FromTable(Names.Doctor);
        }

        public override void Down()
        {
            Delete.ForeignKey(Names.FK.Setting_Doctor).OnTable(Names.Setting);
            Delete.Table(Names.Setting);

            Alter.Table(Names.Doctor)
                .AddColumn(DoctorSettings).AsInt16().NotNullable().WithDefaultValue(0);
        }
    }
}