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
                m.Generator(Generators.Native);
            });

            Property(x => x.Label);
            Property(x => x.FirstName);
            Property(x => x.MiddleName);
            Property(x => x.LastName);
            Property(x => x.IsMale);
            Property(x => x.BirthYear);
            Property(x => x.BirthMonth);
            Property(x => x.BirthDay);

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
        }
    }
}
