using FluentMigrator;
using System;
using System.Linq;

namespace Diagnosis.Data.Versions
{
    [Migration(201501231200)]
    public class AddHolderHrUpdatedAt : Migration
    {
        private const string UpdatedAt = "UpdatedAt";
        private const string CreatedAt = "CreatedAt";
        private static DateTime DefaultHolderCreatedAt = new DateTime(2014, 1, 1, 0, 0, 0);
        private static string defaultSqlCe = string.Format("'{0}'", DefaultHolderCreatedAt.ToString("yyyy-MM-dd hh:mm:ss"));

        public override void Up()
        {
            Alter.Table(Names.AppointmentTbl)
                .AddColumn(CreatedAt).AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .AddColumn(UpdatedAt).AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);
            Alter.Table(Names.CourseTbl)
                .AddColumn(CreatedAt).AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .AddColumn(UpdatedAt).AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);
            Alter.Table(Names.PatientTbl)
                .AddColumn(CreatedAt).AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .AddColumn(UpdatedAt).AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

            Alter.Table(Names.HealthRecordTbl)
                .AddColumn(UpdatedAt).AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

            Execute.Sql(string.Format("UPDATE {0} SET {1} = {2}", Names.HealthRecordTbl, UpdatedAt, CreatedAt));
            Execute.Sql(string.Format("UPDATE {0} SET {1} = {3}, {2} = {3}", Names.AppointmentTbl, UpdatedAt, CreatedAt, defaultSqlCe));
            Execute.Sql(string.Format("UPDATE {0} SET {1} = {3}, {2} = {3}", Names.CourseTbl, UpdatedAt, CreatedAt, defaultSqlCe));
            Execute.Sql(string.Format("UPDATE {0} SET {1} = {3}, {2} = {3}", Names.PatientTbl, UpdatedAt, CreatedAt, defaultSqlCe));
        }

        public override void Down()
        {
            Delete.Column(UpdatedAt)
               .FromTable(Names.AppointmentTbl);
            Delete.Column(UpdatedAt)
               .FromTable(Names.CourseTbl);
            Delete.Column(UpdatedAt)
              .FromTable(Names.PatientTbl);
            Delete.Column(CreatedAt)
               .FromTable(Names.AppointmentTbl);
            Delete.Column(CreatedAt)
               .FromTable(Names.CourseTbl);
            Delete.Column(CreatedAt)
              .FromTable(Names.PatientTbl);

            Delete.Column(UpdatedAt)
              .FromTable(Names.HealthRecordTbl);
        }
    }
}