using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Diagnosis.Models;

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

            Property(x => x.FirstName);
            Property(x => x.MiddleName);
            Property(x => x.LastName);
            Property(x => x.IsMale);
            Property(x => x.BirthDate);
            Property(x => x.SNILS);
            Set(x => x.PatientProperties, s =>
            {
                s.Key(k =>
                {
                    k.Column("PatientID");
                });
                s.Access(Accessor.Field);
                s.Cascade(Cascade.All);
                s.Inverse(true);
            }, r =>
            {
                r.OneToMany();
            });
        }
    }
}
