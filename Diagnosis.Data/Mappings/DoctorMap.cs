using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings.Client
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

            ManyToOne(x => x.CustomVocabulary, m =>
            {
                m.Column(Names.Col.DoctorCustomVocabulary);
                m.Cascade(Cascade.All | Cascade.DeleteOrphans);
                m.Access(Accessor.Field);
            });

            Property(x => x.FirstName, m =>
            {
                m.Length(Length.DoctorFn);
            });
            Property(x => x.MiddleName, m =>
            {
                m.Length(Length.DoctorMn);
            });
            Property(x => x.LastName, m =>
            {
                m.NotNullable(true);
                m.Length(Length.DoctorLn);
            });
            Property(x => x.IsMale);

            Set(x => x.Appointments, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.Doctor);
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
                    k.Column(Names.Id.Doctor);
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
                    k.Column(Names.Id.Doctor);
                });
                s.Inverse(true);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
            ManyToOne(x => x.Speciality, m =>
            {
                m.Column(Names.Id.Speciality);
            });
            Set(x => x.SettingsSet, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.Doctor);
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

namespace Diagnosis.Data.Mappings.Server
{
    public class DoctorMap : ClassMapping<Doctor>
    {
        public DoctorMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            Property(x => x.FirstName, m =>
            {
                m.Length(Length.DoctorFn);
            });
            Property(x => x.MiddleName, m =>
            {
                m.Length(Length.DoctorMn);
            });
            Property(x => x.LastName, m =>
            {
                m.NotNullable(true);
                m.Length(Length.DoctorLn);
            });
            Property(x => x.IsMale);

            Set(x => x.Appointments, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.Doctor);
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
                    k.Column(Names.Id.Doctor);
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
                    k.Column(Names.Id.Doctor);
                });
                s.Inverse(true);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
            ManyToOne(x => x.Speciality, m =>
            {
                m.Column(Names.Id.Speciality);
            });
        }
    }
}