using Diagnosis.Common;
using FluentMigrator;
using System;
using System.Linq;

namespace Diagnosis.Data.Versions.Client
{
    [Migration(201505041500)]
    public class AddCriteria : SyncronizedMigration
    {
        public AddCriteria()
            : base(Constants.SqlCeProvider)
        {
        }

        public override string[] UpTables
        {
            get
            {
                return new[] { Names.Criterion, Names.CriteriaGroup, Names.Estimator };
            }
        }

        public override void Up()
        {
            Execute.Sql(@"CREATE TABLE {0} (
                Id uniqueidentifier NOT NULL DEFAULT NEWID() PRIMARY KEY,                
                Description nvarchar(2000) NOT NULL,               
                HeaderHrsOptions ntext NULL 
                )"
                .FormatStr(Names.Estimator));

            Execute.Sql(@"CREATE TABLE {0} (
                Id uniqueidentifier NOT NULL DEFAULT NEWID() PRIMARY KEY,
                {1} uniqueidentifier NOT NULL,
                Description nvarchar(2000) NOT NULL,
                CONSTRAINT {2} FOREIGN KEY ({1}) REFERENCES {3} (Id) ON UPDATE NO ACTION ON DELETE NO ACTION 
                )"
                .FormatStr(Names.CriteriaGroup, Names.Id.Estimator, Names.FK.CrGr_Est, Names.Estimator));

            Execute.Sql(@"CREATE TABLE {0} (
                Id uniqueidentifier NOT NULL DEFAULT NEWID() PRIMARY KEY,
                {1} uniqueidentifier NOT NULL,
                Description nvarchar(2000) NOT NULL,
                Code nvarchar(50) NULL,
                Options ntext NOT NULL,
                Value nvarchar(50) NOT NULL,
                CONSTRAINT {2} FOREIGN KEY ({1}) REFERENCES {3} (Id) ON UPDATE NO ACTION ON DELETE NO ACTION 
                )"
                .FormatStr(Names.Criterion, Names.Id.CriteriaGroup, Names.FK.Criterion_CritGr, Names.CriteriaGroup));

        }

        public override void Down()
        {

        }
    }
}