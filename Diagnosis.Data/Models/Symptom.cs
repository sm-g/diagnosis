using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class Symptom
    {
        public virtual int Id { get; protected set; }
        public virtual string Title { get; set; }
        public virtual byte Priority { get; set; }

        public Symptom(string title)
        {
            Contract.Requires(title != null);
            Contract.Requires(title.Length > 0);
            Title = title;
        }

        public Symptom() { }
    }
}
