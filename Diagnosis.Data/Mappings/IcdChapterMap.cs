using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class IcdChapterMap : ClassMapping<IcdChapter>
    {
        public IcdChapterMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.Native);
            });

            Property(x => x.Title);
            Property(x => x.Code);
            Set(x => x.IclBlocks, s =>
            {
                s.Key(k =>
                {
                    k.Column("ChapterID");
                });
                s.Inverse(true);
                s.Cascade(Cascade.All);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
        }
    }
}
