using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class PatientMap : ClassMapping<Patient>
    {
        public PatientMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
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
                m.Length(20);
            });
            Property(x => x.IsMale);
            Property(x => x.BirthYear);
            Property(x => x.BirthMonth);
            Property(x => x.BirthDay);
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

            Set(x => x.Courses, s =>
            {
                s.Key(k =>
                {
                    k.Column("PatientID");
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
                    k.Column("PatientID");
                });
                s.Inverse(true);
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
        }
    }
}
