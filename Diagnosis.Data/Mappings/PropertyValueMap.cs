using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class PropertyValueMap : ClassMapping<PropertyValue>
    {
        public PropertyValueMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.Native);
            });

            Property(x => x.Title);
            Set(x => x.PatientProperties, s =>
            {
                s.Key(k =>
                {
                    k.Column("ValueID");
                });
                s.Inverse(true);
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
            ManyToOne(x => x.Property, m =>
            {
                m.Column("PropertyID");
            });
        }
    }
}
