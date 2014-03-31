﻿using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class IcdBlockMap : ClassMapping<IcdBlock>
    {
        public IcdBlockMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.Native);
            });

            Property(x => x.Title);
            Set(x => x.IcdDiseases, s =>
            {
                s.Key(k =>
                {
                    k.Column("BlokID");
                });
                s.Inverse(true);
                s.Cascade(Cascade.All);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
            ManyToOne(x => x.IcdChapter, m => m.Column("ChapterID"));
        }
    }
}