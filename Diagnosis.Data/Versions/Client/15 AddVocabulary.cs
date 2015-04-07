using Diagnosis.Common;
using Diagnosis.Models;
using FluentMigrator;
using System;
using System.Data.SqlServerCe;
using System.Linq;

namespace Diagnosis.Data.Versions.Client
{
    [Migration(201503251200)]
    public class AddVocabulary : SyncronizedMigration
    {
        private const string Title = "Title";
        private const string alterAddFk = "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3}([Id])";
        private const string alterAddPk = "ALTER TABLE {0} ADD CONSTRAINT PK__{0} PRIMARY KEY ([Id])";

        public AddVocabulary()
        {
            Provider = Constants.SqlCeProvider;
        }

        public override string[] UpTables
        {
            get
            {
                return new[] { 
                    Names.Vocabulary, 
                    Names.VocabularyWords, 
                    Names.WordTemplate, 
                    Names.SpecialityVocabularies, 
                    Names.Doctor };
            }
        }

        public override void Up()
        {
            using (var conn = new SqlCeConnection(ConnectionString))
            {
                conn.Open();
                SqlCeCommand cmd = conn.CreateCommand();

                ExecuteNonQuery(cmd, "CREATE TABLE {0} (" +
                    "  [Id] uniqueidentifier DEFAULT NEWID() ROWGUIDCOL NOT NULL" +
                    ", {1} nvarchar(50) NOT NULL)",
                   Names.Vocabulary,
                   Title);

                ExecuteNonQuery(cmd, "CREATE TABLE {0} (" +
                    "  [Id] uniqueidentifier DEFAULT NEWID() ROWGUIDCOL NOT NULL" +
                    ", {1} nvarchar(100) NOT NULL" +
                    ", {2} uniqueidentifier NOT NULL)",
                   Names.WordTemplate,
                   Title,
                   Names.Id.Vocabulary, Names.FK.WordTemplate_Voc, Names.Vocabulary);

                ExecuteNonQuery(cmd, "CREATE TABLE {0} (" +
                    "  [Id] uniqueidentifier DEFAULT NEWID() ROWGUIDCOL NOT NULL" +
                    ", {1} uniqueidentifier NOT NULL" +
                    ", {4} uniqueidentifier NOT NULL)",
                  Names.SpecialityVocabularies,
                  Names.Id.Vocabulary, Names.FK.SpecVoc_Voc, Names.Vocabulary,
                  Names.Id.Speciality, Names.FK.SpecVoc_Spec, Names.Speciality);

                // только на клиенте
                ExecuteNonQuery(cmd, "CREATE TABLE {0} (" +
                    "  [Id] uniqueidentifier DEFAULT NEWID() ROWGUIDCOL NOT NULL" +
                    ", {1} uniqueidentifier NOT NULL" +
                    ", {4} uniqueidentifier NOT NULL)",
                    Names.VocabularyWords,
                    Names.Id.Vocabulary, Names.FK.VocWord_Voc, Names.Vocabulary,
                    Names.Id.Word, Names.FK.VocWord_Word, Names.Word);

                // PK
                ExecuteNonQuery(cmd, alterAddPk,
                    Names.Vocabulary);
                ExecuteNonQuery(cmd, alterAddPk,
                    Names.WordTemplate);
                ExecuteNonQuery(cmd, alterAddPk,
                    Names.SpecialityVocabularies);

                // только на клиенте
                ExecuteNonQuery(cmd, alterAddPk,
                    Names.VocabularyWords);

                // FK
                ExecuteNonQuery(cmd, alterAddFk,
                    Names.WordTemplate,
                    Names.FK.WordTemplate_Voc, Names.Id.Vocabulary, Names.Vocabulary);

                ExecuteNonQuery(cmd, alterAddFk,
                    Names.SpecialityVocabularies,
                    Names.FK.SpecVoc_Voc, Names.Id.Vocabulary, Names.Vocabulary);
                ExecuteNonQuery(cmd, alterAddFk,
                    Names.SpecialityVocabularies,
                    Names.FK.SpecVoc_Spec, Names.Id.Speciality, Names.Speciality);

                // только на клиенте
                ExecuteNonQuery(cmd, alterAddFk,
                   Names.VocabularyWords,
                   Names.FK.VocWord_Voc, Names.Id.Vocabulary, Names.Vocabulary);
                ExecuteNonQuery(cmd, alterAddFk,
                   Names.VocabularyWords,
                   Names.FK.VocWord_Word, Names.Id.Word, Names.Word);


                // add custom voc to doctor
                ExecuteNonQuery(cmd, "ALTER TABLE {0} " +
                    "ADD COLUMN {1} uniqueidentifier NULL",
                  Names.Doctor,
                  Names.Col.DoctorCustomVocabulary, Names.FK.Doc_Voc, Names.Vocabulary);

                ExecuteNonQuery(cmd, alterAddFk,
                  Names.Doctor,
                  Names.FK.Doc_Voc, Names.Col.DoctorCustomVocabulary, Names.Vocabulary);

                // add all words to custom vocabulary for doctor
                ExecuteNonQuery(cmd, "INSERT INTO {0} ({1})	select ('{2}') from {3}",
                    Names.Vocabulary,
                    Title,
                    Vocabulary.CustomTitle,
                    Names.Doctor);
                ExecuteNonQuery(cmd, "INSERT INTO {0} ({1}, {2}) select w.Id, v.Id from {3} as w, {4} as v",
                    Names.VocabularyWords,
                    Names.Id.Word,
                    Names.Id.Vocabulary,
                    Names.Word,
                    Names.Vocabulary);
            }

        }
        private static void ExecuteNonQuery(SqlCeCommand cmd, string format, params object[] args)
        {
            cmd.CommandText = string.Format(format, args);
            cmd.ExecuteNonQuery();
        }
        public override void Down()
        {
        }
    }
}