using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class PassportMap : ClassMapping<Passport>
    {
        public PassportMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.Foreign<Passport>(x => x.Doctor));
            });

            Property(x => x.HashAndSalt, m =>
            {
                m.Length(PasswordHash.PasswordHashManager.HASH_LENGTH);
            });
            Property(x => x.Remember, m =>
            {
                m.Column(c =>
                {
                    c.Default(0);
                });
                m.NotNullable(true);
            });
            OneToOne(x => x.Doctor, m => { });
        }
    }
}