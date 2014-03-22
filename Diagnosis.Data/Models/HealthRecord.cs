using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;

namespace Diagnosis.Models
{
    public class HealthRecord
    {
        ISet<Symptom> symptoms = new HashSet<Symptom>();

        public virtual int Id { get; protected set; }
        public virtual Appointment Appointment { get; protected set; }
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

        public virtual void AddSymptom(Symptom symptom)
        {
            Contract.Requires(symptom != null);
            symptoms.Add(symptom);
        }

        public virtual void RemoveSymptom(Symptom symptom)
        {
            Contract.Requires(symptom != null);
            symptoms.Remove(symptom);
        }

        public HealthRecord(Appointment appointment)
        {
            Contract.Requires(appointment != null);

            Appointment = appointment;
        }

        protected HealthRecord() { }
    }
}
