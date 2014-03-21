﻿using Diagnosis.Models;
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

            Property(x => x.FirstName);
            Property(x => x.MiddleName);
            Property(x => x.LastName);
            Property(x => x.IsMale);
            Property(x => x.Speciality);

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
        }
    }
}