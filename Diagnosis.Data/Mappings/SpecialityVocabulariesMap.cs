using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class SpecialityVocabulariesMap : ClassMapping<SpecialityVocabularies>
    {
        public SpecialityVocabulariesMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            ManyToOne(x => x.Speciality, m =>
            {
                m.Column(Names.Id.Speciality);
                m.NotNullable(true);
            });
            ManyToOne(x => x.Vocabulary, m =>
            {
                m.Column(Names.Id.Vocabulary);
                m.NotNullable(true);
            });
        }
    }
}