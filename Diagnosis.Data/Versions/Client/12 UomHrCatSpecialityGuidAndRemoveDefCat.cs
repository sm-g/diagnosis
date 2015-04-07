using Diagnosis.Common;
using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Linq;

namespace Diagnosis.Data.Versions.Client.Off
{
    [Migration(201502121200)]
    public class UomHrCatSpecialityGuidAndRemoveDefCat : SyncronizedMigration
    {
        public UomHrCatSpecialityGuidAndRemoveDefCat()
        {
            Provider = Constants.SqlCeProvider;
        }

        public override string[] UpTables
        {
            get
            {
                return new[] {
                    Names.Word,
                    Names.UomType,
                    Names.Uom,
                    Names.Speciality,
                    Names.SpecialityIcdBlocks,
                    Names.HrCategory,
                    Names.HrItem,
                    Names.HealthRecord,
                    Names.Doctor };
            }
        }

        public override void Up()
        {
            BeforeUp();

            // delete Word.DefHrCategoryID
            using (var conn = new SqlCeConnection(ConnectionString))
            {
                conn.Open();
                SqlCeCommand cmd = conn.CreateCommand();

                ExecuteNonQuery(cmd, "ALTER TABLE {0} DROP constraint {1}", Names.Word, Names.FK.Word_HrCategory);
                ExecuteNonQuery(cmd, "ALTER TABLE {0} DROP Column {1}", Names.Word, "DefHrCategoryID");
            }

            // change Id PK to Guid

            IntToGuidPK(Names.UomType,
                new[] { new Child() { table = Names.Uom, fk = Names.FK.Uom_UomType, notnull = true } },
                "PK_UomType");

            IntToGuidPK(Names.Uom,
                new[] { new Child() { table = Names.HrItem, fk = Names.FK.HrItem_Uom, notnull = false } },
                "PK__Uom");

            IntToGuidPK(Names.HrCategory,
                new[] { new Child() { table = Names.HealthRecord, fk = Names.FK.Hr_HrCategory, notnull = false } },
                "PK__HrCategory");

            IntToGuidPK(Names.Speciality,
                new[] {
                    new Child() { table = Names.Doctor, fk = Names.FK.Doctor_Speciality, notnull = false } ,
                    new Child() { table = Names.SpecialityIcdBlocks, fk = Names.FK.SpecialityIcdBlocks_Specia, notnull = false } ,
                },
                "PK__Speciality");

            IntToGuidPK(Names.SpecialityIcdBlocks,
              new Child[] { },
              "PK__SpecialityIcdBlocks");
        }

        public override void Down()
        {
            // sorry, no way back
            throw new NotImplementedException();
        }

        private static void ExecuteNonQuery(SqlCeCommand cmd, string format, params object[] args)
        {
            cmd.CommandText = string.Format(format, args);
            cmd.ExecuteNonQuery();
        }

        private void IntToGuidPK(string parentTable, Child[] childs, string parentPkConstraint)
        {
            var fkColumn = string.Format("{0}ID", parentTable);
            var oldFkCol = string.Format("Old{0}", fkColumn);
            var oldId = "OldId";
            var Pk = string.Format("PK__{0}", parentTable);
            var Id = "Id";

            using (var conn = new SqlCeConnection(ConnectionString))
            {
                conn.Open();
                SqlCeCommand cmd = conn.CreateCommand();

                // copy old int Id

                ExecuteNonQuery(cmd, "Alter TABLE {0} add column {1} integer", parentTable, oldId);
                ExecuteNonQuery(cmd, "update {0} set {1} = {2}", parentTable, oldId, Id);

                for (int i = 0; i < childs.Count(); i++)
                {
                    var childTable = childs[i].table;
                    var Fk = childs[i].fk;

                    // copy and delete fkcolumn
                    ExecuteNonQuery(cmd, "Alter TABLE {0} add column {1} integer", childTable, oldFkCol);
                    ExecuteNonQuery(cmd, "update {0} set {1} = {2}", childTable, oldFkCol, fkColumn);

                    ExecuteNonQuery(cmd, "ALTER TABLE {0} DROP constraint {1}", childTable, Fk);
                    ExecuteNonQuery(cmd, "ALTER TABLE {0} DROP Column {1}", childTable, fkColumn);
                }
                // delete old int Id
                ExecuteNonQuery(cmd, "ALTER TABLE {0} DROP constraint {1}", parentTable, parentPkConstraint);
                ExecuteNonQuery(cmd, "ALTER TABLE {0} DROP Column {1}", parentTable, Id);

                // create guid id
                ExecuteNonQuery(cmd, "Alter TABLE {0} add column {1} uniqueidentifier NOT NULL DEFAULT NEWID() ", parentTable, Id);
                ExecuteNonQuery(cmd, "Alter TABLE {0} add CONSTRAINT {1} PRIMARY KEY ({2})", parentTable, Pk, Id);

                for (int i = 0; i < childs.Count(); i++)
                {
                    var childTable = childs[i].table;
                    var Fk = childs[i].fk;

                    // create guid fk columns

                    ExecuteNonQuery(cmd, "Alter TABLE {0} add column {1} uniqueidentifier", childTable, fkColumn);

                    // update fk guid (no Set From in sqlce)

                    cmd.CommandText = string.Format("SELECT {0} FROM {1}", oldFkCol, childTable);
                    List<int> oldFkIds = new List<int>();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        if (!Convert.IsDBNull(reader[0]))
                            oldFkIds.Add(Convert.ToInt32(reader[0]));
                    }
                    reader.Close();

                    foreach (var oldFkId in oldFkIds)
                    {
                        cmd.CommandText = string.Format("SELECT p.Id FROM {0} p inner join {1} c on p.{2}={3}", parentTable, childTable, oldId, oldFkId);
                        Guid newFkId = (Guid)cmd.ExecuteScalar();

                        ExecuteNonQuery(cmd, "UPDATE {0} SET {1}='{2}' WHERE {3}={4}", childTable, fkColumn, newFkId, oldFkCol, oldFkId);
                    }

                    // not null fk
                    if (childs[i].notnull)
                        ExecuteNonQuery(cmd, "Alter TABLE {0} alter column {1} uniqueidentifier NOT NULL", childTable, fkColumn);

                    // add fk CONSTRAINT
                    ExecuteNonQuery(cmd, "Alter TABLE {0} add CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3}({4})", childTable, Fk, fkColumn, parentTable, Id);

                    // drop old int columns
                    ExecuteNonQuery(cmd, "ALTER TABLE {0} DROP Column {1}", childTable, oldFkCol);
                }

                ExecuteNonQuery(cmd, "ALTER TABLE {0} DROP Column {1}", parentTable, oldId);
            }
        }

        private struct Child
        {
            public string table;
            public string fk;
            public bool notnull;
        }
    }
}