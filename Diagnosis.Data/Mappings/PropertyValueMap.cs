using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Diagnosis.Models;

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
