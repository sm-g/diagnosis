using Diagnosis.Common;
using FluentMigrator;
using System;
using System.Linq;

namespace Diagnosis.Data.Versions.Client
{
    [Migration(201504061200)]
    public class AlterUomTypeTitleLength : SyncronizedMigration
    {
        public AlterUomTypeTitleLength()
        {
            Provider = Constants.SqlCeProvider;
        }

        public override string[] UpTables
        {
            get
            {
                return new[] { Names.UomType };
            }
        }

        public override void Up()
        {
            // 20 символов может не хватить
            Execute.Sql(string.Format("alter table {0} alter column Title nvarchar(50) NOT NULL", Names.UomType));
        }

        public override void Down()
        {
        }
    }
}