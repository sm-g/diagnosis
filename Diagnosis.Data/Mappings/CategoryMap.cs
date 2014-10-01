using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class CategoryMap : ClassMapping<HrCategory>
    {
        public CategoryMap()
        {
            Table("RecordCategory");

            Id(x => x.Id, m =>
            {
                m.Generator(Generators.Native);
            });

            Property(x => x.Name, m => m.Column("Title"));
            Property(x => x.Ord);
        }
    }
}
