using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Models
{
    public class Property : EntityBase, IDomainEntity
    {
        ISet<PropertyValue> values = new HashSet<PropertyValue>();
        ISet<PatientRecordProperty> patientProperties = new HashSet<PatientRecordProperty>();

        public virtual string Title { get; set; }
        public virtual IEnumerable<PropertyValue> Values
        {
            get
            {
                return values;
            }
        }
        public virtual IEnumerable<PatientRecordProperty> PatientProperties
        {
            get
            {
                return patientProperties;
            }
        }

        public virtual void AddValue(string value)
        {
            Contract.Requires(!String.IsNullOrEmpty(value));

            var pv = new PropertyValue(this, value);

            if (!values.Contains(pv, PropertyValue.equalityComparer))
            {
                values.Add(pv);
            }
        }

        public virtual void RemoveValue(PropertyValue value)
        {
            values.Remove(value);
        }

        public Property(string title)
        {
            Contract.Requires(!String.IsNullOrEmpty(title));
            Title = title;
        }
        protected Property()
        {
        }

        public override string ToString()
        {
            return Title;
        }
    }
}