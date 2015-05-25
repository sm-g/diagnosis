using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class IcdDiseaseMap : ClassMapping<IcdDisease>
    {
        public IcdDiseaseMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.Native);
            });

            Property(x => x.Title, m =>
            {
                m.NotNullable(true);
                m.UniqueKey(Names.Unique.DiseaseCode);
                m.Length(Length.IcdDiseaseTitle);
            });
            Property(x => x.Code, m =>
            {
                m.NotNullable(true);
                m.Length(Length.IcdCode);
            });

            ManyToOne(x => x.IcdBlock, m =>
            {
                m.Column(Names.Id.IcdBlock);
                m.NotNullable(true);
            });
        }
    }
}