using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class CriteriaGroupMap : ClassMapping<CriteriaGroup>
    {
        public CriteriaGroupMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            Property(x => x.Description, m =>
            {
                m.NotNullable(true);
                m.Length(Length.CrGrDescr);
            });
            Set(x => x.Criteria, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.CriteriaGroup);
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
                m.Column(Names.Id.Estimator);
                m.NotNullable(true);
            });
        }
    }
}