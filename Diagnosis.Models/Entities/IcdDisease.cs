using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Diagnosis.Models
{
    public class IcdDisease : EntityBase, IDomainEntity
    {
        ISet<HealthRecord> healthRecords = new HashSet<HealthRecord>();
        ISet<Symptom> symptoms = new HashSet<Symptom>();

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
