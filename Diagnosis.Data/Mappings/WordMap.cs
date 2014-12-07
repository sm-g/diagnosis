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

            Property(x => x.Title, m =>
            {
                m.NotNullable(true);
                m.Length(100);
            });

            ManyToOne(x => x.DefaultCategory, m =>
            {
                m.Column("DefHrCategoryID");
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
        }
    }
}
