using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Diagnosis.Models;

namespace Diagnosis.Data.Mappings
{
    public class PropertyMap : ClassMapping<Property>
    {
        public PropertyMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.Native);
            });

            Property(x => x.Title);

            Set(x => x.Values, s =>
            {
                s.Key(k =>
                {
                    k.Column("PropertyID");
                });
                s.Inverse(true);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
            Set(x => x.PatientProperties, s =>
            {
                s.Key(k =>
                {
                    k.Column("PropertyID");
                });
                s.Inverse(true);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
        }
    }
}
