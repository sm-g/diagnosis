using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class CriteriaGroupMap : SubclassMapping<CriteriaGroup>
    {
        public CriteriaGroupMap()
        {
            DiscriminatorValue("CriteriaGroup");

            Set(x => x.Criteria, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.CritParent);
                });
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
                s.Inverse(true);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
            ManyToOne(x => x.Estimator, m =>
            {
                m.Column(Names.Id.CritParent);
             //   m.NotNullable(true);
            });
        }
    }
}