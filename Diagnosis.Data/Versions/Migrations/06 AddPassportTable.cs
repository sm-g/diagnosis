using Diagnosis.Models;
using FluentMigrator;
using PasswordHash;

namespace Diagnosis.Data.Versions
{
    [Migration(201412111200)]
    public class AddPassportTable : Migration
    {
        private const string FK_Doctor_Passport = "FK_Doctor_Passport";

        public override void Up()
        {
            Create.Table(Names.PassportTbl)
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey("PK__Passport")
                .WithColumn("HashAndSalt").AsFixedLengthString(PasswordHashManager.HASH_LENGTH).Nullable()
                .WithColumn("Remember").AsBoolean().NotNullable().WithDefaultValue(0);

            // администратор с паролем по умолчанию
            Insert.IntoTable(Names.PassportTbl)
                .Row(new
                {
                    Id = Admin.DefaultId,
                    HashAndSalt = PasswordHashManager.CreateHash(Admin.DefaultPassword)
                });

            // для каждого врача - пасспорт без пароля
            Execute.Sql(string.Format("INSERT INTO {0} ([Id]) Select Id from {1}", Names.PassportTbl, Names.DoctorTbl));

            Create.ForeignKey(FK_Doctor_Passport).FromTable(Names.DoctorTbl)
               .ForeignColumn("Id")
               .ToTable(Names.PassportTbl)
               .PrimaryColumn("Id");
        }

        public override void Down()
        {
            Delete.ForeignKey(FK_Doctor_Passport).OnTable(Names.DoctorTbl);
            Delete.Table(Names.PassportTbl);
        }
    }
}