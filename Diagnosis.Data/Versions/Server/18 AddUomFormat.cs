using Diagnosis.Common;
using FluentMigrator;
using System;
using System.Linq;

namespace Diagnosis.Data.Versions.Server
{
    [Migration(201505041200)]
    public class AddUomFormat : SyncronizedMigration
    {
        // описание формата для преобразования значения измерения

        public AddUomFormat()
            : base(Constants.SqlCeProvider)
        {
        }

        public override string[] UpTables
        {
            get
            {
                return new[] { Names.UomFormat, Names.Uom };
            }
        }

        public override void Up()
        {
            Execute.Sql((@"CREATE TABLE {0} (
                Id uniqueidentifier NOT NULL DEFAULT NEWID() PRIMARY KEY,
                {1} uniqueidentifier NOT NULL,
                {2} nvarchar(50) NOT NULL,
                {3} numeric(18,3) NOT NULL,
                CONSTRAINT {4} FOREIGN KEY ({1}) REFERENCES {5} (Id) ON UPDATE NO ACTION ON DELETE NO ACTION )")
                .FormatStr(Names.UomFormat, Names.Id.Uom, Names.Col.UomFStr, Names.Col.UomFValue, Names.FK.UomFormat_Uom, Names.Uom));

        }

        public override void Down()
        {

        }
    }
}