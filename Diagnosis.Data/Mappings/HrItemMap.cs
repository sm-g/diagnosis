using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class HrItemMap : ClassMapping<HrItem>
    {
        public HrItemMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            Property(x => x.Ord, m =>
            {
                m.Column(c =>
                {
                    c.Default(0);
                });
                m.NotNullable(true);

            });
            Property(x => x.TextRepr, m =>
            {
                m.Length(255);
            });
            ManyToOne(x => x.Word, m =>
            {
                m.Column("WordID");
                m.Cascade(Cascade.Persist);
            });

            Component(x => x.Measure, m =>
            {
                m.Property(x => x.DbValue, x => x.Column("MeasureValue"));
                m.ManyToOne(x => x.Uom, x =>
                {
                    x.Column("UomID");
                });
            });
            ManyToOne(x => x.Disease, m =>
            {
                m.Column("IcdDiseaseID");
            });

            ManyToOne(x => x.HealthRecord, m =>
            {
                m.Column("HealthRecordID");
                m.NotNullable(true);
            });
        }
    }
}
