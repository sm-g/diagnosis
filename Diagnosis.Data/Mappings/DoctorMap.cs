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
                m.Generator(Generators.Native);
            });

            Property(x => x.FirstName, m => m.NotNullable(true));
            Property(x => x.MiddleName);
            Property(x => x.LastName);
            Property(x => x.IsMale);
            Property(x => x.Settings, m => m.NotNullable(true));

            Set(x => x.Courses, s =>
            {
                s.Key(k =>
                {
                    k.Column("DoctorID");
                });
                s.Inverse(true);
                s.Cascade(Cascade.All);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
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

            ManyToOne(x => x.Speciality, m =>
            {
                m.Column("SpecialityID");
            });
        }
    }
}
