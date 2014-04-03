using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Diagnosis.Models
{
    public class IcdDisease
    {
        ISet<HealthRecord> healthRecords = new HashSet<HealthRecord>();

        public virtual int Id { get; protected set; }
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

        protected IcdDisease() { }
    }
}
