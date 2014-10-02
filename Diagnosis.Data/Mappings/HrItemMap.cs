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
                m.Generator(Generators.Native);
            });

            Property(x => x.Order, m => m.Column("Ord"));

            ManyToOne(x => x.Word, m =>
            {
                m.Column("WordID");
            });

            Component(x => x.Measure, m =>
            {
                m.Property(x => x.DbValue, x => x.Column("Val"));
                m.ManyToOne(x => x.Uom, x =>
                {
                    x.Column("UomID");
                });
            });
            ManyToOne(x => x.Disease, m =>
            {
                m.Column("DiseaseID");
            });

            ManyToOne(x => x.HealthRecord, m =>
            {
                m.Column("HealthRecordID");
                m.NotNullable(true);
            });
        }
    }
}
