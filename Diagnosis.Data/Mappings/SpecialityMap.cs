﻿using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class SpecialityMap : ClassMapping<Speciality>
    {
        public SpecialityMap()
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

            Set(x => x.Doctors, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.Speciality);
                });
                s.Inverse(true);
                s.Cascade(Cascade.All);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });

            Set(x => x.SpecialityVocabularies, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.Speciality);
                });
                s.Inverse(true);
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
            Set(x => x.SpecialityIcdBlocks, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.Speciality);
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