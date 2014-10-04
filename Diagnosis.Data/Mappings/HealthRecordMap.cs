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
            Property(x => x.IsDeleted, m => m.NotNullable(true));
            Property(x => x.Unit, m => m.Type<NHibernate.Type.EnumStringType<HealthRecordUnits>>());

            Set(x => x.HrItems, s =>
            {
                s.Key(k =>
                {
                    k.Column("HealthRecordID");
                });
                s.Inverse(true);
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
            ManyToOne(x => x.Appointment, m =>
            {
                m.Column("AppointmentID");
                m.Cascade(Cascade.Persist);
            });
            ManyToOne(x => x.Course, m =>
            {
                m.Column("CourseID");
            });
            ManyToOne(x => x.Patient, m =>
            {
                m.Column("PatientID");
            });
            ManyToOne(x => x.Category, m =>
            {
                m.Column("HrCategoryID");
            });
        }
    }
}
