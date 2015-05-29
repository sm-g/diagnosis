using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class CriterionMap : SubclassMapping<Criterion>
    {
        public CriterionMap()
        {
            DiscriminatorValue(Names.Col.CritDiscriminator);
            Property(x => x.Code, m =>
            {
                //m.NotNullable(true);
                m.Length(Length.CriterionCode);
            });
            
            Property(x => x.Value, m =>
            {
                //m.NotNullable(true);
                m.Length(Length.CriterionValue);
            });

            ManyToOne(x => x.Group, m =>
            {
                m.Column(Names.Id.CritParent);
                //   m.NotNullable(true);
            });
        }
    }
}