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
        private const string DoctorId = "DoctorID";
        private const string DoctorSettings = "Settings";

        public override void Up()
        {
            Create.Table(Names.SettingTbl)
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey("PK__Setting")
                .WithColumn(Title).AsString().NotNullable()
                .WithColumn(Value).AsString().NotNullable()
                .WithColumn(DoctorId).AsGuid().Nullable();

            Create.ForeignKey(FK_Setting_Doctor).FromTable(Names.SettingTbl)
               .ForeignColumn(DoctorId)
               .ToTable(Names.DoctorTbl)
               .PrimaryColumn("Id");

            Delete.Column(DoctorSettings)
                .FromTable(Names.DoctorTbl);
        }

        public override void Down()
        {
            Delete.ForeignKey(FK_Setting_Doctor).OnTable(Names.SettingTbl);
            Delete.Table(Names.SettingTbl);

            Alter.Table(Names.DoctorTbl)
                .AddColumn(DoctorSettings).AsInt16().NotNullable().WithDefaultValue(0);
        }
    }
}