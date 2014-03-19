using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class Diagnosis
    {
        ISet<HealthRecord> healthRecords = new HashSet<HealthRecord>();
        ISet<Diagnosis> children = new HashSet<Diagnosis>();

        public virtual int Id { get; protected set; }
        public virtual string Title { get; set; }
        public virtual string Code { get; set; }
        public virtual Diagnosis Parent { get; set; }
        public virtual ReadOnlyCollection<HealthRecord> HealthRecords
        {
            get
            {
                return new ReadOnlyCollection<HealthRecord>(
                    new List<HealthRecord>(healthRecords));
            }
        }
        public virtual ReadOnlyCollection<Diagnosis> Children
        {
            get
            {
                return new ReadOnlyCollection<Diagnosis>(
                    new List<Diagnosis>(children));
            }
        }

        public Diagnosis(string code, string title, Diagnosis parent = null)
        {
            Contract.Requires(!string.IsNullOrEmpty(title));
            Contract.Requires(code != null && code.Length == 5);

            Code = code;
            Title = title;
            Parent = parent;
        }

        public Diagnosis() { }
    }
}
