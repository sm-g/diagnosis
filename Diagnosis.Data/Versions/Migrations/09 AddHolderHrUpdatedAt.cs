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
            Alter.Table(Names.Appointment)
                .AddColumn(CreatedAt).AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .AddColumn(UpdatedAt).AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);
            Alter.Table(Names.Course)
                .AddColumn(CreatedAt).AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .AddColumn(UpdatedAt).AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);
            Alter.Table(Names.Patient)
                .AddColumn(CreatedAt).AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .AddColumn(UpdatedAt).AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

            Alter.Table(Names.HealthRecord)
                .AddColumn(UpdatedAt).AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

            Execute.Sql(string.Format("UPDATE {0} SET {1} = {2}", Names.HealthRecord, UpdatedAt, CreatedAt));
            Execute.Sql(string.Format("UPDATE {0} SET {1} = {3}, {2} = {3}", Names.Appointment, UpdatedAt, CreatedAt, defaultSqlCe));
            Execute.Sql(string.Format("UPDATE {0} SET {1} = {3}, {2} = {3}", Names.Course, UpdatedAt, CreatedAt, defaultSqlCe));
            Execute.Sql(string.Format("UPDATE {0} SET {1} = {3}, {2} = {3}", Names.Patient, UpdatedAt, CreatedAt, defaultSqlCe));
        }

        public override void Down()
        {
            Delete.Column(UpdatedAt)
               .FromTable(Names.Appointment);
            Delete.Column(UpdatedAt)
               .FromTable(Names.Course);
            Delete.Column(UpdatedAt)
              .FromTable(Names.Patient);
            Delete.Column(CreatedAt)
               .FromTable(Names.Appointment);
            Delete.Column(CreatedAt)
               .FromTable(Names.Course);
            Delete.Column(CreatedAt)
              .FromTable(Names.Patient);

            Delete.Column(UpdatedAt)
              .FromTable(Names.HealthRecord);
        }
    }
}