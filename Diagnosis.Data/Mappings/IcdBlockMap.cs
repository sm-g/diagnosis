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

            Property(x => x.Title, m =>
            {
                m.NotNullable(true);
                m.UniqueKey("BlockCode");
            });
            Property(x => x.Code, m =>
            {
                m.NotNullable(true);
            });
            //Set(x => x.SpecialityIcdBlocks, s =>
            //{
            //    s.Key(k =>
            //    {
            //        k.Column("IcdBlockID");
            //    });
            //    s.Inverse(true);
            //    s.Cascade(Cascade.All | Cascade.DeleteOrphans);
            //    s.Access(Accessor.Field);
            //}, r =>
            //{
            //    r.OneToMany();
            //});
            Set(x => x.IcdDiseases, s =>
            {
                s.Key(k =>
                {
                    k.Column("IcdBlockID");
                });
                s.Inverse(true);
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
            ManyToOne(x => x.IcdChapter, m =>
            {
                m.Column("ChapterID");
                m.NotNullable(true);
            });
        }
    }
}