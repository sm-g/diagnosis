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
                    c.Default(MappingHelper.SqlDateTimeNow);
                });
            });
            Property(x => x.UpdatedAt, m =>
            {
                m.NotNullable(true);
                m.Column(c =>
                {
                    c.Default(MappingHelper.SqlDateTimeNow);
                });
            });

            Set(x => x.HealthRecords, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.Appointment);
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
                m.Column(Names.Id.Course);
                m.Cascade(Cascade.Persist);
                m.NotNullable(true);
            });
            ManyToOne(x => x.Doctor, m =>
            {
                m.Column(Names.Id.Doctor);
                m.NotNullable(true);
            });
        }
    }
}