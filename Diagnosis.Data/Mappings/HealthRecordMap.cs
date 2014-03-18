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

            Property(x => x.Description);

            ManyToOne(x => x.Appointment, m =>
            {
                m.Column("AppointmentID");
            });
            ManyToOne(x => x.Diagnosis, m =>
            {
                m.Column("DiagnosisID");
            });
            Set(x => x.Symptoms, s =>
            {
                s.Table("RecordSymptoms");
                s.Key(k =>
                {
                    k.Column("RecordID");
                });
                s.Access(Accessor.Field);
            }, r =>
            {
                r.ManyToMany(x =>
                {
                    x.Column("SymptomID");
                    x.Class(typeof(Symptom));
                });
            });
        }
    }
}
