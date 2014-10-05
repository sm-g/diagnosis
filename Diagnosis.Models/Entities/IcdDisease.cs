using System.Collections.Generic;
using System.Collections.ObjectModel;
using Iesi.Collections.Generic;

namespace Diagnosis.Models
{
    public class IcdDisease : EntityBase, IDomainEntity, IHrItemObject
    {
        Iesi.Collections.Generic.ISet<HealthRecord> healthRecords;

        public virtual IcdBlock IcdBlock { get; protected set; }
        public virtual string Title { get; set; }
        public virtual string Code { get; set; }
        public virtual IEnumerable<HealthRecord> HealthRecords
        {
            get
            {
                return healthRecords;
            }
        }

        protected IcdDisease() { }
    }
}
