using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;

namespace Diagnosis.Models
{
    public class HealthRecord
    {
        ISet<Symptom> symptoms = new HashSet<Symptom>();
        ISet<PatientRecordProperty> recordProperties = new HashSet<PatientRecordProperty>();

        public virtual int Id { get; protected set; }
        public virtual Appointment Appointment { get; protected set; }
        public virtual string Comment { get; set; }
        public virtual byte Category { get; set; }
        public virtual Diagnosis Diagnosis { get; set; }
        public virtual IcdDisease IcdDisease { get; set; }
        public virtual ReadOnlyCollection<Symptom> Symptoms
        {
            get
            {
                return new ReadOnlyCollection<Symptom>(
                    new List<Symptom>(symptoms));
            }
        }
        public virtual ReadOnlyCollection<PatientRecordProperty> RecordProperties
        {
            get
            {
                return new ReadOnlyCollection<PatientRecordProperty>(
                    new List<PatientRecordProperty>(recordProperties));
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

        public HealthRecord(Appointment appointment, byte category = 0)
        {
            Contract.Requires(appointment != null);

            Appointment = appointment;
            Category = category;
        }

        protected HealthRecord() { }
    }
}
