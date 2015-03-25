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

            Set(x => x.TempWords, s =>
            {
                s.Key(k =>
                {
                    k.Column("VocabularyID");
                });
                s.Inverse(true);
                s.Cascade(Cascade.All);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });

            //Set(x => x.SpecialityIcdBlocks, s =>
            //{
            //    s.Key(k =>
            //    {
            //        k.Column("SpecialityID");
            //    });
            //    s.Inverse(true);
            //    s.Cascade(Cascade.All | Cascade.DeleteOrphans);
            //    s.Access(Accessor.Field);
            //}, r =>
            //{
            //    r.OneToMany();
            //});

            Bag(x => x.Words, s =>
            {
                s.Table("VocabularyWords");
                s.Key(k =>
                {
                    k.Column("VocabularyID");
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