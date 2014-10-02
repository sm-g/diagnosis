using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    //public class MeasureMap : ClassMapping<Measure>
    //{
    //    public MeasureMap()
    //    {
    //        Id(x => x.Id, m =>
    //        {
    //            m.Generator(Generators.Native);
    //        });
    //        Property(x => x.Order, m =>
    //        {
    //            m.Column("Ord");
    //            m.NotNullable(true);
    //        });
    //        Property(x => x.DbValue, m =>
    //        {
    //            m.Column("Val");
    //            m.NotNullable(true);
    //        });
    //        ManyToOne(x => x.Uom, m =>
    //        {
    //            m.Column("UomID");
    //        });
    //        ManyToOne(x => x.HealthRecord, m =>
    //        {
    //            m.Column("HealthRecordID");
    //            m.NotNullable(true);
    //        });
    //    }
    //}
}
