using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class DiagnosisMap : ClassMapping<Diagnosis.Models.Diagnosis>
    {
        public DiagnosisMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.Native);
            });

            Property(x => x.Title);
            Property(x => x.Code);

            Set(x => x.HealthRecords, s =>
            {
                s.Key(k =>
                {
                    k.Column("DiagnosisID");
                });
                s.Inverse(true);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
            Set(x => x.Children, s =>
            {
                s.Key(k =>
                {
                    k.Column("ID");
                });
                s.Inverse(true);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
            ManyToOne(x => x.Parent, m =>
            {
                m.Column("ParentID");
            });
        }
    }
}
