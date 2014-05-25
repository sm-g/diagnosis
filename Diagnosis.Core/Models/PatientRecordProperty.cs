using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;
using System.Text;

namespace Diagnosis.Models
{
    public class PatientRecordProperty
    {
        public virtual int Id { get; protected set; }
        public virtual Patient Patient { get; protected set; }
        public virtual PropertyValue Value { get; set; }
        public virtual Property Property { get; protected set; }
        public virtual HealthRecord HealthRecord { get; protected set; }

        public PatientRecordProperty(Property property, PropertyValue value, Patient patient = null, HealthRecord hr = null)
        {
            Contract.Requires(patient != null || hr != null);
            Contract.Requires(property != null);
            Contract.Requires(value != null);

            Patient = patient;
            Property = property;
            Value = value;
            HealthRecord = hr;
        }

        protected PatientRecordProperty() { }
    }
}
