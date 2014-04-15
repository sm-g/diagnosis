using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class HealthRecordMap : ClassMapping<HealthRecord>
    {
        public HealthRecordMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.Native);
            });

            Property(x => x.Comment);
            Property(x => x.FromYear);
            Property(x => x.FromMonth);
            Property(x => x.FromDay);
            Property(x => x.NumValue);

            ManyToOne(x => x.Appointment, m =>
            {
                m.Column("AppointmentID");
            });
            ManyToOne(x => x.Disease, m =>
            {
                m.Column("DiseaseID");
            });
            ManyToOne(x => x.Symptom, m =>
            {
                m.Column("SymptomID");
                m.Cascade(Cascade.All);
            });
            ManyToOne(x => x.Category, m =>
            {
                m.Column("CategoryID");
            });
        }
    }
}
