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

            Property(x => x.Title, m =>
            {
                m.NotNullable(true);
                m.UniqueKey(Names.Unique.ChpaterCode);
                m.Length(Length.IcdChapterTitle);
            });
            Property(x => x.Code, m =>
            {
                m.NotNullable(true);
                m.Length(Length.IcdCode);
            });
            Set(x => x.IclBlocks, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.IcdChapter);
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