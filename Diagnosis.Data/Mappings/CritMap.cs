using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class CritMap : ClassMapping<Crit>
    {
        public CritMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            Property(x => x.Description, m =>
            {
                m.NotNullable(true);
                m.Length(Length.CriterionDescr);
            });
            Discriminator(x =>
            {
                x.Column(Names.Col.CritType);
            });
        }
    }
}