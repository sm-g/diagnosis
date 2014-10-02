using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class IcdDiseaseMap : ClassMapping<IcdDisease>
    {
        public IcdDiseaseMap()
        {
            Table("Disease");

            Id(x => x.Id, m =>
            {
                m.Generator(Generators.Native);
            });

            Property(x => x.Title, m =>
            {
                m.NotNullable(true);
            });
            Property(x => x.Code, m =>
            {
                m.NotNullable(true);
            });

            Set(x => x.HealthRecords, s =>
            {
                s.Key(k =>
                {
                    k.Column("DiseaseID");
                });
                s.Inverse(true);
                s.Cascade(Cascade.All);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });

            ManyToOne(x => x.IcdBlock, m =>
            {
                m.Column("IcdBlockID");
                m.NotNullable(true);
            });
        }
    }
}
