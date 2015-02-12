using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class HrCategoryMap : ClassMapping<HrCategory>
    {
        public HrCategoryMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            Property(x => x.Name, m =>
            {
                m.Column("Title");
                m.NotNullable(true);
                m.Length(20);
            });
            Property(x => x.Ord, m =>
            {
                m.NotNullable(true);
            });
        }
    }
}