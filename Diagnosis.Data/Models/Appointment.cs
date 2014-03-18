using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public class Appointment
    {
        ISet<HealthRecord> healthRecords = new HashSet<HealthRecord>();

        public virtual int Id { get; protected set; }
        public virtual Course Course { get; set; }
        public virtual Doctor Doctor { get; set; }
        public virtual DateTime DateTime { get; set; }
        public virtual ReadOnlyCollection<HealthRecord> HealthRecords
        {
            get
            {
                return new ReadOnlyCollection<HealthRecord>(
                    new List<HealthRecord>(healthRecords));
            }
        }
    }
}
