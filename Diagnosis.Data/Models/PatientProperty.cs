using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;
using System.Text;

namespace Diagnosis.Models
{
    public class PatientProperty
    {
        public virtual int Id { get; protected set; }
        public virtual Patient Patient { get; set; }
        public virtual PropertyValue Value { get; set; }
        public virtual Property Property { get; set; }

        public PatientProperty(Patient patient, Property property, PropertyValue value)
        {
            Contract.Requires(patient != null);
            Contract.Requires(property != null);
            Contract.Requires(value != null);

            Patient = patient;
            Property = property;
            Value = value;
        }

        protected PatientProperty() { }
    }
}
