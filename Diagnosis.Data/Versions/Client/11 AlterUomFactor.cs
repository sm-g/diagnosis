using Diagnosis.Common;
using FluentMigrator;
using System;
using System.Linq;

namespace Diagnosis.Data.Versions.Client.Off
{
    [Migration(201502091200)]
    public class AlterUomFactor : SyncronizedMigration
    {
        public AlterUomFactor()
        {
            Provider = Constants.SqlCeProvider;
        }

        public override string[] UpTables
        {
            get
            {
                return new[] { Names.Uom };
            }
        }

        public override void Up()
        {
            // Fix
            // 'The conversion is not supported. [ Type to convert from (if known) = varbinary, Type to convert to (if known) = float ]'
            // in syncronization
            Execute.Sql("alter table Uom alter column Factor numeric(18,6) NOT NULL");
        }

        public override void Down()
        {
        }
    }
}