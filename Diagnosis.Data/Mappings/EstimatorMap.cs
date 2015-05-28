using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class EstimatorMap : SubclassMapping<Estimator>
    {
        public EstimatorMap()
        {
            DiscriminatorValue("Estimator");


            Property(x => x.Options, m =>
            {
                //m.NotNullable(false);
                m.Column(col => col.SqlType("ntext"));
            });
            Set(x => x.CriteriaGroups, s =>
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
        }
    }
}