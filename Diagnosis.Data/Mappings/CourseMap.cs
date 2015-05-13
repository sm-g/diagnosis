using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class CourseMap : ClassMapping<Course>
    {
        public CourseMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            Property(x => x.Start, m =>
            {
                m.Column(Names.Col.CourseStart);
                m.NotNullable(true);
            });
            Property(x => x.End, m => m.Column(Names.Col.CourseEnd));
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

            Set(x => x.Appointments, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.Course);
                });
                s.Inverse(true);
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
            Set(x => x.HealthRecords, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.Course);
                });
                s.Inverse(true);
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });

            ManyToOne(x => x.Patient, m =>
            {
                m.Column(Names.Id.Patient);
                m.NotNullable(true);
            });
            ManyToOne(x => x.LeadDoctor, m =>
            {
                m.Column(Names.Id.Doctor);
                m.NotNullable(true);
            });
        }
    }
}