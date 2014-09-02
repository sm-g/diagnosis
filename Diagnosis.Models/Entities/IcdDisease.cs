using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Diagnosis.Models
{
    public class IcdDisease : EntityBase
    {
        ISet<HealthRecord> healthRecords = new HashSet<HealthRecord>();
        ISet<Symptom> symptoms = new HashSet<Symptom>();

        public virtual IcdBlock IcdBlock { get; protected set; }
        public virtual string Title { get; set; }
        public virtual string Code { get; set; }
        public virtual ReadOnlyCollection<HealthRecord> HealthRecords
        {
            get
            {
                return new ReadOnlyCollection<HealthRecord>(
                    new List<HealthRecord>(healthRecords));
            }
        }
        public virtual ReadOnlyCollection<Symptom> Symptoms
        {
            get
            {
                return new ReadOnlyCollection<Symptom>(
                    new List<Symptom>(symptoms));
            }
        }

        protected IcdDisease() { }
    }
}
