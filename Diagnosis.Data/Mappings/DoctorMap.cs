using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class DoctorMap : ClassMapping<Doctor>
    {
        public DoctorMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            OneToOne(x => x.Passport, m =>
            {
                m.Cascade(Cascade.All | Cascade.DeleteOrphans);
                m.Access(Accessor.Field);
                m.Constrained(true);
            });

            Property(x => x.FirstName, m =>
            {
                m.Length(20);
            });
            Property(x => x.MiddleName, m =>
            {
                m.Length(20);
            });
            Property(x => x.LastName, m =>
            {
                m.NotNullable(true);
                m.Length(20);
            });
            Property(x => x.IsMale);

            Set(x => x.Appointments, s =>
            {
                s.Key(k =>
                {
                    k.Column("DoctorID");
                });
                s.Inverse(true);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
            Set(x => x.Courses, s =>
            {
                s.Key(k =>
                {
                    k.Column("DoctorID");
                });
                s.Inverse(true);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
            Set(x => x.HealthRecords, s =>
            {
                s.Key(k =>
                {
                    k.Column("DoctorID");
                });
                s.Inverse(true);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
            ManyToOne(x => x.Speciality, m =>
            {
                m.Column("SpecialityID");
            });
            Set(x => x.SettingsSet, s =>
            {
                s.Key(k =>
                {
                    k.Column("DoctorID");
                });
                s.Inverse(true);
                s.Access(Accessor.Field);
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
            }, r =>
            {
                r.OneToMany();
            });
        }
    }
}