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
                m.Generator(Generators.Native);
            });

            Property(x => x.DateAndTime);
            Set(x => x.HealthRecords, s =>
            {
                s.Key(k =>
                {
                    k.Column("AppointmentID");
                });
                s.Inverse(true);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
            ManyToOne(x => x.Course, m =>
            {
                m.Column("CourseID");
            });
            ManyToOne(x => x.Doctor, m =>
            {
                m.Column("DoctorID");
            });
        }
    }
}
