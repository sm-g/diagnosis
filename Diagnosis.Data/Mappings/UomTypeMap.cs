using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class UomTypeMap : ClassMapping<UomType>
    {
        public UomTypeMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.Native);
            });

            Property(x => x.Title, m =>
            {
                m.NotNullable(true);
                m.Length(20);
            });
            Property(x => x.Ord, m =>
            {
                m.NotNullable(true);
            });
        }
    }
}
