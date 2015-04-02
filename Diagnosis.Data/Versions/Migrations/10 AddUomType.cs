using FluentMigrator;
using System;
using System.Linq;

namespace Diagnosis.Data.Versions.Off
{
    [Migration(201502031200)]
    public class AddUomType : Migration
    {
        private const string Title = "Title";
        private const string Ord = "Ord";
        private const string UomType = "UomType";
        private const string Description = "Description";

        private const string Volume = "Объем";
        private const string Date = "Время";

        public override void Up()
        {
            Create.Table(Names.UomType)
                .WithColumn("Id").AsInt32().Identity().PrimaryKey()
                .WithColumn(Title).AsString(20).NotNullable()
                .WithColumn(Ord).AsInt32().NotNullable();

            Execute.Sql(string.Format("INSERT INTO {0} ({1},{2}) VALUES ('{3}',1)", Names.UomType, Title, Ord, Volume));
            Execute.Sql(string.Format("INSERT INTO {0} ({1},{2}) VALUES ('{3}',2)", Names.UomType, Title, Ord, Date));

            Alter.Table(Names.Uom)
                .AddColumn(Names.Id.UomType).AsInt32().NotNullable()
                    .ForeignKey(Names.FK.Uom_UomType, Names.UomType, "Id").WithDefaultValue(1);

            Alter.Table(Names.Uom)
                .AlterColumn(Description).AsString(100).NotNullable();

            Execute.Sql(string.Format("update {0} set {1} = {2}", Names.Uom, Names.Id.UomType, UomType));
            Delete.Column(UomType).FromTable(Names.Uom);
        }

        public override void Down()
        {
            Delete.ForeignKey(Names.FK.Uom_UomType).OnTable(Names.Uom);
            Delete.Table(Names.UomType);

            // rename back
            Alter.Table(Names.Uom)
               .AddColumn(UomType).AsInt32().NotNullable().WithDefaultValue(1);
            Execute.Sql(string.Format("update {0} set {1} = {2}", Names.Uom, UomType, Names.Id.UomType));
            Delete.Column(Names.Id.UomType).FromTable(Names.Uom);
        }
    }
}