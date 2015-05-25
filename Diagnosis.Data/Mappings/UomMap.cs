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
                m.Generator(Generators.GuidComb);
            });

            Property(x => x.Abbr, m =>
            {
                m.NotNullable(true);
                m.Length(Length.UomAbbr);
            });
            Property(x => x.Description, m =>
            {
                m.NotNullable(true);
                m.Length(Length.UomDescr);
            });
            Property(x => x.Factor, m =>
            {
                m.NotNullable(true);
                m.Precision(Types.Numeric.Precision);
                m.Scale(Types.Numeric.Scale);
            });
            ManyToOne(x => x.Type, m =>
            {
                m.Column(Names.Id.UomType);
                m.NotNullable(true);
            });
            Set(x => x.Formats, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.Uom);
                });
                s.Lazy(CollectionLazy.NoLazy);
                s.Inverse(true);
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
        }
    }
}