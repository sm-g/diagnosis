using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    public class SettingMap : ClassMapping<Setting>
    {
        public SettingMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            Property(x => x.Title, m =>
            {
                m.NotNullable(true);
            });
            Property(x => x.Value, m =>
            {
                m.NotNullable(true);
            });
            ManyToOne(x => x.Doctor, m =>
            {
                m.Column("DoctorID");
            });


        }
    }
}
