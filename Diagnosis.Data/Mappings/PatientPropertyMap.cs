using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Diagnosis.Models;

namespace Diagnosis.Data.Mappings
{
    public class PatientPropertyMap : ClassMapping<PatientProperty>
    {
        public PatientPropertyMap()
        {
            Table("PatientProperties");
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.Native);
            });

            ManyToOne(x => x.Patient, m =>
            {
                m.Column("PatientID");
            });
            ManyToOne(x => x.Property, m =>
            {
                m.Column("PropertyID");
            });
            ManyToOne(x => x.Value, m =>
            {
                m.Column("ValueID");
            });
        }
    }
}
