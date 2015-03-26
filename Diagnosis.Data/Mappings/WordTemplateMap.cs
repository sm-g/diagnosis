using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class WordTemplateMap : ClassMapping<WordTemplate>
    {
        public WordTemplateMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            Property(x => x.Title, m =>
            {
                m.NotNullable(true);
                m.Length(100);
            });

            ManyToOne(x => x.Vocabulary, m =>
            {
                m.Column("VocabularyID");
                m.NotNullable(true);
            });

        }
    }
}