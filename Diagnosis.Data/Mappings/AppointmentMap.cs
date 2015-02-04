using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class AppointmentMap : ClassMapping<Appointment>
    {
        public AppointmentMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            Property(x => x.DateAndTime, m =>
            {
                m.NotNullable(true);
            });
            Property(x => x.CreatedAt, m =>
            {
                m.NotNullable(true);
                m.Column(c =>
                {
                    c.Default(Helper.SqlDateTimeNow);
                });
            });
            Property(x => x.UpdatedAt, m =>
            {
                m.NotNullable(true);
                m.Column(c =>
                {
                    c.Default(Helper.SqlDateTimeNow);
                });
            });

            Set(x => x.HealthRecords, s =>
            {
                s.Key(k =>
                {
                    k.Column("AppointmentID");
                });
                s.Inverse(true);
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
            ManyToOne(x => x.Course, m =>
            {
                m.Column("CourseID");
                m.Cascade(Cascade.Persist);
                m.NotNullable(true);
            });
            ManyToOne(x => x.Doctor, m =>
            {
                m.Column("DoctorID");
                m.NotNullable(true);
            });
        }
    }
}