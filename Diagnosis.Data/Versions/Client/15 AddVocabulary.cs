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
            : base(Constants.SqlCeProvider)
        {
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
            Execute.Sql(@"CREATE TABLE {0} (
                    [Id] uniqueidentifier DEFAULT NEWID() ROWGUIDCOL NOT NULL,
                    {1} nvarchar(50) NOT NULL)".FormatStr(
               Names.Vocabulary,
               Title));

            Execute.Sql(@"CREATE TABLE {0} (
                    [Id] uniqueidentifier DEFAULT NEWID() ROWGUIDCOL NOT NULL,
                    {1} nvarchar(100) NOT NULL,
                    {2} uniqueidentifier NOT NULL)".FormatStr(
               Names.WordTemplate,
               Title,
               Names.Id.Vocabulary, Names.FK.WordTemplate_Voc, Names.Vocabulary));

            Execute.Sql(@"CREATE TABLE {0} (
                    [Id] uniqueidentifier DEFAULT NEWID() ROWGUIDCOL NOT NULL,
                    {1} uniqueidentifier NOT NULL,
                    {4} uniqueidentifier NOT NULL)".FormatStr(
              Names.SpecialityVocabularies,
              Names.Id.Vocabulary, Names.FK.SpecVoc_Voc, Names.Vocabulary,
              Names.Id.Speciality, Names.FK.SpecVoc_Spec, Names.Speciality));

            // только на клиенте
            Execute.Sql(@"CREATE TABLE {0} (
                    [Id] uniqueidentifier DEFAULT NEWID() ROWGUIDCOL NOT NULL,
                    {1} uniqueidentifier NOT NULL,
                    {4} uniqueidentifier NOT NULL)".FormatStr(
                Names.VocabularyWords,
                Names.Id.Vocabulary, Names.FK.VocWord_Voc, Names.Vocabulary,
                Names.Id.Word, Names.FK.VocWord_Word, Names.Word));

            // PK
            Execute.Sql(alterAddPk.FormatStr(Names.Vocabulary));
            Execute.Sql(alterAddPk.FormatStr(Names.WordTemplate));
            Execute.Sql(alterAddPk.FormatStr(Names.SpecialityVocabularies));

            // только на клиенте
            Execute.Sql(alterAddPk.FormatStr(Names.VocabularyWords));

            // FK
            Execute.Sql(alterAddFk.FormatStr(
                Names.WordTemplate,
                Names.FK.WordTemplate_Voc, Names.Id.Vocabulary, Names.Vocabulary));

            Execute.Sql(alterAddFk.FormatStr(
                Names.SpecialityVocabularies,
                Names.FK.SpecVoc_Voc, Names.Id.Vocabulary, Names.Vocabulary));
            Execute.Sql(alterAddFk.FormatStr(
                Names.SpecialityVocabularies,
                Names.FK.SpecVoc_Spec, Names.Id.Speciality, Names.Speciality));

            // только на клиенте
            Execute.Sql(alterAddFk.FormatStr(
               Names.VocabularyWords,
               Names.FK.VocWord_Voc, Names.Id.Vocabulary, Names.Vocabulary));
            Execute.Sql(alterAddFk.FormatStr(
               Names.VocabularyWords,
               Names.FK.VocWord_Word, Names.Id.Word, Names.Word));


            // add custom voc to doctor
            Execute.Sql("ALTER TABLE {0} ADD COLUMN {1} uniqueidentifier NULL".FormatStr(
              Names.Doctor,
              Names.Col.DoctorCustomVocabulary, Names.FK.Doc_Voc, Names.Vocabulary));

            Execute.Sql(alterAddFk.FormatStr(
              Names.Doctor,
              Names.FK.Doc_Voc, Names.Col.DoctorCustomVocabulary, Names.Vocabulary));

            // add all words to custom vocabulary for doctor
            Execute.Sql("INSERT INTO {0} ({1})	select ('{2}') from {3}".FormatStr(
                Names.Vocabulary,
                Title,
                Vocabulary.CustomTitle,
                Names.Doctor));
            Execute.Sql("INSERT INTO {0} ({1}, {2}) select w.Id, v.Id from {3} as w, {4} as v".FormatStr(
                Names.VocabularyWords,
                Names.Id.Word,
                Names.Id.Vocabulary,
                Names.Word,
                Names.Vocabulary));

        }

        public override void Down()
        {
        }
    }
}