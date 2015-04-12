using FluentMigrator;
using System;
using System.Linq;

namespace Diagnosis.Data.Versions.Client
{
    [Migration(201504121200)]
    public class AddHrToDateDescribedAt : SyncronizedMigration
    {
        // существующие DescribedAt = createdAt
        public override string[] UpTables
        {
            get
            {
                return new[] { Names.HealthRecord };
            }
        }

        public override void Up()
        {
            Alter.Table(Names.HealthRecord)
                .AddColumn(Names.Col.HrToDay).AsInt16().Nullable()
                .AddColumn(Names.Col.HrToMonth).AsInt16().Nullable()
                .AddColumn(Names.Col.HrToYear).AsInt32().Nullable()
                .AddColumn(Names.Col.HrDescribedAt).AsDateTime().Nullable().WithDefault(SystemMethods.CurrentDateTime);

            Execute.Sql(string.Format("UPDATE {0} SET {1} = {2}", Names.HealthRecord, Names.Col.HrDescribedAt, Names.Col.CreatedAt));
            Execute.Sql(string.Format("alter table {0} alter column {1} datetime NOT NULL", Names.HealthRecord, Names.Col.HrDescribedAt));
        }

        public override void Down()
        {

        }
    }
}