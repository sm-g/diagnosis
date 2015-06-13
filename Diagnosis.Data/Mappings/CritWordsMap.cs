using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings.Client
{
    public class CritWordsMap : ClassMapping<CritWords>
    {
        public CritWordsMap()
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
            ManyToOne(x => x.Crit, m =>
            {
                m.Column(Names.Id.Crit);
                m.NotNullable(true);
                m.Cascade(Cascade.Persist);
            });
        }
    }
}