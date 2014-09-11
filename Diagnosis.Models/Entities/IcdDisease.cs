using System.Collections.Generic;
using System.Collections.ObjectModel;
using Iesi.Collections.Generic;

namespace Diagnosis.Models
{
    public class IcdDisease : EntityBase, IDomainEntity
    {
        Iesi.Collections.Generic.ISet<HealthRecord> healthRecords;
        Iesi.Collections.Generic.ISet<Symptom> symptoms;

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
        public virtual IEnumerable<Symptom> Symptoms
        {
            get
            {
                return symptoms;
            }
        }

        protected IcdDisease() { }
    }
}
