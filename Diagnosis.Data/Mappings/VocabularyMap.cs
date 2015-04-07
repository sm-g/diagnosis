using Diagnosis.Models;
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
                    k.Column(Names.Id.Vocabulary);
                });
                s.Lazy(CollectionLazy.NoLazy); // для синх после закрытия сессии
                s.Inverse(true);
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });

            Bag(x => x.Words, s =>
            {
                s.Table(Names.VocabularyWords);
                s.Key(k =>
                {
                    k.Column(Names.Id.Vocabulary);
                });
                s.Inverse(true);
                s.Cascade(Cascade.Persist);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.ManyToMany(x =>
                {
                    x.Column(Names.Id.Word);
                    x.Class(typeof(Word));
                });
            });

            Set(x => x.Specialities, s =>
            {
                s.Table(Names.SpecialityVocabularies);
                s.Key(k =>
                {
                    k.Column(Names.Id.Vocabulary);
                });
                s.Lazy(CollectionLazy.NoLazy); // для синх после закрытия сессии
                s.Cascade(Cascade.None); // dont touch other side, just delete relation
                s.Access(Accessor.Field);
            }, r =>
            {
                r.ManyToMany(x =>
                {
                    x.Column(Names.Id.Speciality);
                    x.Class(typeof(Speciality));
                });
            });

            Set(x => x.SpecialityVocabularies, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.Vocabulary);
                });
                s.Inverse(true);
                s.Lazy(CollectionLazy.NoLazy); // для синх после закрытия сессии
                s.Cascade(Cascade.None);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
        }
    }
}