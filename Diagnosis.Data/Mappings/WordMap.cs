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

            Property(x => x.Priority);
            Property(x => x.Title);
            Property(x => x.IsEnum);

            ManyToOne(x => x.DefaultCategory, m =>
            {
                m.Column("DefaultCategoryID");
            });
            ManyToOne(x => x.Parent, m =>
            {
                m.Column("ParentID");
            });
        }
    }
}
