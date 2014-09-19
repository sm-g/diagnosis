using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class UomMap : ClassMapping<Uom>
    {
        public UomMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.Native);
            });

            Property(x => x.Abbr, m => m.NotNullable(true));
            Property(x => x.Description);
            Property(x => x.Factor, m => m.NotNullable(true));
            Property(x => x.Type, m =>
            {
                m.Column("UomType");
                m.NotNullable(true);
            });
        }
    }
}
