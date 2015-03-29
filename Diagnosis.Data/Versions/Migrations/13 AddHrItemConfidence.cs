using Diagnosis.Common;
using Diagnosis.Models;
using FluentMigrator;
using System;
using System.Linq;

namespace Diagnosis.Data.Versions
{
    [Migration(201503111200)]
    public class AddHrItemConfidence : SyncronizedMigration
    {
        private string ConfidenceCol = "Confidence";
        public AddHrItemConfidence()
        {
            Provider = Constants.SqlCeProvider;
        }

        public override string[] UpTables
        {
            get
            {
                return new[] { Names.HrItem };
            }
        }

        public override void Up()
        {
            Alter.Table(Names.HrItem)
                .AddColumn(ConfidenceCol).AsString().WithDefaultValue(Confidence.Present.ToString());
        }

        public override void Down()
        {
        }
    }
}