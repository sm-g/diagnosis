using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class VocabularyWordsMap : ClassMapping<VocabularyWords>
    {
        public VocabularyWordsMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            ManyToOne(x => x.Word, m =>
            {
                m.Column(Names.Id.Word);
                m.NotNullable(true);
                m.Cascade(Cascade.Persist);
            });
            ManyToOne(x => x.Vocabulary, m =>
            {
                m.Column(Names.Id.Vocabulary);
                m.NotNullable(true);
                m.Cascade(Cascade.Persist);
            });
        }
    }
}