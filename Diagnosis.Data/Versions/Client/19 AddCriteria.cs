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
                return new[] { Names.Crit, Names.CritWords };
            }
        }

        public override void Up()
        {
            Execute.Sql(@"CREATE TABLE {0} (
                Id uniqueidentifier NOT NULL DEFAULT NEWID() PRIMARY KEY,
                {1} uniqueidentifier NULL,
                {3} nvarchar(255) NOT NULL,
                Description nvarchar(2000) NOT NULL,
                Code nvarchar(50) NULL,
                Options ntext NULL,
                Value nvarchar(50) NULL,
                CONSTRAINT {2} FOREIGN KEY ({1}) REFERENCES {0} (Id) ON UPDATE NO ACTION ON DELETE NO ACTION
                )"
               .FormatStr(Names.Crit, Names.Id.CritParent, Names.FK.Crit_Crit,
                Names.Col.CritType));

            //            Execute.Sql(@"CREATE TABLE {0} (
            //                Id uniqueidentifier NOT NULL DEFAULT NEWID() PRIMARY KEY,
            //                Description nvarchar(2000) NOT NULL,
            //                HeaderHrsOptions ntext NULL
            //                )"
            //                .FormatStr(Names.Estimator));

            //            Execute.Sql(@"CREATE TABLE {0} (
            //                Id uniqueidentifier NOT NULL DEFAULT NEWID() PRIMARY KEY,
            //                {1} uniqueidentifier NOT NULL,
            //                Description nvarchar(2000) NOT NULL,
            //                CONSTRAINT {2} FOREIGN KEY ({1}) REFERENCES {3} (Id) ON UPDATE NO ACTION ON DELETE NO ACTION
            //                )"
            //                .FormatStr(Names.CriteriaGroup, Names.Id.Estimator, Names.FK.CrGr_Est, Names.Estimator));

            //            Execute.Sql(@"CREATE TABLE {0} (
            //                Id uniqueidentifier NOT NULL DEFAULT NEWID() PRIMARY KEY,
            //                {1} uniqueidentifier NOT NULL,
            //                Description nvarchar(2000) NOT NULL,
            //                Code nvarchar(50) NULL,
            //                Options ntext NOT NULL,
            //                Value nvarchar(50) NOT NULL,
            //                CONSTRAINT {2} FOREIGN KEY ({1}) REFERENCES {3} (Id) ON UPDATE NO ACTION ON DELETE NO ACTION
            //                )"
            //                .FormatStr(Names.Criterion, Names.Id.CriteriaGroup, Names.FK.Criterion_CritGr, Names.CriteriaGroup));

            Execute.Sql(@"CREATE TABLE {0} (
                    Id uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID() PRIMARY KEY,
                    {1} uniqueidentifier NOT NULL,
                    {4} uniqueidentifier NOT NULL,
                    CONSTRAINT {2} FOREIGN KEY ({1}) REFERENCES {3} (Id) ON UPDATE NO ACTION ON DELETE NO ACTION,
                    CONSTRAINT {5} FOREIGN KEY ({4}) REFERENCES {6} (Id) ON UPDATE NO ACTION ON DELETE NO ACTION
                    )"
                .FormatStr(
                  Names.CritWords,
                  Names.Id.Crit, Names.FK.CritWord_Crit, Names.Crit,
                  Names.Id.Word, Names.FK.CritWord_Word, Names.Word));
        }

        public override void Down()
        {
        }
    }
}