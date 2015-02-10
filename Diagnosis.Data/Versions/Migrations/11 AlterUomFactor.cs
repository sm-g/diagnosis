using FluentMigrator;
using System;
using System.Linq;

namespace Diagnosis.Data.Versions
{
    [Migration(201502091200)]
    public class AlterUomFactor : Migration
    {
        public override void Up()
        {
            // Fix
            // 'The conversion is not supported. [ Type to convert from (if known) = varbinary, Type to convert to (if known) = float ]'
            // in syncronization
            Execute.Sql("alter table Uom alter column Factor numeric(18,6))");
        }

        public override void Down()
        {

        }
    }
}