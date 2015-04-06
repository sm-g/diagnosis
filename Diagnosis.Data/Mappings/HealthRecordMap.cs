using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;

namespace Diagnosis.Data.Mappings
{
    public class HealthRecordMap : ClassMapping<HealthRecord>
    {
        public HealthRecordMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            Component(x => x.FromDate, m =>
            {
                m.Property(x => x.Day, x =>
                {
                    x.Column(Names.Col.HrFromDay);
                });
                m.Property(x => x.Month, x =>
                {
                    x.Column(Names.Col.HrFromMonth);
                });
                m.Property(x => x.Year, x =>
                {
                    x.Column(Names.Col.HrFromYear);
                });
            });

            Property(x => x.IsDeleted, m =>
            {
                m.NotNullable(true);
                m.Column(c =>
                {
                    c.Default(0);
                });
            });
            Property(x => x.Ord, m =>
            {
                m.Column(c =>
                {
                    c.Default(0);
                });
                m.NotNullable(true);
            });
            Property(x => x.CreatedAt, m =>
            {
                m.NotNullable(true);
                m.Column(c =>
                {
                    c.Default(Helper.SqlDateTimeNow);
                });
            });
            Property(x => x.UpdatedAt, m =>
            {
                m.NotNullable(true);
                m.Column(c =>
                {
                    c.Default(Helper.SqlDateTimeNow);
                });
            });
            Property(x => x.Unit, m => m.Type<EnumStringType<HealthRecordUnit>>());

            Set(x => x.HrItems, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.HealthRecord);
                });
                s.Inverse(true);
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
            ManyToOne(x => x.Appointment, m =>
            {
                m.Column(Names.Id.Appointment);
                m.Cascade(Cascade.Persist);
            });
            ManyToOne(x => x.Course, m =>
            {
                m.Column(Names.Id.Course);
                m.Cascade(Cascade.Persist);
            });
            ManyToOne(x => x.Patient, m =>
            {
                m.Column(Names.Id.Patient);
                m.Cascade(Cascade.Persist);
            });
            ManyToOne(x => x.Doctor, m =>
            {
                m.Column(Names.Id.Doctor);
                m.NotNullable(true);
            });
            ManyToOne(x => x.Category, m =>
            {
                m.Column(Names.Id.HrCategory);
            });
        }
    }
}