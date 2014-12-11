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
                m.Generator(Generators.GuidComb);
            });


            Property(x => x.HashAndSalt, m =>
            {
                m.Length(PasswordHash.PasswordHashManager.HASH_LENGTH);
            });
            Property(x => x.Remember);
        }
    }
}
