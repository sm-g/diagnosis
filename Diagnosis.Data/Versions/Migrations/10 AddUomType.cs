﻿using FluentMigrator;
using System;
using System.Linq;

namespace Diagnosis.Data.Versions
{
    [Migration(201502031200)]
    public class AddUomType : Migration
    {
        private const string Title = "Title";
        private const string Ord = "Ord";
        private const string UomTypeId = "UomTypeID";
        private const string UomType = "UomType";
        private const string FK_Uom_UomType = "FK_Uom_UomType";
        private const string Description = "Description";

        private const string Volume = "Объем";
        private const string Date = "Время";

        public override void Up()
        {
            Create.Table(Names.UomTypeTbl)
                .WithColumn("Id").AsInt32().Identity().PrimaryKey()
                .WithColumn(Title).AsString(20).NotNullable()
                .WithColumn(Ord).AsInt32().NotNullable();

            Execute.Sql(string.Format("INSERT INTO {0} ({1},{2}) VALUES ('{3}',1)", Names.UomTypeTbl, Title, Ord, Volume));
            Execute.Sql(string.Format("INSERT INTO {0} ({1},{2}) VALUES ('{3}',2)", Names.UomTypeTbl, Title, Ord, Date));

            Alter.Table(Names.UomTbl)
                .AddColumn(UomTypeId).AsInt32().NotNullable()
                    .ForeignKey(FK_Uom_UomType, Names.UomTypeTbl, "Id").WithDefaultValue(1);

            Alter.Table(Names.UomTbl)
                .AlterColumn(Description).AsString(100).NotNullable();

            Execute.Sql(string.Format("update {0} set {1} = {2}", Names.UomTbl, UomTypeId, UomType));
            Delete.Column(UomType).FromTable(Names.UomTbl);
        }

        public override void Down()
        {
            Delete.ForeignKey(FK_Uom_UomType).OnTable(Names.UomTbl);
            Delete.Table(Names.UomTypeTbl);

            // rename back
            Alter.Table(Names.UomTbl)
               .AddColumn(UomType).AsInt32().NotNullable().WithDefaultValue(1);
            Execute.Sql(string.Format("update {0} set {1} = {2}", Names.UomTbl, UomType, UomTypeId));
            Delete.Column(UomTypeId).FromTable(Names.UomTbl);
        }
    }
}