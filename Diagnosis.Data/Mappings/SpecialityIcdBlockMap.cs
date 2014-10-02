﻿using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Diagnosis.Models;

namespace Diagnosis.Data.Mappings
{
    public class SpecialityIcdBlockMap : ClassMapping<SpecialityIcdBlocks>
    {
        public SpecialityIcdBlockMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.Native);
            });

            ManyToOne(x => x.Speciality, m =>
            {
                m.Column("SpecialityID");
                m.NotNullable(true);
            });
            ManyToOne(x => x.IcdBlock, m =>
            {
                m.Column("IcdBlockID");
                m.NotNullable(true);
            });
        }
    }
}
