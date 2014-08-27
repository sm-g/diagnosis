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
                m.Generator(Generators.Native);
            });

            Property(x => x.Priority, m => m.NotNullable(true));
            Property(x => x.Title, m => m.NotNullable(true));

            ManyToOne(x => x.DefaultCategory, m =>
            {
                m.Column("DefaultCategoryID");
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

            Set(x => x.SymptomWords, s =>
            {
                s.Key(k =>
                {
                    k.Column("WordID");
                });
                s.Inverse(true);
                s.Cascade(Cascade.All);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });

            Set(x => x.Symptoms, s =>
            {
                s.Table("SymptomWords");
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
                    x.Column("SymptomID");
                    x.Class(typeof(Symptom));
                });
            });
        }
    }
}
