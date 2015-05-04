using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class UomFormatMap : ClassMapping<UomFormat>
    {
        public UomFormatMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            Property(x => x.String, m =>
            {
                m.NotNullable(true);
                m.Length(50);
            });
            Property(x => x.MeasureValue, m =>
            {
                m.Precision(Types.Numeric.Precision);
                m.Scale(Types.Numeric.ShortScale);
                m.NotNullable(true);
            });
            ManyToOne(x => x.Uom, m =>
            {
                m.Column(Names.Id.Uom);
                m.NotNullable(true);
            });
        }
    }
}