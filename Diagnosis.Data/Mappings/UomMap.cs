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

            Property(x => x.Abbr, m =>
            {
                m.NotNullable(true);
                m.Length(10);
            });
            Property(x => x.Description, m =>
            {
                m.NotNullable(true);
                m.Length(100);
            });
            Property(x => x.Factor, m => m.NotNullable(true));
            ManyToOne(x => x.Type, m =>
            {
                m.Column("UomTypeID");
                m.NotNullable(true);
            });
        }
    }
}