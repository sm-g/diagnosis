﻿using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class VocabularyMap : ClassMapping<Vocabulary>
    {
        public VocabularyMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            Property(x => x.Title, m =>
            {
                m.NotNullable(true);
                m.Length(50);
            });

            OneToOne(x => x.Doctor, m =>
            {
                m.PropertyReference(typeof(Doctor).GetPropertyOrFieldMatchingName("CustomVocabulary"));
            });

            Set(x => x.WordTemplates, s =>
            {
                s.Key(k =>
                {
                    k.Column("VocabularyID");
                });
                s.Lazy(CollectionLazy.NoLazy); // для создания слов после закрытия сессии
                s.Inverse(true);
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });

            Bag(x => x.Words, s =>
            {
                s.Table("VocabularyWords");
                s.Key(k =>
                {
                    k.Column("VocabularyID");
                });
                s.Inverse(true);
                s.Cascade(Cascade.Persist);
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