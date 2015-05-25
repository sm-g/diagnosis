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
                m.Length(Length.WordTitle);
            });

            ManyToOne(x => x.Vocabulary, m =>
            {
                m.Column(Names.Id.Vocabulary);
                m.NotNullable(true);
            });
        }
    }
}