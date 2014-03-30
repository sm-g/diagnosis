using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class SpecialityMap : ClassMapping<Speciality>
    {
        public SpecialityMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.Native);
            });

            Property(x => x.Title);

            Set(x => x.Doctors, s =>
            {
                s.Key(k =>
                {
                    k.Column("SpecialityID");
                });
                s.Inverse(true);
                s.Cascade(Cascade.All);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
            Set(x => x.IcdBlocks, s =>
            {
                s.Table("SpecialityIcdBlocks");
                s.Key(k =>
                {
                    k.Column("SpecialityID");
                });
                s.Cascade(Cascade.All);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.ManyToMany(x =>
                {
                    x.Column("BlockID");
                    x.Class(typeof(IcdBlock));
                });
            });
        }
    }
}
