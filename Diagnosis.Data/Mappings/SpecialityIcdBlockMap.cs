using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Diagnosis.Models;

namespace Diagnosis.Data.Mappings
{
    public class SpecialityIcdBlockMap : ClassMapping<SpecialityIcdBlock>
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
            });
            ManyToOne(x => x.IcdBlock, m =>
            {
                m.Column("IcdBlockID");
            });
        }
    }
}
