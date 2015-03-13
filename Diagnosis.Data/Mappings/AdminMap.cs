using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings
{
    // there is no Admin table

    //public class AdminMap : ClassMapping<Admin>
    //{
    //    public AdminMap()
    //    {
    //        Id(x => x.Id, m =>
    //        {
    //            m.Generator(Generators.Assigned);
    //        });

    //        OneToOne(x => x.Passport, m =>
    //        {
    //            m.Cascade(Cascade.All | Cascade.DeleteOrphans);
    //            m.Access(Accessor.ReadOnly);
    //            m.Constrained(true);
    //        });

    //        Property(x => x.FirstName, m =>
    //        {
    //            m.Length(20);
    //        });
    //        Property(x => x.MiddleName, m =>
    //        {
    //            m.Length(20);
    //        });
    //        Property(x => x.LastName, m =>
    //        {
    //            m.NotNullable(true);
    //            m.Length(20);
    //        });
    //    }
    //}
}