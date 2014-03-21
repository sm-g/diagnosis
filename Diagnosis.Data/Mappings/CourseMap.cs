﻿using Diagnosis.Models;
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
                m.Generator(Generators.Native);
            });

            Property(x => x.Start, m => m.Column("StartDate"));
            Property(x => x.End, m => m.Column("EndDate"));
            Set(x => x.Appointments, s =>
            {
                s.Key(k =>
                {
                    k.Column("CourseID");
                });
                s.Inverse(true);
                s.Cascade(Cascade.All);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
            ManyToOne(x => x.Patient, m =>
            {
                m.Column("PatientID");
            });
            ManyToOne(x => x.LeadDoctor, m =>
            {
                m.Column("DoctorID");
            });
        }
    }
}