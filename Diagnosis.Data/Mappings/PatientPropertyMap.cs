using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class PatientRecordPropertyMap : ClassMapping<PatientRecordProperty>
    {
        public PatientRecordPropertyMap()
        {
            Table("PatientRecordProperties");
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.Native);
            });

            ManyToOne(x => x.Patient, m =>
            {
                m.Column("PatientID");
            });
            ManyToOne(x => x.Property, m =>
            {
                m.Column("PropertyID");
            });
            ManyToOne(x => x.Value, m =>
            {
                m.Column("ValueID");
            });
            ManyToOne(x => x.HealthRecord, m =>
            {
                m.Column("RecordID");
            });
        }
    }
}
