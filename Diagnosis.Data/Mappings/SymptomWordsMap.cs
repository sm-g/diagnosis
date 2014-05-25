using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Diagnosis.Models;

namespace Diagnosis.Data.Mappings
{
    public class SymptomWordsMap : ClassMapping<SymptomWords>
    {
        public SymptomWordsMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.Native);
            });

            ManyToOne(x => x.Symptom, m =>
            {
                m.Column("SymptomID");
            });
            ManyToOne(x => x.Word, m =>
            {
                m.Column("WordID");
            });
        }
    }
}
