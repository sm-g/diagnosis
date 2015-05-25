using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class EstimatorMap : ClassMapping<Estimator>
    {
        public EstimatorMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            Property(x => x.Description, m =>
            {
                m.NotNullable(true);
                m.Length(Length.EstimatorDescr);
            });
            Property(x => x.HeaderHrsOptions, m =>
            {
                m.NotNullable(false);
                m.Column(col => col.SqlType("ntext"));
            });
            Set(x => x.CriteriaGroups, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.Estimator);
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