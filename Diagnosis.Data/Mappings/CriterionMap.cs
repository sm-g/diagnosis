using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class CriterionMap : ClassMapping<Criterion>
    {
        public CriterionMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            Property(x => x.Description, m =>
            {
                m.NotNullable(true);
                m.Length(2000);
            });
            Property(x => x.Code, m =>
            {
                m.NotNullable(true);
                m.Length(50);
            });
            Property(x => x.Options, m =>
            {
                m.NotNullable(true);
                m.Column(col => col.SqlType("ntext"));
            });
            Property(x => x.Value, m =>
            {
                m.NotNullable(true);
                m.Length(50);
            });

            ManyToOne(x => x.Group, m =>
            {
                m.Column(Names.Id.CriteriaGroup);
                m.NotNullable(true);
            });
        }
    }
}