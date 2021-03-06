﻿using Diagnosis.Models;
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
                m.Generator(Generators.GuidComb);
            });

            Property(x => x.Title, m =>
            {
                m.NotNullable(true);
                m.Length(Length.LongTitle);
            });
            Property(x => x.Ord, m =>
            {
                m.NotNullable(true);
            });
            Set(x => x.Uoms, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.UomType);
                });
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