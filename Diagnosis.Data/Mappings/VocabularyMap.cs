using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings.Client
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

            Set(x => x.SpecialityVocabularies, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.Vocabulary);
                    //k.OnDelete(OnDeleteAction.Cascade);
                });
                s.Inverse(true);
                s.Lazy(CollectionLazy.NoLazy); // для синх после закрытия сессии
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });

            Set(x => x.VocabularyWords, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.Vocabulary);
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
namespace Diagnosis.Data.Mappings.Server
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

            Set(x => x.SpecialityVocabularies, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.Vocabulary);
                    //k.OnDelete(OnDeleteAction.Cascade);
                });
                s.Inverse(true);
                s.Lazy(CollectionLazy.NoLazy); // для синх после закрытия сессии
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
        }
    }
}