using Diagnosis.Models;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;

namespace Diagnosis.Data.Repositories
{
    public class PropertyValueRepository : ModelRepository<PropertyValue>, IPropertyValueRepository
    {
        public PropertyValue GetByValue(string value)
        {
            ISession session = NHibernateHelper.GetSession();
            {
                PropertyValue PropertyValue = session
                    .CreateCriteria(typeof(PropertyValue))
                    .Add(Restrictions.Eq("Title", value))
                    .UniqueResult<PropertyValue>();
                return PropertyValue;
            }
        }
    }
}
