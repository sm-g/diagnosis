using Diagnosis.Common;
using FluentMigrator;
using System;
using System.Linq;

namespace Diagnosis.Data.Versions.Client.Off
{
    [Migration(201503251200)]
    public class AddVocabulary : SyncronizedMigration
    {
        private const string Title = "Title";

        public AddVocabulary()
        {
            Provider = Constants.SqlCeProvider;
        }

        public override string[] UpTables
        {
            get
            {
                return new[] { Names.Vocabulary, Names.VocabularyWords, Names.WordTemplate, Names.SpecialityVocabularies };
            }
        }

        public override void Up()
        {
            Create.Table(Names.Vocabulary)
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey("PK__Vocabulary").WithDefault(SystemMethods.NewGuid)
                .WithColumn(Title).AsString(50).NotNullable();

            Create.Table(Names.WordTemplate)
               .WithColumn("Id").AsGuid().NotNullable().PrimaryKey("PK__WordTemplate").WithDefault(SystemMethods.NewGuid)
               .WithColumn(Title).AsString(100).NotNullable()
               .WithColumn(Names.Id.Vocabulary).AsGuid().NotNullable().ForeignKey(Names.FK.WordTemplate_Voc, Names.Vocabulary, "Id");

            Create.Table(Names.SpecialityVocabularies)
               .WithColumn("Id").AsGuid().NotNullable().PrimaryKey("PK__SpecialityVocabularies").WithDefault(SystemMethods.NewGuid)
               .WithColumn(Names.Id.Vocabulary).AsGuid().NotNullable().ForeignKey(Names.FK.SpecVoc_Voc, Names.Vocabulary, "Id")
               .WithColumn(Names.Id.Speciality).AsGuid().NotNullable().ForeignKey(Names.FK.SpecVoc_Spec, Names.Speciality, "Id");

            // только на клиенте
            Create.Table(Names.VocabularyWords)
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey("PK__VocabularyWords").WithDefault(SystemMethods.NewGuid)
                .WithColumn(Names.Id.Vocabulary).AsGuid().NotNullable().ForeignKey(Names.FK.VocWord_Voc, Names.Vocabulary, "Id")
                .WithColumn(Names.Id.Word).AsGuid().NotNullable().ForeignKey(Names.FK.VocWord_Word, Names.Word, "Id");

            Alter.Table(Names.Doctor)
                .AddColumn(Names.Col.DoctorCustomVocabulary).AsGuid().Nullable().ForeignKey(Names.FK.Doc_Voc, Names.Vocabulary, "Id");

        }

        public override void Down()
        {
        }
    }
}