using FluentMigrator;
using System;
using System.Linq;

namespace Diagnosis.Data.Versions
{
    [Migration(201501141200)]
    public class AddDoctorSettings : Migration
    {
        private const string FK_Setting_Doctor = "FK_Setting_Doctor";
        private const string Title = "Title";
        private const string Value = "Value";
        private const string DoctorId = Names.Id.Doctor;
        private const string DoctorSettings = "Settings";

        public override void Up()
        {
            Create.Table(Names.Setting)
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey("PK__Setting")
                .WithColumn(Title).AsString().NotNullable()
                .WithColumn(Value).AsString().NotNullable()
                .WithColumn(DoctorId).AsGuid().Nullable();

            Create.ForeignKey(FK_Setting_Doctor).FromTable(Names.Setting)
               .ForeignColumn(DoctorId)
               .ToTable(Names.Doctor)
               .PrimaryColumn("Id");

            Delete.Column(DoctorSettings)
                .FromTable(Names.Doctor);
        }

        public override void Down()
        {
            Delete.ForeignKey(FK_Setting_Doctor).OnTable(Names.Setting);
            Delete.Table(Names.Setting);

            Alter.Table(Names.Doctor)
                .AddColumn(DoctorSettings).AsInt16().NotNullable().WithDefaultValue(0);
        }
    }
}