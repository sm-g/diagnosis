using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Diagnosis.Models
{
    public class HealthRecord
    {
        ISet<Symptom> symptoms = new HashSet<Symptom>();

        public virtual int Id { get; protected set; }
        public virtual Appointment Appointment { get; set; }
        public virtual string Description { get; set; }
        public virtual Diagnosis Diagnosis { get; set; }
        public virtual ReadOnlyCollection<Symptom> Symptoms
        {
            get
            {
                return new ReadOnlyCollection<Symptom>(
                    new List<Symptom>(symptoms));
            }
        }
    }
}
