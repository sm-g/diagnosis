﻿using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class SymptomMap : ClassMapping<Symptom>
    {
        public SymptomMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.Native);
            });

            ManyToOne(x => x.Disease, m =>
            {
                m.Column("DiseaseID");
            });
            ManyToOne(x => x.DefaultCategory, m =>
            {
                m.Column("DefaultCategoryID");
            });

            Set(x => x.Words, s =>
            {
                s.Table("SymptomWords");
                s.Key(k =>
                {
                    k.Column("SymptomID");
                });
                s.Cascade(Cascade.All);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.ManyToMany(x =>
                {
                    x.Column("WordID");
                    x.Class(typeof(Word));
                });
            });
        }
    }
}
