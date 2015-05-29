using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class CritMap : ClassMapping<Crit>
    {
        public CritMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            Property(x => x.Description, m =>
            {
                m.NotNullable(true);
                m.Length(Length.CritDescr);
            });
            Property(x => x.Options, m =>
            {
                m.Column(col => col.SqlType(MappingHelper.SqlTypeNText));
            });
            Property(x => x.OptionsFormat, m =>
            {
                m.Length(Length.CritOptionsFormat);
            });
            Discriminator(x =>
            {
                x.Column(Names.Col.CritType);
            });

            Set(x => x.CritWords, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.Crit);
                });
                s.Inverse(true);
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
        }
    }
}