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
                m.Column("StartDate");
                m.NotNullable(true);
            });
            Property(x => x.End, m => m.Column("EndDate"));

            Set(x => x.Appointments, s =>
            {
                s.Key(k =>
                {
                    k.Column("CourseID");
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
                    k.Column("CourseID");
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
                m.Column("PatientID");
                m.NotNullable(true);
            });
            ManyToOne(x => x.LeadDoctor, m =>
            {
                m.Column("DoctorID");
                m.NotNullable(true);
            });
        }
    }
}
