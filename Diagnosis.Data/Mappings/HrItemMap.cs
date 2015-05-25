using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;

namespace Diagnosis.Data.Mappings
{
    public class HrItemMap : ClassMapping<HrItem>
    {
        public HrItemMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            Property(x => x.Ord, m =>
            {
                m.Column(c =>
                {
                    c.Default(0);
                });
                m.NotNullable(true);
            });

            Component(x => x.Comment, x =>
            {
                x.Property(y => y.String, m =>
                {
                    m.Column(Names.Col.HrItemTextRepr);
                    m.Length(Length.Comment);
                });
            });

            Property(x => x.Confidence, m => m.Type<EnumStringType<Confidence>>());
            ManyToOne(x => x.Word, m =>
            {
                m.Column(Names.Id.Word);
                m.Cascade(Cascade.Persist);
            });

            Component(x => x.Measure, m =>
            {
                m.Property(x => x.DbValue, x =>
                {
                    x.Column(Names.Col.HrItemMeasure);
                    x.Precision(Types.Numeric.Precision);
                    x.Scale(Types.Numeric.Scale);
                });
                m.ManyToOne(x => x.Uom, x =>
                {
                    x.Column(Names.Id.Uom);
                });
            });
            ManyToOne(x => x.Disease, m =>
            {
                m.Column(Names.Id.IcdDisease);
            });

            ManyToOne(x => x.HealthRecord, m =>
            {
                m.Column(Names.Id.HealthRecord);
                m.NotNullable(true);
            });
        }
    }
}