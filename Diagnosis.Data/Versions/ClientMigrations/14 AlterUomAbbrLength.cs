using Diagnosis.Common;
using FluentMigrator;
using System;
using System.Linq;

namespace Diagnosis.Data.Versions.Off
{
    [Migration(201503151200)]
    public class AlterUomAbbrLength : SyncronizedMigration
    {
        public AlterUomAbbrLength()
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
            // 10 символов может не хватить для обозначения единицы
            Execute.Sql("alter table Uom alter column Abbr nvarchar(20) NOT NULL");
        }

        public override void Down()
        {
        }
    }
}