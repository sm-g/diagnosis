using Diagnosis.Common;
using FluentMigrator;
using System;
using System.Linq;

namespace Diagnosis.Data.Versions
{
    [Migration(201503251200)]
    public class AddVocabulary : SyncronizedMigration
    {
        private string FK_VocWord_Voc = "FK_VocabularyWords_Vocabulary";
        private string FK_VocWord_Word = "FK_VocabularyWords_Word";
        private string FK_SpecVoc_Voc = "FK_SpecialityVocabularies_Word";
        private string FK_SpecVoc_Spec = "FK_SpecialityVocabularies_Speciality";
        private string FK_WordTemplate_Voc = "FK_WordTemplate_Vocabulary";
        private string FK_Doc_Voc = "FK_Doctor_Vocabulary";

        private const string VocabularyID = "VocabularyID";
        private const string SpecialityID = "SpecialityID";

        private string WordID = "WordID";
        private const string Title = "Title";

        public AddVocabulary()
        {
            Provider = Constants.SqlCeProvider;
        }

        public override string[] UpTables
        {
            get
            {
                return new[] { Names.VocabularyTbl, Names.VocabularyWordTbl, Names.WordTemplateTbl, Names.SpecialityVocabulariesTbl };
            }
        }

        public override void Up()
        {
            Create.Table(Names.VocabularyTbl)
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey("PK__Vocabulary").WithDefault(SystemMethods.NewGuid)
                .WithColumn(Title).AsString(50).NotNullable();

            Create.Table(Names.WordTemplateTbl)
               .WithColumn("Id").AsGuid().NotNullable().PrimaryKey("PK__WordTemplate").WithDefault(SystemMethods.NewGuid)
               .WithColumn(Title).AsString(100).NotNullable()
               .WithColumn(VocabularyID).AsGuid().NotNullable().ForeignKey(FK_WordTemplate_Voc, Names.VocabularyTbl, "Id");

            Create.Table(Names.SpecialityVocabulariesTbl)
               .WithColumn("Id").AsGuid().NotNullable().PrimaryKey("PK__SpecialityVocabularies").WithDefault(SystemMethods.NewGuid)
               .WithColumn(VocabularyID).AsGuid().NotNullable().ForeignKey(FK_SpecVoc_Voc, Names.VocabularyTbl, "Id")
               .WithColumn(SpecialityID).AsGuid().NotNullable().ForeignKey(FK_SpecVoc_Spec, Names.SpecialityTbl, "Id");

            // только на клиенте
            Create.Table(Names.VocabularyWordTbl)
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey("PK__VocabularyWords").WithDefault(SystemMethods.NewGuid)
                .WithColumn(VocabularyID).AsGuid().NotNullable().ForeignKey(FK_VocWord_Voc, Names.VocabularyTbl, "Id")
                .WithColumn(WordID).AsGuid().NotNullable().ForeignKey(FK_VocWord_Word, Names.WordTbl, "Id");

            Alter.Table(Names.DoctorTbl)
                .AddColumn("CustomVocabularyID").AsGuid().Nullable().ForeignKey(FK_Doc_Voc, Names.VocabularyTbl, "Id");
        }

        public override void Down()
        {
        }
    }
}