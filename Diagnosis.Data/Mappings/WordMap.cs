using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class WordMap : ClassMapping<Word>
    {
        public WordMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            Property(x => x.Title, m =>
            {
                m.NotNullable(true);
                m.Length(100);
                m.UniqueKey("WordTitle");
            });

            ManyToOne(x => x.Parent, m =>
            {
                m.Column("ParentID");
            });

            Set(x => x.Children, s =>
            {
                s.Key(k =>
                {
                    k.Column("ParentID");
                });
                s.Inverse(true);
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });

            Bag(x => x.HealthRecords, s =>
            {
                s.Table("HrItem");
                s.Key(k =>
                {
                    k.Column("WordID");
                });
                s.Cascade(Cascade.All);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.ManyToMany(x =>
                {
                    x.Column("HealthRecordID");
                    x.Class(typeof(HealthRecord));
                });
            });

            Bag(x => x.Vocabularies, s =>
            {
                s.Table("VocabularyWords");
                s.Key(k =>
                {
                    k.Column("WordID");
                });
                s.Cascade(Cascade.All);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.ManyToMany(x =>
                {
                    x.Column("VocabularyID");
                    x.Class(typeof(Vocabulary));
                });
            });
        }
    }
}